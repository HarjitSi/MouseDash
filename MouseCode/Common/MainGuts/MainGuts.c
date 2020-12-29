/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2010
*				 Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File: 			MainGuts.c
*
* Written By:	Harjit Singh
*
* Date: 			08/30/11
*
* Purpose:		This file contains the guts of the main.c file. It was moved here so that
*				ZV code could be extended to support ZVI.
*
*				Over time, the routines in this file will move out to other files.
*
* Notes:	   	This file is never compiled - it is included in main.c which is then compiled.
*
* To Be Done:
*
*-Date--------Author-----Description--------------------------------------
* 08/30/11		HarjitS		Created from main.c
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

#ifdef ENABLE_TEST_LOGGING
UByte ubDataLog;
void vLoggingTest(void);
#endif


void vWaitConnectWriteLog(void);

void vWaitConnectWriteLog(void)
{
	vDisableProfilerNServo();

	vDispChar(0, 'W');

	// wait for Panic press and release
	vWaitPanicPressNRelease();

	vWriteLogToHost();

	vDispChar(0, 'D');

	return;
}

/**************************************************************************
* Routine: 		vLogConvFactors
*
* Purpose: 		Log the conversion factors so that we can easily go between
*               natural mouse units and human friendly units.
*
* Arguments:	None
*
* RETURNS: 	    void
*
* Notes:
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 12/10/2013    HarjitS     Created
*/
void vLogConvFactors(void)
{
    SAVE_N_DIS_ALL_INT;

    vLogCmd(LMCmdConvFactors);
    vPutLog(LMCurVersion);
    vPutLog(MM_PER_CELL);
    vPutLog(BYTE0(COUNTS_PER_CELL));
    vPutLog(BYTE1(COUNTS_PER_CELL));
    vPutLog(BYTE0(UTURN_LENGTH));
    vPutLog(BYTE1(UTURN_LENGTH));
    vPutLog(BYTE0(LAT_CORR_FWD_OFFSET_TARG_CNT));
    vPutLog(BYTE1(LAT_CORR_FWD_OFFSET_TARG_CNT));

    RESTORE_INT;

    return;
}

void vLogRunStart(ULong ulRunMode, ULong ulTime)
{
    SAVE_N_DIS_ALL_INT;

    vLogCmd(LMCmdRunStart);

    if (LMPylRunModeLearn == ulRunMode)
    {
        vPutLog(mLearnParam.slAccelMPSS >> 5);

        vPutLog(mLearnParam.slMaxVelMPS >> 3);

        vPutLog(mLearnParam.slMaxVelMPS >> 3);

        // TODO: Add actual lateral accel
        vPutLog(0);
    }
    else
    {
        vPutLog(mSpeedRunParam.slAccelMPSS >> 5);

        vPutLog(mSpeedRunParam.slMaxVelMPS >> 3);

        vPutLog(mSpeedRunParam.slMaxVelMPS >> 3);

        // TODO: Add actual lateral accel
        vPutLog(0);
    }
    vPutLog(ubTries);

    vPutLog(ulRunMode);

    vPutLog(BYTE0(ulTime));
    vPutLog(BYTE1(ulTime));
    vPutLog(BYTE2(ulTime));
    vPutLog(BYTE3(ulTime));

    RESTORE_INT;

    return;
}

void vLogRunEnd(Bool bSuccess, ULong ulTime)
{
    SAVE_N_DIS_ALL_INT;

    if (bSuccess)
    {
        vLogCmd(LMCmdRunEndSuccess);
    }
    else
    {
        vLogCmd(LMCmdRunEndFail);
    }

    vPutLog(BYTE0(ulTime));
    vPutLog(BYTE1(ulTime));
    vPutLog(BYTE2(ulTime));
    vPutLog(BYTE3(ulTime));

    RESTORE_INT;

    return;
}

int main(void)
{
	//
	// Init the Learning and Speed run profiler parameters. These variables
	// are in m/s and m/s^2. The routines that use this info. do the conversion
	// to c/s and c/s^2 on the fly.
	//
	mLearnParam.slAccelMPSS = LEARNING_ACCEL_MPSS;
	mLearnParam.slDecelMPSS = LEARNING_DECEL_MPSS;
	mLearnParam.slMaxVelMPS = LEARNING_MAX_VEL_MPS;

	mSpeedRunParam.slAccelMPSS = SPEEDRUN_ACCEL_MPSS;
	mSpeedRunParam.slDecelMPSS = SPEEDRUN_DECEL_MPSS;
	mSpeedRunParam.slMaxVelMPS = SPEEDRUN_START_MAX_VEL_MPS;

	// setup the internal clock cycle counter
	vSetupCPUCycleCntr();

	// Setup the SPI port
	vSetupSPI();

	// Setup the UART serial channel
	vSetupUART();

#ifdef ENABLE_TEST_LOGGING
	vLoggingTest();
#endif

	// Get the flash size and end address
	vInitFlashBlock();

#ifdef CHIP_ERASE_FLASH
	vChipEraseFlashBlock();
#endif

	//
	// init the data gathering code
	//
	// NOTE: 	This needs to be after the 1KHzInt is up and running because it
	// 			does some SPI transactions
	//
	vSetupLog(LD_LOG_TO_FLASH, LT_LOG_PUTLOG);

#ifdef ENABLE_TEST_FLASH
//	vTestFlash();

    vStressTestLogging();

	while (1 != 0)
		;
#endif

    // ONLY DO THIS IF YOU WANT THE FLASH TO BE ERASED EVERY BOOT
    // NORMALLY, I DON'T DO THIS BUT CALL THIS IN A MENU
    vEraseLog();

	// wait for Panic press and release
	vWaitPanicPressNRelease();

    vStartLog();
    vLogConvFactors();

    // test the PrintString logging capability
    LOG_PRINTF("Hello World from: %s", MOUSE_NAME);

    vLogRunStart(LMPylRunModeLearn, ulRunTime);

	ubMotionCmdStatus |= (UBMOTIONCMDSTATUSM_PATH_VALID | \
							UBMOTIONCMDSTATUSM_PATH_NEW);

	// sit here and wait until all motion commands have been dispatched
	while ((UBMOTIONCMDSTATUSM_PATH_VALID & ubMotionCmdStatus) && \
			 	!g_bbMouseStopped)
	{
		;
	}

	// wait until motion is completed
	while (!g_bbMotionComplete && !g_bbMouseStopped)
	{
		;
	}

    ulRunTime = ulMilliSeconds - ulRunTime;

	vDisableProfilerNServo();

    vLogRunEnd(TRUE, ulRunTime);

    vDelayMS(5000);

	vWaitConnectWriteLog();

	return(0);
}

void vLoggingTest(void)
{
	void vFoo(void);

	// Get the flash size and end address
	vInitFlashBlock();

	//
	// init the data gathering code
	//
	// NOTE: 	This needs to be after the 1KHzInt is up and running because it
	// 			does some SPI transactions
	//
	vSetupLog(LD_LOG_TO_FLASH, LT_LOG_DUMPLOG);
    vEraseLog();

	// wait for Panic press and release
	vWaitPanicPressNRelease();

	// use the same byte to log data in the main and interrupt routine
	// this way, if there is a discontinuity, in the logged data, we can
	// tell that something went wrong
	ubDataLog = 0;

	// start data gathering
	vStartLog();

    ULong ulStartTime = ulGetCycleCnt();

	vDispChar(0, 'S');

	for (ULong ulCount = 0; ulCount < 0x100000; ulCount++)
	{
        // wait for space in the buffer
        while (1 >= ulGetAvailBufLog())
        {
            ;
        }

		// make writing and updating the variable atomic so that
		// we don't have double values in the output
		SAVE_N_DIS_ALL_INT;

		vPutLog(ubDataLog);

		ubDataLog++;

		RESTORE_INT;
	}

	vDispChar(0, 'C');

    debug_printf("Time to write 1MB log file: %d\n\r", ulGetCycleCnt()- ulStartTime);

	vWaitConnectWriteLog();

	// never exit
	while (1 != 0)
	{
		;
	}

}

void vFoo(void)
{
	bSensorReadDone = TRUE;

#ifdef LOG_IN_INTERRUPT
	for (ULong ulTemp = 128; ulTemp < 256; ulTemp++)
	{
		vPutLog(ubDataLog);
		ubDataLog++;
	}
#endif

	return;
}
#endif

