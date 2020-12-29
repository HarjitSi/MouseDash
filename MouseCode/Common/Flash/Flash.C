/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        		Flash.c
*
* Written By:  	Harjit Singh
*
* Date:        	08/08/2010
*
* Purpose:     	This file contains routines to access the Atmel AT25DF321 serial flash.
*
* Notes:
*
* To Be Done:
*
* Modification History:
* 09/21/10		Added a data logging function
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

#include "Common\Include\types.h"

#define FLASH_C
#include "Common\flash\flash.h"
#include "serial\serial.h"
#include "simcode\simcode.h"
#include "Common\loop\log.h"
#include "Common\display\display.h"

#include <stdlib.h>
#include <stdio.h>

static void vEnableWriteFlash(void);

inline Bool bFlashReady(UByte ubFlashStatus)
{
	return((FLASH_RSR_nRDY_BSY & ubFlashStatus) ? FALSE : TRUE);
}

/**************************************************************************
* Routine: 		vInitFlashBlock
*
* Purpose: 		Initialize the flash variables like size, etc.
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	None
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 06/17/12		HarjitS     Created
*/
void vInitFlashBlock(void)
{
	ULong ulMfgDevId;

	// get the manufacturer and device id and use it to figure out flash parameters
	ulMfgDevId = ulReadMfgDevIdFlashBlock() & FLASH_MFG_DEVID_MASK;

	switch (ulMfgDevId)
	{
		case FLASH_MFG_DEVID_AT25DF321:
			ulSizeFlash = FLASH_SIZE_AT25DF321;
			ulEndFlash = ulStartFlash + ulSizeFlash - 1;
			break;

		case FLASH_MFG_DEVID_AT25DF321A:
			ulSizeFlash = FLASH_SIZE_AT25DF321;
			ulEndFlash = ulStartFlash + ulSizeFlash - 1;
			break;

		case FLASH_MFG_DEVID_M25Q128:
			ulSizeFlash = FLASH_SIZE_M25Q128;
			ulEndFlash = ulStartFlash + ulSizeFlash - 1;
			break;

		default:
            {
                SAVE_N_DIS_ALL_INT;
                vLogCmd(LMCmdError);
                vPutLog(LMPlyErrorFlashUnknownDeviceID);
                RESTORE_INT;
		    }
			vDispChar(0, 'F');

            vDelayMS(1000);

			ulSizeFlash = 0;
			ulEndFlash = 0;
			break;
	}

	return;
}

ULong ulReadMfgDevIdFlashBlock(void)
{
	ULong ulTemp;
	UByte ubCmdFlash = FLASH_CMD_READ_MANUF_DEV_ID;

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	// read the manuf. device ID
	vTransactSPI(SPI_FLASH, &ubCmdFlash, sizeof(ubCmdFlash), (UByte *) &ulTemp, sizeof(ULong), SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	return (ulTemp);
}

ULong ulReadStatusRegFlashBlock(void)
{
	ULong ulTemp = 0x00;
	UByte ubCmdFlash = FLASH_CMD_READ_SR;

	SPI_SERIAL_PRINTF("r");

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	// read the status register
	vTransactSPI(SPI_FLASH, &ubCmdFlash, sizeof(ubCmdFlash), (UByte *) &ulTemp, sizeof(ULong), SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	return (ulTemp);
}

void vReadStatusRegFlash(UByte * pubCmdBuf, UByte * pReadBuf)
{
	// SPI flash read command
	*pubCmdBuf = FLASH_CMD_READ_SR;

	// read the status register
	vTransactSPI(SPI_FLASH, pubCmdBuf, sizeof(UByte), pReadBuf, sizeof(UByte), SPIDLY_NORMAL);

	return;
}

void vReadFlashBlock(ULong ulFlashAddr, UByte * pReadBuf, UWord uwBytesToRead)
{
	// 1 byte for command
	// 3 bytes for address
	UByte ubWriteBuf[1+3];

	// Setup SPI transaction

	// use the first page for all testing
	ubWriteBuf[0] = FLASH_CMD_READ;
	ubWriteBuf[1] = BYTE2(ulFlashAddr);
	ubWriteBuf[2] = BYTE1(ulFlashAddr);
	ubWriteBuf[3] = BYTE0(ulFlashAddr);

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	vTransactSPI(SPI_FLASH, ubWriteBuf, 4, pReadBuf, uwBytesToRead, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	return;
}


static void vEnableWriteFlash(void)
{
	SPI_SERIAL_PRINTF("E");

	vWriteOneByteSPI(SPI_FLASH, FLASH_CMD_WRITE_ENB, SPIDLY_MIN);

	return;
}

static void vEnableWriteFlashWithWELCheck(void)
{
	SPI_SERIAL_PRINTF("E");

	vWriteOneByteSPI(SPI_FLASH, FLASH_CMD_WRITE_ENB, SPIDLY_MIN);

	// check to see if the WEL bit is set right away or not
	if (!(FLASH_RSR_WEL & ulReadStatusRegFlashBlock()))
	{
        RED_LED_ON;
	}

	return;
}

void vUnProtectFlashBlock(void)
{
	SPI_SERIAL_PRINTF("U");

	vEnableWriteFlash();					// must enable writes before we
											// can run the global unprotect
											// command!

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	UByte ubWriteBuf[2];

	ubWriteBuf[0] = FLASH_CMD_WRITE_SR;
	ubWriteBuf[1] = 0x00;					// disable global protection

	vTransactSPI(SPI_FLASH, ubWriteBuf, 2, NULL, 0, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	return;
}

/**************************************************************************
* Routine: 		vWriteFlashBlockWithWELCheck
*
* Purpose: 		Write a buffer of data to the flash
*
* Arguments:	ulFlashAddr: 		Flash address to write data to
*				pWriteBuf:			Pointer to buffer that is used for the transaction
*				uwBytesToWrite:	# of bytes in the buffer - includes flash command
*									flash address and buffer
*
* RETURNS: 	void
*
* Notes:	 	None
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/24/12		HarjitS     Added header
*/
void vWriteFlashBlockWithWELCheck(ULong ulFlashAddr, UByte * pWriteBuf, UWord uwBytesToWrite)
{
	vEnableWriteFlashWithWELCheck();

	// Setup SPI transaction
	pWriteBuf[0] = FLASH_CMD_PROGRAM;
	pWriteBuf[1] = BYTE2(ulFlashAddr);
	pWriteBuf[2] = BYTE1(ulFlashAddr);
	pWriteBuf[3] = BYTE0(ulFlashAddr);

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	vTransactSPI(SPI_FLASH, pWriteBuf, uwBytesToWrite, NULL, 0, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	// wait until the write completes
	while (FLASH_RSR_nRDY_BSY & ulReadStatusRegFlashBlock())
		;

	return;
}

/**************************************************************************
* Routine: 		vWriteFlashBlock
*
* Purpose: 		Write a buffer of data to the flash
*
* Arguments:	ulFlashAddr: 		Flash address to write data to
*				pWriteBuf:			Pointer to buffer that is used for the transaction
*				uwBytesToWrite:	# of bytes in the buffer - includes flash command
*									flash address and buffer
*
* RETURNS: 	void
*
* Notes:	 	None
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 06/24/12		HarjitS     Added header
*/
void vWriteFlashBlock(ULong ulFlashAddr, UByte * pWriteBuf, UWord uwBytesToWrite)
{
	vEnableWriteFlash();

	// Setup SPI transaction
	pWriteBuf[0] = FLASH_CMD_PROGRAM;
	pWriteBuf[1] = BYTE2(ulFlashAddr);
	pWriteBuf[2] = BYTE1(ulFlashAddr);
	pWriteBuf[3] = BYTE0(ulFlashAddr);

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	vTransactSPI(SPI_FLASH, pWriteBuf, uwBytesToWrite, NULL, 0, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	// wait until the write completes
	while (FLASH_RSR_nRDY_BSY & ulReadStatusRegFlashBlock())
		;

	return;
}

void vWriteFlash(ULong ulFlashAddr, UByte * pWriteBuf, UWord uwBytesToWrite)
{
	// this writes a command to the SPI buffer
	vEnableWriteFlash();

	// Setup SPI transaction
	pWriteBuf[0] = FLASH_CMD_PROGRAM;
	pWriteBuf[1] = BYTE2(ulFlashAddr);
	pWriteBuf[2] = BYTE1(ulFlashAddr);
	pWriteBuf[3] = BYTE0(ulFlashAddr);

	vTransactSPI(SPI_FLASH, pWriteBuf, uwBytesToWrite, NULL, 0, FLASH_PAGE_PROGRAM_DELAY);

	return;
}
void vErase64KFlashBlock(ULong ulFlashAddr)
{
	// 1 byte for command
	// 3 bytes for address
	UByte ubWriteBuf[1+3];

	vEnableWriteFlash();

	// use the first page for all testing
	ubWriteBuf[0] = FLASH_CMD_ERASE_64K;
	ubWriteBuf[1] = BYTE2(ulFlashAddr);
	ubWriteBuf[2] = BYTE1(ulFlashAddr);
	ubWriteBuf[3] = BYTE0(ulFlashAddr);

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	vTransactSPI(SPI_FLASH, ubWriteBuf, 4, NULL, 0, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

    // TODO: add a timeout to the flash erase 64K command
	// wait until the write completes
	while (FLASH_RSR_nRDY_BSY & ulReadStatusRegFlashBlock())
		;

	return;
}

/**************************************************************************
* Routine: 		vChipEraseFlashBlock
*
* Purpose: 		Erase the entire flash chip
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	Erase takes between 32 and 56 seconds for AT25DF321 and
*				up to 250 seconds for the M25Q128.
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 11/13/11		HarjitS     Created
*/
void vChipEraseFlashBlock(void)
{
	// 1 byte for command
	// 3 bytes for address
	UByte ubWriteBuf[1+3];

	// Unprotect the flash
	vUnProtectFlashBlock();

	vEnableWriteFlash();

	// use the first page for all testing
	ubWriteBuf[0] = FLASH_CMD_CHIP_ERASE;

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	vTransactSPI(SPI_FLASH, ubWriteBuf, 1, NULL, 0, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	for (SLong slTimeOut = FLASH_CHIP_ERASE_TIME_MAX; slTimeOut > 0; slTimeOut--)
	{
        ULong ulStopTime = ulMilliSeconds + 999;

		while (ulMilliSeconds < ulStopTime)
		{
			;
		}

		// check if the erase completed
		if (!(FLASH_RSR_nRDY_BSY & ulReadStatusRegFlashBlock()))
			break;

		vDispCharBlock(1, '0' + (slTimeOut / 100) % 100);
		vDispCharBlock(2, '0' + (slTimeOut / 10) % 10);
		vDispCharBlock(3, '0' + slTimeOut % 10);
	}

	return;
}

#ifdef ENABLE_TEST_FLASH
/**************************************************************************
* Routine: 		vTestFlash
*
* Purpose: 		Test the SPI flash interface and flash chip
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
void vTestFlash(void)
{
#define	TEST_FLASH_BUF_SIZE	(256)
	// 1 byte for command
	// 3 bytes for address
	// 256 bytes for data payload
	UByte ubWriteBuf[1+3+TEST_FLASH_BUF_SIZE];
	UByte ubReadBuf[TEST_FLASH_BUF_SIZE];

#ifdef CONTINUOUS_READ_FLASH
	while (1  != 0)
		vReadFlashBlock(0x00, ubReadBuf, TEST_FLASH_BUF_SIZE);
#endif

	ULong ulTemp = ulReadMfgDevIdFlashBlock();
	debug_printf("Mfg Dev. ID = %08x\n", ulTemp);

	ulTemp = ulReadStatusRegFlashBlock();
	debug_printf("Status Register = %08x\n", ulTemp);

	debug_printf("Enabling write\n");
	vEnableWriteFlash();

	ulTemp = ulReadStatusRegFlashBlock();
	debug_printf("Status Register = %08x\n", ulTemp);

	debug_printf("Global unprotect\n");
	ubWriteBuf[0] = FLASH_CMD_WRITE_SR;
	ubWriteBuf[1] = 0x00;					// disable global protection

	// get and lock the SPI bus
	// by getting the lock, we are guaranteed that there isn't an embedded xfer
	// in progress
	while (FALSE == bGetNonEmbeddedLockSPI())
		;

	vTransactSPI(SPI_FLASH, ubWriteBuf, 2, NULL, 0, SPIDLY_MIN);

	// wait until the transfer completes
	while (FALSE == bDoneSPI())
		;

	// done with the SPI bus, make it available for anything else
	vReturnNonEmbeddedLockSPI();

	ulTemp = ulReadStatusRegFlashBlock();
	debug_printf("Status Register = %08x\n", ulTemp);

	debug_printf("Data currently at address 0\n");
	// read what is in currently at address 0
	vReadFlashBlock(0x00, ubReadBuf, TEST_FLASH_BUF_SIZE);

	// print the data read
	for (ULong i = 0; i < TEST_FLASH_BUF_SIZE; i+=16)
	{
		debug_printf("%03d: %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x\n", i,
						ubReadBuf[i], ubReadBuf[i+1], ubReadBuf[i+2], ubReadBuf[i+3], \
						ubReadBuf[i+4], ubReadBuf[i+5], ubReadBuf[i+6], ubReadBuf[i+7], \
						ubReadBuf[i+8], ubReadBuf[i+9], ubReadBuf[i+10], ubReadBuf[i+11], \
						ubReadBuf[i+12], ubReadBuf[i+13], ubReadBuf[i+14], ubReadBuf[i+15]);
	}

	// generate new data
	for (ULong i = 0; i < TEST_FLASH_BUF_SIZE; i++)
	{
		ubWriteBuf[4+i] = (UByte) BYTE0(ulGetCycleCnt());
	}

	debug_printf("Erasing address 0\n");
	// erase the page
	vErase64KFlashBlock(0x00);

	ulTemp = ulReadStatusRegFlashBlock();
	debug_printf("Status Register = %08x\n", ulTemp);

	debug_printf("Writing data to address 0\n");
	// write out the generated data
	vWriteFlashBlock(0, ubWriteBuf, 1+3+TEST_FLASH_BUF_SIZE);

	ulTemp = ulReadStatusRegFlashBlock();
	debug_printf("Status Register = %08x\n", ulTemp);

	debug_printf("Reading data just written to address 0\n");
	// read what we just wrote and check if it matches what we wrote out
	vReadFlashBlock(0x00, &ubReadBuf[0], TEST_FLASH_BUF_SIZE);

	for (ULong i = 0; i < TEST_FLASH_BUF_SIZE; i++)
	{
		if (0 == (i % 16))
			debug_printf(".");

		if (ubReadBuf[i] != ubWriteBuf[4+i])
			debug_printf("\n0x%02x Wrote: 0x%02x Read: 0x%02x\n", \
							i, ubWriteBuf[4+i],	ubReadBuf[i]);
	}
	debug_printf("\nTest Complete\n");

	return;
}

#endif
