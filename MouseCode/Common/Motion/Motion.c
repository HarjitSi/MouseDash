/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 1995
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        motion.c
*
* Written By:  Harjit Singh
*
* Date:        2/18/99
*
* Purpose:     This file contains code that will move the mouse in the maze
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

#define MOTION_C


#include "Common\loop\log.h"

#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#ifdef ENABLE_LONG_CORR_LOG_OUTPUT
#define LONG_LOG_PRINTF(...) LOG_PRINTF(__VA_ARGS__)
#else
#define LONG_LOG_PRINTF(...)
#endif


/**************************************************************************
* Routine:      vPreSensorReadMMProcessing
*
* Purpose:      Do whatever we need to before we start the read of the
*               sensors for this millimeter
*
* Arguments:    Current encoder position
*
* RETURNS: 	    void
*
* Notes:
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 08/07/2016    Harjit      Added this header
*/
inline void vPreSensorReadMMProcessing(SLong slCurPos)
{
#ifdef LOG_SENSOR_DATA_MM
    //
    // need to grab the data before the sensor cycle otherwise
    // we'll be reading variables whose data may be changing.
    // Also, we want the slMMPosition to match the sensor data.
    //
    vLogSensorData();
#endif

    return;
}

/**************************************************************************
* Routine:      vPostSensorReadMMProcessing
*
* Purpose:      The sensors for this millimeter have been read. Now do all
*               the processing for this millimeter
*
* Arguments:    None
*
* RETURNS: 	    void
*
* Notes:
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 08/07/2016    Harjit      Added this header
*/
void vPostSensorReadMMProcessing(void)
{
	vLogProfServoSensors();

	return;
}

SLong slSquareRoot(SLong slNum)
{
	ULong ulSquaredbit, ulRemainder, ulRoot;

	if (slNum < 1)
	{
		return 0;
	}

	//
	// Load the binary constant 01 00 00 ... 00, where the number
	// of zero bits to the right of the single one bit
	// is even, and the one bit is as far left as is consistant
	// with that condition.
	//
	ulSquaredbit  = (unsigned int) ((((unsigned int) ~0L) >> 1) &
                        ~(((unsigned int) ~0L) >> 2));

	// Form bits of the answer
	ulRemainder = slNum;
	ulRoot = 0;

	while (ulSquaredbit > 0)
	{
		if (ulRemainder >= (ulSquaredbit | ulRoot))
		{
			ulRemainder -= (ulSquaredbit | ulRoot);
			ulRoot >>= 1;
			ulRoot |= ulSquaredbit;
		}
		else
		{
			ulRoot >>= 1;
		}

		ulSquaredbit >>= 2;
	}

   return ulRoot;
}

inline void vSetubCoord(UByte ubValue)
{
	ubCoord = ubValue;

    SAVE_N_DIS_ALL_INT;

    // write out the log current cell location command and the payload
    vLogCmd(LMCmdCellLoc);
    vPutLog(ubValue);

    RESTORE_INT;

    cyxCoord.slCoordY = (ubValue & COORDM_Y) >> (COORDV_Y_SHIFT - COORDV_YX_SHIFT);
	cyxCoord.slCoordX = (ubValue & COORDM_X) << COORDV_YX_SHIFT;

	return;
}

/**************************************************************************
* Routine: 		vUpdateubCoordCalcXY
*
* Purpose: 		Update cyxCoord and ubCoord
*
* Arguments:	Current move from ubMtnCmd
*
* RETURNS: 	    void
*
* Notes:	 	Must be called *before* orientation is updated.
*
*				ubCoord is cyxCoord's x and y values whose LSB has been
*				stripped and packed into one byte
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 11/14/10		HarjitS     Created
*/
void vUpdateubCoordCalcXY(void)
{
	// validate orientation before using it
	if (UBORIENT_MAX < ubOrient)
	{
        vFilterLogCmd(LMCmdError);
        vPutLog(LMPlyErrorOrientBadUpdateCoord);
		vDispChar(0, 'O');
	}
	else
	{
		SLong slDeltaY, slDeltaX;

		//
		// handle coordinate update for forward and diagonal forward
		//
		// We don't have to determine if the move is diagonal or not because the
		// orientation for diogonals Vs non-diagonals is different.
		//
		if (MTNCMDV_MOVE_TYPE_FWD == (ubMtnCmd & MTNCMDM_MOVE_TYPE))
		{
			slDeltaY = cxyFwdXY[ubOrient].slCoordY;
			slDeltaX = cxyFwdXY[ubOrient].slCoordX;
		}
		else
		{
			ULong ulTurnType;

			// get the turn type
			ulTurnType = ubMtnCmd & MTNCMDM_TURN_TYPE;

			if (MTNCMDV_MOVE_DRCTN_RIGHT == (ubMtnCmd & MTNCMDM_MOVE_DRCTN))
			{
				slDeltaY = cxyRightXY[ulTurnType][ubOrient].slCoordY;
				slDeltaX = cxyRightXY[ulTurnType][ubOrient].slCoordX;
			}
			else
			{
				slDeltaY = cxyLeftXY[ulTurnType][ubOrient].slCoordY;
				slDeltaX = cxyLeftXY[ulTurnType][ubOrient].slCoordX;
			}
		}

		cyxCoord.slCoordY += slDeltaY;
		cyxCoord.slCoordX += slDeltaX;
	}

	// convert from t_CoordXY to ubCoord
	ubCoord = ubConverttCoordXYubCoord(cyxCoord);

    SAVE_N_DIS_ALL_INT;

    // write out the log current cell location command and the payload
    vLogCmd(LMCmdCellLoc);
    vPutLog(ubCoord);

    RESTORE_INT;

	LONG_LOG_PRINTF("C:%2x %2x %2x %2x S:%08x", ubCoord, cyxCoord.slCoordY, cyxCoord.slCoordX, ubOrient, ulSysFlags);

	return;
}

/**************************************************************************
* Routine:      vLatCorrUpdateOffset()
*
* Purpose:      Compute angle error term for lateral correction and jam
*               it into something the profiler can use
*
* Arguments:    True = try push/pull also
*
* RETURNS: 	    void
*
* Notes:
*
* To Be Done:   Need to add damping
*
*-Date----------Author------Description--------------------------------------
* 08/20/2016    Harjit      Added function header
*                           Changed to only use one wall even if we have data
*                           for both walls
*/
static inline void vLatCorrUpdateOffset(Bool bPushPull)
{
    if (lcLatCorr.slError != 0)
    {
        vLogCmd(LMCmdLatCorrCorrection2);
        // record the lateral correction sensor distance 8.8mm as sign + 5.2 mm
        vPutLog(BYTE0(slConvp8To5p2(lcLatCorr.slError)));
        vPutLog(BYTE0(ulWalls));
    }
    else
    {
        vLogCmd(LMCmdLatCorrNoCorrDeadBand);
    }

    return;
}

/**************************************************************************
* Routine:		vLogProfServo
*
* Purpose:		Log the profiler and servo data
*
* Arguments:	None
*
* RETURNS:		void
*
* Notes:
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 03/02/15		Harjit		Modified vUpdateMotionSensorStruct() to be this routine
*/
void vLogProfServo(void)
{
    vLogCmd(LMCmdProfServoOnly);

    SLong slTemp;

    // record the forward error as sign + 5.2 mm
    slTemp = slConvCountsp0Tomm5p2(sFwdLoop.slError);
    vPutLog(BYTE0(slTemp));

    // record the rotational position as 9.7 deg
    slTemp = slConvGryop0ToDeg9p7(sRotProf.slRotPos);
    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));

    SLong slFwdVel = CONV_VEL_CPMS_TO_MPS(sFwdProf.slFwdVel);
    vPutLog(BYTE0(slFwdVel));

    slTemp = slConvGryop0ToDeg4p8(sRotLoop.slError);

    // the range is sign + 4.8
    SATURATE(slTemp, -(2^12), (2^12));
    vPutLog(BYTE0(slTemp));

    // write out the integer portion of FVel and RotPosError
    vPutLog((BYTE1(slFwdVel) & 0x07) | (BYTE1(slTemp) << 3));

    vPutLog(CONV_VBAT_CNTS_TO_VOLTS(GET_ADC_VBAT));

    return;
}

/**************************************************************************
* Routine:		vLogProfServoSensors
*
* Purpose:		Log the profiler, servo and sensor data
*
* Arguments:	None
*
* RETURNS:		void
*
* Notes:
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 08/10/14		Harjit		Modified vUpdateMotionSensorStruct() to be this routine
*/
void vLogProfServoSensors(void)
{
    // if the servo
    vLogCmd(LMCmdProfServoSensors);

    SLong slTemp;

    // record the forward error as sign + 5.2 mm
    slTemp = slConvCountsp0Tomm5p2(sFwdLoop.slError);
    vPutLog(BYTE0(slTemp));

    // record the rotational position as 9.7 deg
    slTemp = slConvGryop0ToDeg9p7(sRotProf.slRotPos);
    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));

    SLong slFwdVel = CONV_VEL_CPMS_TO_MPS(sFwdProf.slFwdVel);
    vPutLog(BYTE0(slFwdVel));

    slTemp = slConvGryop0ToDeg4p8(sRotLoop.slError);

    // the range is sign + 4.8
    SATURATE(slTemp, -(2^12), (2^12));
    vPutLog(BYTE0(slTemp));

    // write out the integer portion of FVel and RotPosError
    vPutLog((BYTE1(slFwdVel) & 0x07) | (BYTE1(slTemp) << 3));

    vPutLog(CONV_VBAT_CNTS_TO_VOLTS(GET_ADC_VBAT));

    vPutLog(Pose.slMMPosition);

#define ENCODE_SENSOR_POWER(x,y)  if((x)<2048){y=(x)/16;}else if((x)<4096){y=((x)-2048)/32+128;}else{y=((x)-4096)/64+192;}
    // see payload description of LMCmdProfServoSensors in LogMsg.cs
    UByte ubTemp;

    ENCODE_SENSOR_POWER(sLeftFrontSensor.uwPwr, ubTemp);
    vPutLog(ubTemp);

    ENCODE_SENSOR_POWER(sLeftDiagSensor.uwPwr, ubTemp);
    vPutLog(ubTemp);

    ENCODE_SENSOR_POWER(sRightDiagSensor.uwPwr, ubTemp);
    vPutLog(ubTemp);

    ENCODE_SENSOR_POWER(sRightFrontSensor.uwPwr, ubTemp);
    vPutLog(ubTemp);

    vPutLog(BYTE1(sLeftFrontSensor.slDist));
    vPutLog(BYTE1(sLeftDiagSensor.slDist));
    vPutLog(BYTE1(sRightDiagSensor.slDist));
    vPutLog(BYTE1(sRightFrontSensor.slDist));

    ubTemp = ((BYTE0(sLeftFrontSensor.slDist) & 0xc0) >> 6) \
        | ((BYTE0(sLeftDiagSensor.slDist) & 0xc0) >> 4) \
        | ((BYTE0(sRightDiagSensor.slDist) & 0xc0) >> 2) \
        | (BYTE0(sRightFrontSensor.slDist) & 0xc0);
    vPutLog(ubTemp);

	return;
}

/**************************************************************************
* Routine: 		vLongCorrDataGather
*
* Purpose: 		Take the sensor data and save it for long. corr. As it grabs
*				data, it keeps track of the peak sensor value and its index
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	Used by orthogonal and diagonal long. corr.
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 11/05/11		Harjit		Created
* 09/19/12		Harjit		Removed derivative calculator and replaced with peak
*							detector
*/
void vLongCorrDataGather(void)
{
    vLogCmd(LMCmdLongCorrDataGather);

	return;
}

void vFLongCorrDataReset(void)
{
    vLogCmd(LMCmdLongCorrDataReset);

    return;
}

void vFrontLongCorrEnable(void)
{
    // if already enabled, do nothing
    if (g_bbEnableLongCorrector)
    {
        vLogCmd(LMCmdLongCorrAlreadyEnabled);

        return;
    }

    vFLongCorrDataReset();

	LONG_LOG_PRINTF("FLCI MMCnt:%3d NP:%8d", Pose.slMMPosition, Pose.slNextPegPos);

	if ((Pose.slMMPosition > FRONT_LONG_CORR_DONE) \
            && (Pose.slMMPosition < REAR_LONG_CORR_INIT))
    	{
            vLogCmd(LMCmdLongCorrSkip);

    		// adjust long corr's next peg expected position
    		Pose.slNextPegPos += COUNTS_PER_CELL;

    		LONG_LOG_PRINTF("NP4:%8d TP4", Pose.slNextPegPos, sFwdProf.slTarPos);

    		// since we are in the middle of the cell, we don't have any pegs, so
    		// tell the lateral correction code to ignore peg data
    		sLatCorrUsePegs.bUsePegs = FALSE;
    	}

    vLogCmd(LMCmdLongCorrEnable);

    // grab data this mm also
    vLongCorrDataGather();

    return;
}

void vFrontLongCorrDisable(void)
{
    vLogCmd(LMCmdLongCorrDisable);

    return;
}

void vFrontLongCorrPropagate(Bool bUseLeftFallingEdge, Bool bUseRightFallingEdge)
{
	//
	// Go and find the rising and falling edges and detect if we only have
	// posts.
	//
    Bool bPegOnLeft = FALSE;
    Bool bPegOnRight = FALSE;

    bPegOnLeft  = (sLeftDiagSensorRisingEdge.bFound && bUseLeftFallingEdge);
    bPegOnRight = (sRightDiagSensorRisingEdge.bFound && bUseRightFallingEdge);

    // log the edges/peg info.
    if (bPegOnLeft)
    {
        vLogCmd(LMCmdLongCorrLeftBothEdges);
    }
    else if (bUseLeftFallingEdge)
    {
        vLogCmd(LMCmdLongCorrLeftFallingEdge);
    }
    else if (sLeftDiagSensorRisingEdge.bFound)
    {
        vLogCmd(LMCmdLongCorrLeftRisingEdge);
    }
    else
    {
        vLogCmd(LMCmdLongCorrLeftNoEdges);
    }

    if (bPegOnRight)
    {
        vLogCmd(LMCmdLongCorrRightBothEdges);
    }
    else if (bUseRightFallingEdge)
    {
        vLogCmd(LMCmdLongCorrRightFallingEdge);
    }
    else if (sRightDiagSensorRisingEdge.bFound)
    {
        vLogCmd(LMCmdLongCorrRightRisingEdge);
    }
    else
    {
        vLogCmd(LMCmdLongCorrRightNoEdges);
    }

    if (sLatCorrUsePegs.bUsePegs)
    {
        vLogCmd(LMCmdLatCorrPegs);

        LONG_LOG_PRINTF("LP: %d RP %d", sLatCorrUsePegs.slLeftDist, sLatCorrUsePegs.slRightDist);
    }

    //
    // if slLongCorr is positive, then that means we are short by that distance
    // and have to go that much further
    // if slLongCorr is negative, then that means we overshot by that distance
    // and have to go that much less
    //
    SLong slLongCorr = 0;
    ULong ulCount = 0;

    // if we have a falling edge, get its position
    //
    if (bUseLeftFallingEdge)
    {
        slLongCorr += sLeftDiagSensorFallingEdge.sSensor.slReadPos;

        LONG_LOG_PRINTF("LFE:%6d", sLeftDiagSensorFallingEdge.sSensor.slReadPos);

        ulCount++;
    }

    if (bUseRightFallingEdge)
    {
        slLongCorr += sRightDiagSensorFallingEdge.sSensor.slReadPos;

        LONG_LOG_PRINTF("RFE:%6d", sRightDiagSensorFallingEdge.sSensor.slReadPos);

        ulCount++;
    }

    slLongCorr /= ulCount;

    LONG_LOG_PRINTF("C:%2d NPP:%6d LC:%6d TP:%6d", ulCount, Pose.slNextPegPos, slLongCorr, sFwdProf.slTarPos);

    slLongCorr += - FWD_PEG_CENTER_FE_OFFSET - Pose.slNextPegPos;

    //
    // Log the amount corrected
    //
    // Kind of slimy to log the command and then pump out the data, but I can't
    // think of a better way to do it
    //
    vLogCmd(LMCmdLongCorrCorrection);

    SLong slTemp = slConvCountsp0Tomm13p2(slLongCorr);
    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));

    return;
}

void vFLatCorrStart(void)
{
    vLogCmd(LMCmdLatCorrStart);

    return;
}

void vFLatCorrHold(void)
{
	// hold until we get to where we want to do the lateral correction
	if ((Pose.slMMPosition > F_LAT_CORR_INIT_READ_WALLS_1) \
            && (Pose.slMMPosition < F_LAT_CORR_DONE))
	{
        vFLatCorrStart();
	}
    else
    {
        vLogCmd(LMCmdLatCorrHold);
    }

	return;
}

void vReadWalls1(void)
{
    // Log this state
    vLogCmd(LMCmdLatCorrInitReadWalls1);
	return;
}

void vReadWalls2(void)
{
    // Log this state
    vLogCmd(LMCmdLatCorrDoReadWalls2);

	return;
}

void vReadWalls3StartLatCorr(void)
{
    // Log that we are NOT backtracking
    vLogCmd(LMCmdLatCorrDoReadWalls3NM_NoBT);

    //
    // log that we have to abort the path because we found a front
    // wall in our way.
    //
    vLogCmd(LMCmdAbortPathFrontWall);

    //
    // log that we have to abort the path because we found a side
    // wall in our way.
    //
    vLogCmd(LMCmdAbortPathSideWall);

	return;
}

void vFLatCorrDo(void)
{
    vLogCmd(LMCmdLatCorrDone);
	return;
}

void vFLatCorrEnable(void)
{
    vLogCmd(LMCmdLatCorrAlreadyEnabled);

    vLogCmd(LMCmdLatCorrEnable);

    return;
}

void vFLatCorrDisable(void)
{
    vLogCmd(LMCmdLatCorrDisable);

    return;
}

/**************************************************************************
* Routine: 		vDiagLatCorrDo
*
* Purpose: 		Since we do lateral correction in two different zones in a diag. fwd move,
*				use the same code in both places but putting the code into this routine.
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	None
*
* To Be Done:	Tune sensor threshold and correction gain
*
*-Date----------Author------Description--------------------------------------
* 11/15/11		HarjitS     Created
*/
void vDiagLatCorrDo(void)
{

    UByte ubLMCmd = LMCmdDiagCorrSkip;

    ubLMCmd = LMCmdDiagLatCorrDoLeft;

    ubLMCmd = LMCmdDiagLatCorrDoLeftOffset;

    ubLMCmd = LMCmdDiagLatCorrDoRightOffset;

    vLogCmd(ubLMCmd);

    if ((ubLMCmd == LMCmdDiagLatCorrDoLeftOffset) \
        || (ubLMCmd == LMCmdDiagLatCorrDoRightOffset))
    {
        // record the lateral correction sensor distance 8.8mm as sign + 5.2 mm
        vPutLog(BYTE0(slConvp8To5p2(slError)));
    }

	return;
}

/**************************************************************************
* Routine: 		vDiagCorrInit
*
* Purpose: 		Setup the diagonal forward motion
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
* 11/14/11		HarjitS     Created
*/
void vDiagCorrInit(void)
{
	// figure out on which side of the mouse we should look for the peg
	if ((DIAGSTATEM_SENSOR_SIDE & uwDiagState) == DIAGSTATEV_SENSOR_SIDE_LEFT)
	{
        vLogCmd(LMCmdDiagCorrInitLeft);
		pDiagSensorPeak = &sLeftDiagSensorPeak;
	}
	else
	{
        vLogCmd(LMCmdDiagCorrInitRight);
		pDiagSensorPeak = &sRightDiagSensorPeak;
	}

	return;
}

void vDiagLatCorrHold(void)
{
	if (Pose.slMMPosition >= DIAG_LAT_CORR1_SAMP_1)
	{
        vLogCmd(LMCmdDiagLatCorrHold);
	}

	return;
}

void vDiagLatCorr2Sample1(void)
{
	//
	// Don't have to do anything in this state because vPostSensorReadMMProcessing
	// is already grabbing and storing the front senors.
	//
	vLogCmd(LMCmdDiagLatCorr2Sample1);

	return;
}

void vDiagLatCorr2Sample2Do(void)
{
    vLogCmd(LMCmdDiagLatCorr2Sample2Do);

    vLogCmd(LMCmdDiagLongCorrDo);

	return;
}

void vDiagLongCorrDo(void)
{
    vLogCmd(LMCmdDiagLongCorrCorrection);

    SLong slTemp = slConvCountsp0Tomm13p2(slLongCorr);
    vPutLog(BYTE0(slTemp));
    vPutLog(BYTE1(slTemp));

	return;
}

void vDiagMoveUpdate(void)
{
    vLogCmd(LMCmdDiagMoveUpdate);
	return;
}

void vDiagLatCorr1Sample1(void)
{
	//
	// Don't have to do anything in this state because vPostSensorReadMMProcessing
	// is already grabbing and storing the front senors.
	//

    vLogCmd(LMCmdDiagLatCorr1Sample1);
	return;
}

void vDiagLatCorr1Sample2Do(void)
{
    vLogCmd(LMCmdDiagLatCorr1Sample2Do);
	return;
}

/**************************************************************************
* Routine: 		vAlignTurnU
*
* Purpose: 		Use the front wall to zero out any rotational error in a uturn
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:	 	Called by sensor interrupt.
*
* To Be Done:	None
*
*-Date----------Author------Description--------------------------------------
* 11/14/10		HarjitS     Created
*/

void vAlignTurnU(void)
{
    //
    // The LMCmdUTurn wants the data in mm in sign + 5.2 format with saturation.
    // Thes sensor data is in mm, 8.8 format, just convert format.
    //
    vLogCmd(LMCmdUTurn);
    vPutLog(BYTE0(slConvp8To5p2(slError)));


    vFilterLogCmd(LMCmdError);
    vPutLog(LMPlyErrorUTurnAlignErrorTooHigh);

	return;
}

/**************************************************************************
* Routine: 		vDoTurnU
*
* Purpose: 		Run the inplace Uturn
*
* Arguments:	None
*
* RETURNS: 	    void
*
* Notes:	 	We need some motion routine to do the u-turn, so this is it.
*               ServoPrf.c calls this routine every mS if we are doing a u-turn.
*
*				We might want to change this to have ServoPrf.c call the sensor
*               routine	and have that call this routine so that we can use the
*               sensors to finish the turn with the mouse properly pointed.
*
* To Be Done:	Use sensor data to end the turn when the mouse has the desired
*               posture.
*
*-Date----------Author------Description--------------------------------------
* 10/09/10		HarjitS     Moved out the code from vMoveMouse
*/
void vDoTurnU(void)
{
	ubOrient = (ubOrient + ulTurnDeltaOrientTable[MTNCMDV_TURN_TYPE_UTURN]) & 0x07;

    //
    // Update the coordinate of the mouse but do it *before* the orientation
    // is updated. For a Uturn it is meaningless but we do it to ensure that
    // generate the coordinate event for logging of the orientation
    //
    vUpdateubCoordCalcXY();

    vLogCmd(LMCmdOrient);
    vPutLog(ubOrient);

	return;
}

static void vSetupStopInCenter(void)
{

    vLogCmd(LMCmdError);
    vPutLog(LMPlyErrorSetupStopInCenter1);

    vLogCmd(LMCmdError);
    vPutLog(LMPlyErrorSetupStopInCenter2);

	return;
}

void vMoveMouse(void)
{
	// get the motion command and the next one for the motion code to
	// setup the profiler for the end of the motion
	ubMtnCmd = *pubMotionCmd++;
	ubNextMtnCmd = *pubMotionCmd;

    vLogCmd(LMCmdMove);
    vPutLog(ubMtnCmd);

}
