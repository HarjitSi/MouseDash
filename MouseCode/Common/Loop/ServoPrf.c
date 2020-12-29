/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 1995
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:			servoPrf.s
*
* Written By:  	Harjit Singh
*
* Date:        	02/15/10, 10/12/97
*
* Purpose:     	This file is the source file for the servo loop and profiler.
*				Porting it from 68K assembly to C for Zeetah V.
*				ServoPrf.s and ServoSup.s have been combined into this one file.
*
* Notes:
*
* To Be Done:
*
* Modification History:
*
*/

#define SERVOPRF_C

#include <stdio.h>

#include "Common\Include\types.h"
#include "Common\loop\log.h"
#include "serial\serial.h"
#include <stm32f10x.h>
#include <stdlib.h>

#include "ServoPrf.h"

/**************************************************************************
* Routine: 		vCheckNStopMouse
*
* Purpose: 		Stop the mouse if the battery voltage is too low or it has
*				crashed or the panic button was pressed.
*
* Arguments:	None
*
* RETURNS: 	    void
*
* Notes:	 	Crash detection uses the forward error which is also used to skip
*				accelerating the mouse. This is broken.
*
* To Be Done:	Need to tweak the thresholds
*
*-Date--------Author-----Description--------------------------------------
* 11/12/10		Harjit		Created
*/
inline static void vCheckNStopMouse(void)
{
	//
	// check if the battery voltage is too low and set the flag as appropriate
	//
	if (VBAT_CUTOFF_THRESHOLD >= GET_ADC_VBAT)
	{
		SERVO_SERIAL_PRINTF("CC:%4d B:%4d\n\r", slCrashErrorCntr, GET_ADC_VBAT);

		ulVBatIsLowCount++;

		if (VBAT_CUTOFF_COUNT <= ulVBatIsLowCount)
		{
            g_bbVBatTooLow = TRUE;
            g_bbMouseStopped = TRUE;

            vFilterLogCmd(LMCmdError);
            vPutLog(LMPlyErrorBatteryLow);

			// display a 'B'at to indicate the battery is too low
			vDispChar(0, 'B');
		}
	}
	else
	{
		ulVBatIsLowCount = 0;
	}

	//
	// check if the mouse has crashed by examining the forward and
	// rotation servo errors
	//
	if (CRASH_FWD_ERROR_THRESHOLD <= labs(sFwdLoop.slError))
	{
		SERVO_SERIAL_PRINTF("CC:%4d FE:%4d\n\r", slCrashErrorCntr, sFwdLoop.slError);

		slCrashErrorCntr++;

		if (CRASH_FWD_ROT_ERROR_COUNT <= slCrashErrorCntr)
		{
            g_bbFwdServoErrorTooHigh = TRUE;
            g_bbMouseStopped = TRUE;

            vFilterLogCmd(LMCmdError);
            vPutLog(LMPlyErrorFwdErrorHigh);

			// display a 'C' to indicate the mouse has crashed
			// don't want to use a 'F' because that is used to indicate a
			// front wall
			vDispChar(0, 'C');
		}
	}
	else if (CRASH_ROT_ERROR_THRESHOLD <= labs(sRotLoop.slError))
	{
		SERVO_SERIAL_PRINTF("CC:%4d RE:%4d\n\r", slCrashErrorCntr, sRotLoop.slError);

		slCrashErrorCntr++;

		if (CRASH_FWD_ROT_ERROR_COUNT <= slCrashErrorCntr)
		{
            g_bbRotServoErrorTooHigh = TRUE;
            g_bbMouseStopped = TRUE;

            vFilterLogCmd(LMCmdError);
            vPutLog(LMPlyErrorRotErrorHigh);

			// display a 'R' to indicate the mouse has crashed because of
			// a rotational error
			vDispChar(0, 'R');
		}
	}
	else
	{
		slCrashErrorCntr = 0;
	}

	//
	// If the panic button was pressed while the mouse is moving, stop the
	// mouse by setting the stop mouse flag and letting the high level code
	// disable the motors and clean everything up.
	//
	if (g_bbPanicButtonPressed && !g_bbMotionComplete)
	{
        g_bbMouseStopped = TRUE;
	}

	return;
}

/**************************************************************************
* Routine: 		vServoProf
*
* Purpose: 		Run the forward and rotational servos and also profile the motion.
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	The servos are a digital lead filter:
*
*
*            	 				 (s + ZERO1)
*				D(s) = GAIN -----------
*							 (s + POLE1)
*
*
* 				The forward time constant of the mouse is approx. 84ms
* 				The rotational time constant of the mouse is approx. 59ms
*
*				The mouse' transfer function is:
*
*		             		         461.3
*				Mf(s) =  ---------------
*						 s * (0.084 s + 1)
*
*		             		         461.3
*				Mr(s) =  ---------------
*						 s * (0.059 s + 1)
*
*				D(z) = G(z) = Z[D(s)] = y(n)
*
*				y(n) = Pole * y(n -1) + Gain * x(n) - Zero * Gain * x(n -1)
*
*				n		: Current sample time.
*				n -1	: Previous sample time.
*				y(n)	: Compensator output at n.
*				x(n)	: Command position - Actual Position at n. AKA Error at n.
*				x(n -1)	: Command position - Actual Position at n -1.
*
*				18.13 = (11.5 * 0.13) / 0.5 + 4.13 * 12.0 - 4.13 * 12.0
*
*				See notes on 9/20/10 for fixed point details.
*
*				When RotVel is positive the mouse turns right.
*				When RotVel is negative the mouse turns left.
*
* To Be Done:
*
*-Date--------Author-----Description--------------------------------------
* 09/14/10		Harjit		Writing forward profiler and reworking older rotational
*							profiler and servo loop code.
*/
void vServoProf(void)
{
    vLogCmd(LMCmdCorrector);

    SLong slTemp = lcLatCorr.slError;

    SATURATE(slTemp, INT16_MIN, INT16_MAX);

    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));

    slTemp = slCorrOutput >> 4;

    SATURATE(slTemp, INT16_MIN, INT16_MAX);

    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));

#ifdef NEW
    vLogCmd(LMCmdCorrector2);

    SLong slTemp = lcLatCorr.slError;

    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));
    vPutLog(BYTE2(slTemp));
    vPutLog(BYTE3(slTemp));

    slTemp = lcLatCorr.slAngle;

    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));
    vPutLog(BYTE2(slTemp));
    vPutLog(BYTE3(slTemp));
#endif

    vLogProfServo();

#ifdef LOG_SENSOR_DATA_MS
	vLogSensorData();
#endif

#ifdef ENABLE_GATHER_LOOP_RESP
	vGatherLoopRespData(slRightEnc, slLeftEnc);
#endif

	return;
}

void vGatherLoopRespData(SLong slRightEnc, SLong slLeftEnc)
{
	Accel aAccelData;

	// read the accelerometer data
	vReadAccel(&aAccelData);

	ulDump[0] = sFwdProf.slFwdVel;
	ulDump[1] = sFwdProf.slFwdPos;
	ulDump[2] = sRotProf.slRotVel;
	ulDump[3] = sRotProf.slRotPos;
	ulDump[4] = SMASH_LONG(sRotLoop.slError,sFwdLoop.slError);
//	ulDump[5] = SMASH_LONG(slLeftEnc,slRightEnc);
	ulDump[5] = SMASH_LONG(sRotLoop.slOutputLast,sFwdLoop.slOutputLast);

	ulDump[6] = SMASH_LONG(GET_ADC_GYRO,GET_ADC_VBAT);
	ulDump[7] = SMASH_LONG(aAccelData.uwY,aAccelData.uwX);
	ulDump[8] = SMASH_LONG(slGetMMPosition(),aAccelData.uwZ);

	ulDump[9] = SMASH_LONG(sRotProf.slFeedFwd >> K_LOOP_SHIFT, sFwdProf.slFeedFwd >> K_LOOP_SHIFT);

	vDumpLog(10);

	return;
}

void vLogSensorData(void)
{
	//
	// make sure to update vDumpLog in log.c
	//
	ulDump[0] = SMASH_LONG(slGetMMPosition(), ubCoord);
	ulDump[1] = SMASH_LONG(sLeftFrontSensor.uwPwr, sLeftFrontSensor.slDist);
	ulDump[2] = SMASH_LONG(sRightFrontSensor.uwPwr, sRightFrontSensor.slDist);
	ulDump[3] = SMASH_LONG(sLeftDiagSensor.uwPwr, sLeftDiagSensor.slDist);
	ulDump[4] = SMASH_LONG(sRightDiagSensor.uwPwr, sRightDiagSensor.slDist);
	ulDump[5] = sRotProf.slRotVel;

	vDumpLog(6);

	return;
}

void vLogGyroData(void)
{
	//
	// make sure to update vDumpLog in log.c
	//
	ulDump[0] = GET_ADC_GYRO;

	vDumpLog(1);

	return;
}

void vGatherFwdRotStepResponseData(void)
{
	Accel aAccelData;

	// read the accelerometer data
	vReadAccel(&aAccelData);

	// use the absolute value of the PWM drive because we want the
	// magnitude for rotation step response
	ulDump[0] = labs(slLeftPWMOverride) + labs(slRightPWMOverride);
	ulDump[1] = GET_ADC_VBAT;
	ulDump[2] = slGetLeftEnc();
	ulDump[3] = slGetRightEnc();;
	ulDump[4] = GET_ADC_GYRO;

	ulDump[5] = aAccelData.uwX;
	ulDump[6] = aAccelData.uwY;
	ulDump[7] = aAccelData.uwZ;

	vDumpLog(8);

	return;
}

void vGatherExecutionTimeData(void)
{
	// write out execution time for different processes
	ulDump[0] = SMASH_LONG((ulServoIntFinishTime - ulServoIntStartTime), (ul1KHzIntFinishTime - ul1KHzIntStartTime));
	ulDump[1] = SMASH_LONG((ulServoIntStartTime - ul1KHzIntStartTime), (ulSensorIntFinishTime - ulSensorIntStartTime));
	ulDump[2] = ulSolverFinishTime - ulSolverStartTime;

	vDumpLog(3);

	return;
}

