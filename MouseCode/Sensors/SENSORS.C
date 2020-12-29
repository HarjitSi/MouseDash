/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 1995
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:			sensor.c
*
* Written By:	Harjit Singh
*
* Date:			Oct 25, 1995; Feb 1, 2010
*
* Purpose:		This file contains code that will read the sensors.
*
* Notes:		The sensors are *all* sensors i.e. wall, gyro, accel.
*
* To Be Done:	None
*
* Modification History:
*-Date--------Author-----Description--------------------------------------
* 09/03/11		Harjit		Adapting code from ZV to ZVI.
*							Removed accelerometer code.
*/


#define SENSORS_C

#include "Common\Include\types.h"

#include "Common\loop\log.h"

static inline void vSetFrontEmtDriveToLow(void)
{
    vLogCmd(LMCmdSensDriveLow);

    return;
}

static inline void vSetFrontEmtDriveToHigh(void)
{
    vLogCmd(LMCmdSensDriveHigh);

    return;
}

