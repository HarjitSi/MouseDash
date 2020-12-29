/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        		Log.h
*
* Written By:  	Harjit Singh
*
* Date:        	01/14/96
*
* Purpose:     	This is the data logger header file
*
* Notes:
*
* To Be Done:
*
* Modification History:
*
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

#ifndef INClogH
#define INClogH

#include "Common\Include\types.h"

#include "Common\Include\LogMsg.h"

#include "Common\Include\ZthInclude.h"

#define CSTRING_LEN				(256)		// length of buffer for strings
											// to be logged or sent out the
											// serial port
#ifdef LOG_C

char cString[CSTRING_LEN];					// used to print strings for logging
                                            // or those sent out the serial port
#else

extern char cString[CSTRING_LEN];           // used to print strings for logging
                                            // or those sent out the serial port

#define LOG_PRINTF(...) {int len=snprintf(cString, CSTRING_LEN, __VA_ARGS__);vPutStringLog(cString,len);}

#endif

// LT_LOG_DUMPLOG means log to flash using vDumpLog()
// LT_LOG_PUTLOG means log to flash using vPutLog()
// LT_LOG_DUMPLOG_ACCUMULATE means log data to RAM using vDumpLog and add the
// data to what is already in the buffer
typedef enum
{
    LT_LOG_NONE,
    LT_LOG_DUMPLOG,
    LT_LOG_PUTLOG,
	LT_LOG_DUMPLOG_ACCUMULATE
} LOG_TYPE;

typedef enum
{
	LD_LOG_TO_RAM,
	LD_LOG_TO_FLASH
} LOG_DEVICE;

#define	MAX_LOG_COUNT	16
										// maximum number of logging entries
										// per logging event

#ifdef LOG_C
ULong ulDump[MAX_LOG_COUNT];
#else
extern ULong ulDump[MAX_LOG_COUNT];
#endif // LOG_C

#define SMASH_LONG(high,low)	((((high)<<16)&0xffff0000)|((low)&0x0000ffff))

// This function converts encoder distances that are N.0 to mm in 5.2 format
static inline SLong slConvCountsp0Tomm5p2(SLong slCounts)
{
    slCounts = (slCounts * 4) / COUNTS_PER_MM;

    SATURATE(slCounts, INT8_MIN, INT8_MAX);
    return(slCounts);
}

// This function converts encoder distances that are N.0 to mm in sign + 13.2 format
static inline SLong slConvCountsp0Tomm13p2(SLong slCounts)
{
    slCounts = (slCounts * 4) / COUNTS_PER_MM;

    SATURATE(slCounts, INT16_MIN, INT16_MAX);
    return(slCounts);
}

// This function converts distances that are N.8 to sign + 5.2 format
static inline SLong slConvp8To5p2(SLong slCounts)
{
    slCounts >>= 6;

    SATURATE(slCounts, INT8_MIN, INT8_MAX);
    return(slCounts);
}

// This function converts distances that are N.8 to sign + 6.1 format
static inline SLong slConvp8To6p1(SLong slCounts)
{
    slCounts >>= 7;

    SATURATE(slCounts, INT8_MIN, INT8_MAX);
    return(slCounts);
}

// This function converts angles that are gyro units N.0 to degrees 9.7 format
static inline SLong slConvGryop0ToDeg9p7(SLong slGyro)
{
    slGyro %= (COUNTS_PER_DEGREE * 360);
    slGyro = (slGyro * 128) / COUNTS_PER_DEGREE;

    // convert it to 0 to 359.9921875
    if (slGyro < 0)
    {
        slGyro+= 360 * 128;
    }

    return(slGyro);
}

// This function converts angles that are gyro units N.0 to degrees 4.8 format
static inline SLong slConvGryop0ToDeg4p8(SLong slGyro)
{
    slGyro = (slGyro * 256) / COUNTS_PER_DEGREE;

    return(slGyro);
}



void vFilterLogCmd(UByte ubLogValue);

void vLogCmd(UByte ubLogValue);

void vStartLog(void);

void vStopLog(void);

void vScanLog(void);

void vEraseLog(void);

Bool bIsDoneLog(void);

void vSetupLog(LOG_DEVICE, LOG_TYPE);

void vReSetupLogRAM(void);

ULong ulGetAvailBufLog(void);

void vPutLog(UByte ubData);

void vPutStringLog(char * pString, int iLen);

void vDumpLog(ULong ulCount);

void vCheckNWriteLogToFlash(void);

void vWriteLogToHost(void);

void vDumpLogToHost(void);

void vStressTestFlash(void);

#endif /* INClogH */
