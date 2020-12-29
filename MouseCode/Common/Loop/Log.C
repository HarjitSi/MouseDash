/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        		Log.C
*
* Written By:  	Harjit Singh
*
* Date:        	08/05/97, 02/17/10
*
* Purpose:     	This file contains data logging code
*
* Notes:       	The SPI flash data format was changed from 32 bit type to 8 bit type to
*				minimize the wasteage.
*
*				Location 0 of the page = 1's complement indicates how
*										many UByte data entries are in that page.
*										This way erased pages will have 0xff for
*										location 0 and that will tell us we have no
*										more info. in the flash or not.
*
*				Location 1 to 255: data payload
*
* To Be Done:
*
* Modification History:
* 09/21/10		HS		Added flash logging capability
* 06/10/12		HS		Changed from logging using 32 bit entries to 8 bit entries
*/
/*
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


#define LOG_C

#include <stdio.h>
#include <string.h>

#include "Common\Include\types.h"
#include "log.h"

#include "simcode\simcode.h"
#include "Common\flash\flash.h"
#include "Common\display\display.h"
#include "serial\serial.h"
#include "Common\Include\ZthInclude.h"
#include "Common\menu\menu.h"

#include <cross_studio_io.h>

// Logging has two modes: log to RAM or log to SPI flash.
//
// The mode is indicated by ldLogDevice. Since the modes are mutually exclusive,
// we use as many of the same variables for both.
//
// When we are logging to RAM, we store data in ubBufLog.  The size of this buffer
// is BUF_LOG_LEN.
//
// When we are logging to flash, we still use ubBufLog but use an additional buffer
// called ubSPIBufLog for writes to the SPI flash
//
// NOTE: The code assumes that the LOG_*_LEN constants are power of 2 values
//
#ifdef ENABLE_STEP_RESP
#define	BUF_LOG_LEN			(0x4000)	// number of entries for logging to RAM
#else
#define	BUF_LOG_LEN			(0x8000)	// number of entries for logging to RAM
#endif
#define	BUF_LOG_MSK			(BUF_LOG_LEN-1)
										// mask for pointer math

// make sure BUF_LOG_LEN is a power of two
#if (BUF_LOG_LEN & (BUF_LOG_LEN - 1))
#error BUF_LOG_LEN is not a power of two
#endif

#if (BUF_LOG_LEN != 0x8000)
#warning BUF_LOG_LEN != 0x8000
#endif

#define	LOG_SPI_BUF_LEN		(FLASH_SPI_BUF_LEN)
										// buffer for spi flash writes:
										// 1 byte for command
										// 3 bytes for address
										// 256 bytes for data payload
										// flash.c: vWriteFlash() uses this info.
#if LOG_SPI_BUF_LEN != 260
#error Logging and flash code hard wired and are different, so now wont work.
#endif

#define	ENTRIES_WRITE_PAGE	(SIZE_WRITE_PAGE/sizeof(UByte)-1)
										// number of data entries that we can put
										// in one page of the SPI flash
										// the -1 is because the first UByte is used
										// for the data entry count for this page

// This variable tells us if we are logging to the flash or the RAM
static LOG_DEVICE ldLogDevice = LD_LOG_TO_RAM;

// This variable tells us whether we are gathering data or accumulating it
static LOG_TYPE ltLogMode = LT_LOG_NONE;

enum eLogState {LS_IDLE, LS_LOGGING, LS_NEED_FLUSH, LS_FLUSHED} lsStateLog;

static ULong ulHeadLog, ulTailLog;			// how much data have we logged

static ULong ulFlashAddr;					// address to read/write

static ULong ulCountLog;					// how many entries have we logged

static ULong ulSizeLog;						// how much data can we log

static UByte ubBufLog[BUF_LOG_LEN];			// this is the buffer that is used to store
											// the log data. If we are logging to
											// flash, then it is copied from here
											// to the SPI flash buffer

static UByte ubSPIBufLog[LOG_SPI_BUF_LEN];
											// buffer for SPI flash transfers
											// This is used to hold data during
											// SPI transfers

#define GETLENBUFLOG	((ulHeadLog-ulTailLog)&BUF_LOG_MSK)

#define GETAVAILBUFLOG	(BUF_LOG_LEN-((ulHeadLog-ulTailLog)&BUF_LOG_MSK)-1)

#define PUTBYTEBUFLOG(x)	ubBufLog[ulHeadLog]=x;ulHeadLog++;ulHeadLog&=BUF_LOG_MSK

enum eWriteState {WS_IDLE, WS_WAIT_DO_STATUS_REG_READ, WS_SR_POLL} ewsWriteStateLog;

static Bool bFilterLog = FALSE;

static Bool bIsFlashOkay = FALSE;           // this is used to keep track of
                                            // whether vSetupLog succeeded in
                                            // setting up the flash or not

/**************************************************************************
* Routine: 		vFilterLogCmd
*
* Purpose: 		As various routines run, they call this routine to
*               create log events.
*
*               Some routines are in the same state for multiple mm,
*               so only log them once.
*
* Arguments:	Data to log
*
* RETURNS: 	    void
*
* Notes:        if logging data in non-interrupt code, be sure to disable
*               and re-enable interrupts!
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 12/04/2013    HarjitS     Created
*/
void vFilterLogCmd(UByte ubLogValue)
{
    //
    // init with LMCmdPrintString becase we need something to init this
    // variable with and because I can't think of why we would call
    // this routing with LMCmdPrintString.
    //
    static UByte ubPrevLogValue = LMCmdPrintString;

    if (ubLogValue != ubPrevLogValue)
    {
        // since we have a different command, we are no longer filtering
        bFilterLog = FALSE;

        vPutLog(ubLogValue);

        ubPrevLogValue = ubLogValue;
    }
    else
    {
        bFilterLog = TRUE;
    }

    return;
}

/**************************************************************************
* Routine: 		vLogCmd
*
* Purpose: 		As various routines run, they call this routine to
*               create log events.
*
* Arguments:	Data to log
*
* RETURNS: 	    void
*
* Notes:        if logging data in non-interrupt code, be sure to disable
*               and re-enable interrupts!
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 03/01/2014    HarjitS     Created from vFilterLogCmd()
*/
void vLogCmd(UByte ubLogValue)
{
    bFilterLog = FALSE;

    vPutLog(ubLogValue);

    return;
}

inline void vStartLog(void)
{
	lsStateLog = LS_LOGGING;

	return;
}

inline void vStopLog(void)
{
	if (LS_IDLE != lsStateLog)
	{
		if (LD_LOG_TO_RAM == ldLogDevice)
		{
			lsStateLog = LS_IDLE;
		}
		else
		{
			lsStateLog = LS_NEED_FLUSH;
		}
	}

	return;
}

inline Bool bIsDoneLog(void)
{
	return(LS_LOGGING != lsStateLog);
}

SLong slNum64KBlocksInUse(void)
{
    // figure out how many blocks are in use
    SLong slNumBlocks = ulSizeFlash / SIZE_ERASE_64K_BLOCK;

    SLong sl64KBlockCount = 0;

    for (SLong slAddrBlock = (slNumBlocks - 1) * SIZE_ERASE_64K_BLOCK;
            slAddrBlock >= 0;
            slAddrBlock -= SIZE_ERASE_64K_BLOCK)
    {
        UByte ubReadBuf;

        //
        // In the current logging scheme, if the first byte is 0xff
        // one's complement of 0, then we have reached the end of
        // the data.
        //
        // So, use that info. to stop erasing blocks that aren't used
        // and speed up the boot process.
        //
        vReadFlashBlock(slAddrBlock, (UByte *) &ubReadBuf, sizeof(UByte));

        // we are being clever by using 1's complement format to store
        // how many bytes are used in a page. We do this because the
        // erased value of a page is 0xff
        if (0xff != (UByte) ubReadBuf)
        {
            break;
        }

        sl64KBlockCount++;

        // show the number of blocks left
        vDispHexChar(0, BYTE0(slNumBlocks - sl64KBlockCount) >> 4);
        vDispHexChar(1, BYTE0(slNumBlocks - sl64KBlockCount) & 0x0f);

        // flash an 'S' on the display every other block i.e. 400ms to 700ms
        if (!(0x01 & sl64KBlockCount))
        {
            vDispChar('3', 'S');
        }
        else
        {
            vDispChar('3', ' ');
        }
    }

    return (slNumBlocks - sl64KBlockCount);
}

void vScanLog(void)
{
    // figure out how many blocks are in use
    slNum64KBlocksInUse();

    vDelayMS(500);

    return;
}

void vEraseLog(void)
{
    if (!bIsFlashOkay)
    {
        return;
    }

    // get how many blocks to erase
    SLong slNumBlocksToErase = slNum64KBlocksInUse();

    if (slNumBlocksToErase == 0)
    {
        return;
    }

    // Unprotect the flash
    vUnProtectFlashBlock();

    for (ULong ulAddrBlock = ulStartFlash; slNumBlocksToErase > 0; \
            ulAddrBlock += SIZE_ERASE_64K_BLOCK, slNumBlocksToErase--)
    {
         // show the number of blocks left
        vDispHexChar(0, BYTE0(slNumBlocksToErase) >> 4);
        vDispHexChar(1, BYTE0(slNumBlocksToErase) & 0x0f);

        // flash an 'E' on the display every other block i.e. 400ms to 700ms
        if (!(0x01 & slNumBlocksToErase))
        {
            vDispChar('3', 'E');
        }
        else
        {
            vDispChar('3', ' ');
        }

        vErase64KFlashBlock(ulAddrBlock);
    }

    return;
}

void vSetupLog(LOG_DEVICE ldDevice, LOG_TYPE ltLogModeDes)
{
	// save which device we are logging to
	ldLogDevice = ldDevice;

	// save whether we are accumulating data or gathering data
	ltLogMode = ltLogModeDes;

	lsStateLog = LS_IDLE;					// initialize state of logging

	ulHeadLog = ulTailLog = 0;				// init the FIFO pointers

	ulCountLog = 0;							// we haven't gathered any data as yet

	ewsWriteStateLog = WS_IDLE;				// we don't have any writes to
											// flash pending
	if (LD_LOG_TO_RAM == ldDevice)
	{
		ulSizeLog = BUF_LOG_LEN;
	}
	else
	{
		ulSizeLog = ulSizeFlash;

		// if the flash wasn't initialized for some reason, show an error
		if (0 == ulSizeLog)
		{
            SAVE_N_DIS_ALL_INT;
            vLogCmd(LMCmdError);
            vPutLog(LMPlyErrorLogSizeZero);
            RESTORE_INT;

			vDispChar(0, 'F');
		}
		else
		{
            bIsFlashOkay = TRUE;
		}
	}

	// init the flash base address
	ulFlashAddr = ulStartFlash;

	// initialize the data log buffer so that the array is all zero at
	// start of data gathering
	memset(ubBufLog, 0, sizeof(ubBufLog));

	// zero the ulDump memory
	memset(ulDump, 0, sizeof(ulDump));

	return;
}

void vReSetupLogRAM(void)
{
	SAVE_N_DIS_ALL_INT;

	lsStateLog = LS_IDLE;					// initialize state of logging

	ulHeadLog = 0;							// since this is logging to RAM, we don't
											// need to do anything to the tail

	ulCountLog = 0; 						// add new data to existing data

	RESTORE_INT;

	return;
}

/**************************************************************************
* Routine: 		ulGetAvailBufLog
*
* Purpose: 		Get number of bytes available in the BufLog buffer.
*
* Arguments:	None
*
* RETURNS: 	    Number of available bytes in ubBufLog
*
* Notes:	 	None
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 12/02/13		HarjitS     Created
*/
inline ULong ulGetAvailBufLog(void)
{
	ULong ulTemp;

	SAVE_N_DIS_ALL_INT;

	ulTemp = GETAVAILBUFLOG;

	RESTORE_INT;

	return (ulTemp);
}


/**************************************************************************
* Routine: 		ulGetLenBufLog
*
* Purpose: 		Get length of BufLog buffer.
*
* Arguments:	None
*
* RETURNS: 	    Number of entries in ubBufLog
*
* Notes:	 	None
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/24/12		HarjitS     Created
*/
static inline ULong ulGetLenBufLog(void)
{
	ULong ulTemp;

	SAVE_N_DIS_ALL_INT;

	ulTemp = GETLENBUFLOG;

	RESTORE_INT;

	return (ulTemp);
}

/**************************************************************************
* Routine: 		ulPeekULongBufLog
*
* Purpose: 		Get a ULong from the logging buffer but don't increment the head pointer:
*				ulHeadLog
*
* Arguments:	None
*
* RETURNS: 	ULong pointed to by the head pointer: ulHeadLog
*
* Notes:	 	None
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/17/12		HarjitS     Created
*/
inline static ULong ulPeekULongBufLog(void)
{
	ULong ulTemp;
	ULong ulIndex = ulHeadLog;

	ulTemp = ubBufLog[ulIndex];
	ulIndex++;
	ulIndex &= BUF_LOG_MSK;

	ulTemp += ubBufLog[ulIndex] << 8;
	ulIndex++;
	ulIndex &= BUF_LOG_MSK;

	ulTemp += ubBufLog[ulIndex] << 16;
	ulIndex++;
	ulIndex &= BUF_LOG_MSK;

	ulTemp += ubBufLog[ulIndex] << 24;
	ulIndex++;
	ulIndex &= BUF_LOG_MSK;

	return(ulTemp);
}

/**************************************************************************
* Routine: 		vGetFromBufLog
*
* Purpose: 		Get a UByte from the logging buffer.
*
* Arguments:
*				pWriteBuf:			Pointer to buffer that is used for the transaction
*				uwBytesToWrite: 	# of bytes in the buffer - includes flash command
*									flash address and buffer
*
* RETURNS: 	None
*
* Notes:	 	None
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/24/12		HarjitS     Created
* 09/01/12		HarjitS     To speed it upt, modifed to do a copy rather than just
*							return a byte
*/
void vGetFromBufLog(UByte * pWriteBuf, UWord uwBytesToWrite)
{
	SAVE_N_DIS_ALL_INT;

	do
	{
		*pWriteBuf++ = ubBufLog[ulTailLog];
		uwBytesToWrite--;

		ulTailLog++;
		ulTailLog &= BUF_LOG_MSK;
	} while (uwBytesToWrite);

	RESTORE_INT;

	return;
}

/**************************************************************************
* Routine: 		vPutUByteBufLog
*
* Purpose: 		Put ubData into the logging buffer
*
* Arguments:	ubData - data to be added to the logging buffer
*
* RETURNS: 	None
*
* Notes:	 	Increments ulHeadLog by size of UByte i.e. 1 byte
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/24/12		HarjitS     Created
* 09/16/12		HarjitS     The SPI code runs off of a timer and so it is no longer
*							necessary to enable the SPI state machine
*/
inline static void vPutUByteBufLog(UByte ubData)
{
	SAVE_N_DIS_ALL_INT;

    PUTBYTEBUFLOG(ubData);

    //
    // if after adding a byte, the head and tail are equal, that means we
    // overflowed the buffer, flag it by turning on the RED LED
    //
    if (ulHeadLog == ulTailLog)
    {
        RED_LED_ON;
    }

	RESTORE_INT;

	return;
}

/**************************************************************************
* Routine: 		vPutULongBufLog
*
* Purpose: 		Put ulData into the logging buffer
*
* Arguments:	ulData - data to be added to the logging buffer
*
* RETURNS: 	None
*
* Notes:	 	Increments ulHeadLog by size of ULong i.e. 4 bytes
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/17/12		HarjitS     Created
* 09/16/12		HarjitS     The SPI code runs off of a timer and so it is no longer
*							necessary to enable the SPI state machine
*/
inline static void vPutULongBufLog(ULong ulData)
{

    SAVE_N_DIS_ALL_INT;

	PUTBYTEBUFLOG(BYTE0(ulData));
	PUTBYTEBUFLOG(BYTE1(ulData));
	PUTBYTEBUFLOG(BYTE2(ulData));
	PUTBYTEBUFLOG(BYTE3(ulData));

	RESTORE_INT;

	return;
}

/**************************************************************************
* Routine: 		vPutLog
*
* Purpose: 		Put ubData argument into the logging buffer
*
* Arguments:	ubData - data to be added to the logging buffer
*
* RETURNS: 	    None
*
* Notes:	 	Increments ulHeadLog by size of UByte i.e. 1 bytes
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/17/12		HarjitS     Created
*/
void vPutLog(UByte ubData)
{
	if((LT_LOG_PUTLOG == ltLogMode) && !bIsDoneLog() && !bFilterLog)
	{
		if ((ulCountLog + 1) < ulSizeLog)
		{
			vPutUByteBufLog(ubData);

			// When we are logging to flash, in every flash page, we have a data count entry.
			// So, we need to account for it by adding one extra entry per the formula below.
			if (LD_LOG_TO_FLASH == ldLogDevice)
			{
#if ((SIZE_WRITE_PAGE-1) != 255)
#error next piece of code is broken
#endif
				if (0 == (ulCountLog & ENTRIES_WRITE_PAGE))
				{
					ulCountLog++;
				}
			}

			ulCountLog++;
		}
		else
		{
			vStopLog();
		}

	}

	return;
}

/**************************************************************************
* Routine: 		vPutStringLog
*
* Purpose: 		Write a string to the log
*
* Arguments:	Pointer to the string and how many characters to write out
*
* RETURNS: 	    void
*
* Notes:	    It doesn't write a null character at the end of the string
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 12/06/13      HarjitS     Created by modifying vPutStringUART()
*/
void vPutStringLog(char *pString, int iLen)
{
    SAVE_N_DIS_ALL_INT;

    // write out the log print string command
    vLogCmd(LMCmdPrintString);

    // per the payload requirements, first write out the length
    vPutLog(iLen);

    // now write out the actual string
	while (iLen)
	{
		vPutLog(*pString);
		pString++;
        iLen--;
	}

    RESTORE_INT;

	return;
}

/**************************************************************************
* Routine: 		vDumpLog(ULong ulCount)
*
* Purpose: 		Put log data from ulDump[] into queue
*
* Arguments:	ulCount - how many ulDump[] to log
*
* RETURNS: 	    void
*
* Notes:	 	Max. data we can process is forty bytes per ms
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 09/24/10		HarjitS     Created
* 10/22/11		HarjitS     Combined with vDoLog()
* 06/24/12		HarjitS     Moved vPutBufLog() into this function
*/
void vDumpLog(ULong ulCount)
{
	if(!bIsDoneLog())
	{
		if ((ulCountLog + ulCount) < ulSizeLog)
		{
			for (ULong ulI = 0; ulI < ulCount; ulI++)
			{
				// based on the logging mode, write the data into the buffer or
				// add the new data to data already in the buffer
				switch (ltLogMode)
				{
                    case LT_LOG_DUMPLOG:
                        vPutULongBufLog(ulDump[ulI]);
                        break;
                    case LT_LOG_DUMPLOG_ACCUMULATE:
                        // add a dummy statement so that we can have a variable
                        // decleration in a case statement.
                        ;

                        ULong ulTemp;

                        ulTemp = ulPeekULongBufLog() + ulDump[ulI];

                        vPutULongBufLog(ulTemp);
                        break;
                    default:
                        break;
				}

				// When we are logging to flash, in every flash page, we have a data count entry.
				// So, we need to account for it by adding one extra entry per the formula below.
				if (LD_LOG_TO_FLASH == ldLogDevice)
				{
#if ((SIZE_WRITE_PAGE-1) != 255)
#error next piece of code is broken
#endif
					if (0 == (ulCountLog & ENTRIES_WRITE_PAGE))
					{
						ulCountLog++;
					}
				}

				ulCountLog++;
			}
		}
		else
		{
			vStopLog();
		}

	}

	// zero the ulDump memory
	memset(ulDump, 0, sizeof(ulDump));

	return;
}

void vCheckNWriteLogToFlash(void)
{
	static UByte ubSPIFlashCmd; 				// storage for SPI flash command

	SPI_SERIAL_PRINTF("C");

	if ((LD_LOG_TO_FLASH == ldLogDevice) && ((LS_LOGGING == lsStateLog) ||
												(LS_NEED_FLUSH == lsStateLog)))
	{
		switch (ewsWriteStateLog)
		{
			case WS_IDLE:
				// add a dummy statement so that we can have a variable
				// decleration in a case statement.
				;
				// get # of entries in the buffer that need to be written to the flash
				ULong ulEntriesInBuf = ulGetLenBufLog();

				// write out up to one page at a time
				if (ENTRIES_WRITE_PAGE <= ulEntriesInBuf)
				{
						ulEntriesInBuf = ENTRIES_WRITE_PAGE;
				}

				// write out if we have a full page to write out or if we
				// have a partial page and we are flushing
				if ((ENTRIES_WRITE_PAGE == ulEntriesInBuf) || \
					 ((0 != ulEntriesInBuf) && (LS_NEED_FLUSH == lsStateLog)))
				{
					// Try to send only if we are able to get the bus
					if (TRUE == bGetNonEmbeddedLockSPI())
					{
						// in the buffer, skip over the spi flash command (1B) and
						// addr (3B)to the first entry that is used for flash data
						ULong ulEntries = FLASH_WRITE_BUF_OFFSET;

						//
						// write # of valid entries in page using the 1's complement format
						ubSPIBufLog[ulEntries] = (UByte) ~ulEntriesInBuf;
						ulEntries++;

						// fill up the flash SPI buffer with data
						vGetFromBufLog((UByte *) &ubSPIBufLog[ulEntries], (UWord) ulEntriesInBuf);

						ULong ulBytesToWrite = ulEntries + ulEntriesInBuf;

						// fill in the SPI transaction structure and wait for the transfer
						vWriteFlash(ulFlashAddr, (UByte *) &ubSPIBufLog[0], \
											(UWord) ulBytesToWrite);

#ifdef ENABLE_LOG_DEBUG
                        debug_printf("addr: %08x ulBytesToWrite: %08x ulEntries: %08x ulEntriesInBuf: %08x\n\r",
                            ulFlashAddr,
                            ulBytesToWrite,
                            ulEntries,
                            ulEntriesInBuf);
#endif

						// increment flash address to the available location
						// the +1 is because we need to cover the # of valid
						// entries in the page
						ulFlashAddr += ulEntriesInBuf + 1;

						// read the flash status register to make sure the write completed
						ewsWriteStateLog = WS_WAIT_DO_STATUS_REG_READ;
					}
				}
				break;

			case WS_WAIT_DO_STATUS_REG_READ:
				// wait until we can do a SPI access and then setup the
				// status register read
				if (TRUE == bEmbedOnlyInQueueSPI())
				{
					// setup and queue the SPI flash status register read
					vReadStatusRegFlash(&ubSPIFlashCmd, &ubSPIBufLog[0]);

					// get the flash status register and see if the write completed
					ewsWriteStateLog = WS_SR_POLL;
				}
				break;

			case WS_SR_POLL:
				// if there are only embedded SPI commands, that means the
				// status register read completed, so look at the flash write
				// status.
				if (TRUE == bEmbedOnlyInQueueSPI())
				{
					// if the flash write completed, then go back to idle state
					// otherwise, do another read of the status register.
	// TODO: if for some reason the write fails, we will be stuck here, every
	// TODO: millisecond, doing reads of the SPI. probably should add code
	// TODO: to stop this and forgo all future SPI flash access.
					if (bFlashReady(ubSPIBufLog[0]))
					{
						ewsWriteStateLog = WS_IDLE;

                        //
                        // Only change state if there is no more data to write
                        // out.
                        //
						if ((LS_NEED_FLUSH == lsStateLog) \
                            && (0 == ulGetLenBufLog()))
						{
							// we have flushed all the data to the flash
							lsStateLog = LS_FLUSHED;
						}

						// return the SPI bus in case someone else wants to use it
						vReturnNonEmbeddedLockSPI();
					}
					else
					{
                        // detect if we have a programming error
//                        if (ubSPIBufLog[0] & 0x3a)
//                        {
//                            RED_LED_ON;
//                        }

						// setup and queue the SPI flash status register read
						// and stay in this state
						vReadStatusRegFlash(&ubSPIFlashCmd, &ubSPIBufLog[0]);
					}
				}
				break;

			default:
					ewsWriteStateLog = WS_IDLE;
				break;
		}
	}

	return;
}

/**************************************************************************
* Routine: 		vWriteLogToHost
*
* Purpose: 		Read log data from SPI flash and then write it to the host
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	We use the ulBufLog[] buffer differently i.e. instead of using it as an
*				array, we read data into it and then when we are done writing it to
*				the host, we start from the begining. If we were using it as a FIFO,
*				then we would have to split the transfer to the host into two when
*				the FIFO wrapped around.
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 09/21/10		HarjitS     Modified to support reading data from SPI flash
*/
void vWriteLogToHost(void)
{
	DEBUG_FILE *pdfFile;

    ULong ulCntr = 0;

#ifdef ENABLE_LOG_PERF_ANALYSIS
    ULong ulReadTime = 0;

    ULong ulTransferTime = 0;

    ULong ulDebugCallCount = 0;
#endif

	// when we are logging to RAM, and logging is completed and this routine
	// is called, the logging state is idle, so don't do the idle check when logging
	// to RAM

	// if we haven't logged any data, do nothing and return
	if ((LD_LOG_TO_FLASH == ldLogDevice) && (LS_IDLE == lsStateLog))
		return;

	// turn off logging if necessary - this would happen if we were logging and
	// decided to stop right away.
	if (LS_LOGGING == lsStateLog)
		vStopLog();

	pdfFile = debug_fopen("Step.bin","wb");

	if(0 != pdfFile)
	{
		if (LD_LOG_TO_RAM == ldLogDevice)
		{
			debug_fwrite(&ubBufLog[0], ulGetLenBufLog() * sizeof(UByte), 1, pdfFile);
		}
		else
		{
			//
			// if we have data in the buffer wait until vCheckNWriteLogToFlash()
			// sends the data out
            if (ulHeadLog != ulTailLog)
            {
    			while (LS_FLUSHED != lsStateLog)
    				;
            }

			ULong ulEntriesToCopy = ENTRIES_WRITE_PAGE;
											// cannot be less than
											// ENTRIES_WRITE_PAGE because
											// that will cause the for loop
											// below to not start

			ulHeadLog = ulTailLog = 0;		// we haven't read any data
											// from the flash as yet

			for (ulFlashAddr = ulStartFlash; (ulFlashAddr <= ulSizeFlash) && \
					(ENTRIES_WRITE_PAGE == ulEntriesToCopy); ulFlashAddr+= SIZE_WRITE_PAGE)
			{
#ifdef ENABLE_LOG_PERF_ANALYSIS
                ULong ulStartTime = ulGetCycleCnt();
#endif
				vReadFlashBlock(ulFlashAddr, (UByte *) &ubSPIBufLog[0], SIZE_WRITE_PAGE);

#ifdef ENABLE_LOG_PERF_ANALYSIS
                ulReadTime += ulGetCycleCnt() - ulStartTime;
#endif

#ifdef ENABLE_LOG_DEBUG
                debug_printf("addr: %08x ulBytesRead: %08x\n\r",
                    ulFlashAddr,
                    SIZE_WRITE_PAGE);
#endif

				// get # of valid entries in the page
				ulEntriesToCopy = 255 - ubSPIBufLog[0];

				// copy data from the SPI buffer to the RAM buffer
				if (ENTRIES_WRITE_PAGE >= ulEntriesToCopy)
				{
					// copy data from SPI flash read buffer to RAM buffer
					// The '<=' is in there because the ulSPIBufLog's first entry
					// needs to be skipped
					for (ULong ulIndex = 1; ulIndex <= ulEntriesToCopy; ulIndex++)
					{
                        vPutUByteBufLog(ubSPIBufLog[ulIndex]);
					}
				}

				//
				// make sure there is enough space in the RAM buffer to hold
				// another page of data before reading it. Otherwise, write out
				// what is already in the RAM buffer.
				//
				if (ENTRIES_WRITE_PAGE == ulEntriesToCopy)
				{
                    // if there isn't enough space in the RAM buffer, write it out
					if (ENTRIES_WRITE_PAGE > (BUF_LOG_LEN - ulHeadLog))
					{
#ifdef ENABLE_LOG_PERF_ANALYSIS
                        ULong ulStartTime = ulGetCycleCnt();
#endif

						// write the data we've read out to the host
						debug_fwrite(&ubBufLog[0], ulGetLenBufLog() * sizeof(UByte), 1, pdfFile);

#ifdef ENABLE_LOG_PERF_ANALYSIS
                        ulTransferTime += ulGetCycleCnt() - ulStartTime;

                        ulDebugCallCount++;
#endif
#ifdef ENABLE_LOG_DEBUG
                        debug_printf("Bytes transfered: %08x\n\r",
                            ulGetLenBufLog() * sizeof(UByte));
#endif

						// use buffer from the begining again
						ulHeadLog = 0;

						// Flash "T" to indicate we are Transfering
						if (!(0x01 & ulCntr))
						{
							vDispChar('3', 'T');
						}
						else
						{
							vDispChar('3', ' ');
						}

						ulCntr++;
					}
				}
				else
				{
#ifdef ENABLE_LOG_PERF_ANALYSIS
                    ULong ulStartTime = ulGetCycleCnt();
#endif

					// write the data we've alreay read to the host
					debug_fwrite(&ubBufLog[0], ulGetLenBufLog() * sizeof(UByte), 1, pdfFile);

#ifdef ENABLE_LOG_PERF_ANALYSIS
                    ulTransferTime += ulGetCycleCnt() - ulStartTime;

                    ulDebugCallCount++;
#endif

#ifdef ENABLE_LOG_DEBUG
                    debug_printf("Bytes transfered: %08x\n\r",
                        ulGetLenBufLog() * sizeof(UByte));
#endif
				}
			}
		}

		debug_fclose(pdfFile);

#ifdef ENABLE_LOG_PERF_ANALYSIS
        debug_printf("Time to Read 1MB from Flash: %d\n\r", ulReadTime);
        debug_printf("Time to Transfer 1MB to Host: %d\n\r", ulTransferTime);
        debug_printf("Number of times debug_fwrite called: %d\n\r", ulDebugCallCount);
#endif
		vDispChar('3', ' ');
	}

	return;
}

void vDumpLogToHost(void)
{
	DEBUG_FILE *pdfFile;

    ULong ulCntr = 0;

    ULong ulNumBlocksInUse = slNum64KBlocksInUse();

	pdfFile = debug_fopen("Dump.bin","wb");

	if(0 != pdfFile)
	{
		for (ulFlashAddr = ulStartFlash;
                ulFlashAddr < (ulNumBlocksInUse * SIZE_ERASE_64K_BLOCK);
                ulFlashAddr += BUF_LOG_LEN)
		{
			vReadFlashBlock(ulFlashAddr, (UByte *) &ubBufLog[0], BUF_LOG_LEN);

			// write the data we've read out to the host
			debug_fwrite(&ubBufLog[0], BUF_LOG_LEN, 1, pdfFile);

#ifdef ENABLE_LOG_DEBUG
            debug_printf("addr: %08x ulBytesRead and transfered: %08x\n\r",
                ulFlashAddr,
                BUF_LOG_LEN);
#endif

            // show the block number that is being transferred
            vDispHexChar(0, (ulNumBlocksInUse - BYTE2(ulFlashAddr)) >> 4);
            vDispHexChar(1, (ulNumBlocksInUse - BYTE2(ulFlashAddr)) & 0x0f);

			// Flash "T" to indicate we are Transfering
			if (!(0x01 & ulCntr))
			{
				vDispChar('3', 'T');
			}
			else
			{
				vDispChar('3', ' ');
			}

			ulCntr++;
		}
	}

		debug_fclose(pdfFile);

	return;
}

/**************************************************************************
* Routine: 		vStressTestLogging
*
* Purpose: 		Stress test the SPI flash interface and flash chip
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	This routine uses the inherit delay in debug_printf. It should be modified
*				to use explicit ulDelayMS() calls.
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 08/??/10		HarjitS     Created
*/
void vStressTestLogging(void)
{
    ULong ulCounter = 0;

    vWaitPanicPressNRelease();

    debug_printf("Erasing Flash\n");
    vEraseLog();

    debug_printf("Starting Stress Write\n");
    vStartLog();

    // enable the profiler and servo routine
    vEnableProfilerNServo();

    // enable the forward and rotation servos
    g_bbEnableRotServo = TRUE;
    g_bbEnableFwdServo = TRUE;

    while (1 != 0)
    {
        if (GETAVAILBUFLOG > 0x10)
        {
            vPutULongBufLog(ulCounter);

            ulCounter++;

            if ((ulCounter & 0xfff) == 0xfff)
            {
                //
                // the * 4 is because we write out four bytes per
                // ulCounter write
                //
                vDispHexChar(0, BYTE2(ulCounter * 4) >> 4);
                vDispHexChar(1, BYTE2(ulCounter * 4) & 0x0f);
            }

            if (ulCounter > (1024 * 1024))
            {
                break;
            }
        }
    }

	vDisableProfilerNServo();

    debug_printf("Done Stress Write\n");

    vDumpLogToHost();

    return;
}

