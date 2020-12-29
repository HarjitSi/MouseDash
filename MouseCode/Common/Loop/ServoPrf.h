/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:			servoPrf.h
*
* Written By:	Harjit Singh
*
* Date:			02/15/10, 1/10/99
*
* Purpose:     	This file is the header file for the servo loop and profiler
*				and support routines
*
* Notes:
*
* To Be Done:
*
* Modification History:
* 02/15/20		Renamed from ServoSup.h to ServoPrf.h
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

#ifndef INCServoPrfH
#define INCServoPrfH
#include "Common\Include\types.h"

typedef struct sLoop
{
	SLong slError;								// 12.00 x(n)
	SLong slErrorLast;							// 12.00 x(n-1)
	SLong slOutputLast;							// 11.5	 y(n-1)
} Loop;

// To avoid mixing signed and unsigned variables, all variables have been
// changed to signed. Since the variables have sufficient range, it doesn't
// matter.

typedef struct sFwdProfiler
{
	SLong slFwdAccel, slFwdDecel;				//  6.16 acceleration/deceleration
	SLong slFwdVel;								// 11.16 velocity
	SLong slActVel;								// 11.00 actual current velocity
	SLongLong sllFwdPosFP;						// 32.16 position
	SLong slFwdPos;								// 32.00 position
	SLong slActPos;								// 32.00 actual current position
	SLong slTarPos;								// 32.00 target position
	SLong slMaxVel;								// 11.16 maximum velocity
	SLong slFinVel;								// 11.16 final velocity
	SLong slFinVelSq;							// 22.10 final velocity squared
	SLong slFeedFwd;                            // 18.13 feed forward
} FwdProfiler;

#define	FWD_PROF_FIN_VEL_SQ_SHIFT	(16+16-10)	// since velocity is 11.16,
												// this means after we square
												// velocity, we have to right
												// shift the result by this amount
												// to keep as much range as
												// possible but still fit within
												// 32 bits

#define FWD_PROF_DECEL_SHIFT		(16-10)		// Decel is 6.16 and DecelRqd
												// is 22.10, when comparing
												// the two, we need to shift
												// Decel by this amount

#define	CONV_TURN_VEL_SHIFT			(4+8)		// used to shift turn vel.
												// sqrt result to 11.16 for
												// max. vel and fin. vel
												// When we take the square root
												// of the Accel*radius, we were
												// at .8 and the square root makes
												// it a .4.
												// The result we want is 11.16, so
												// we need another .8 to get to .16

#define ROT_PROF_ROT_POS_SHIFT		(16)		// get integer portion of sllRotPosRP

#define CALC_FIN_VEL_SQ(a) (((SLongLong) a*a)>>FWD_PROF_FIN_VEL_SQ_SHIFT)

typedef struct sRotProfiler
{
	SLong slTurnStartCount;						// 32.00 forward position where the
												//       turn started
												//       rotational position where
												//       uturn started
	SLong slTurnLength;							// 17.00 length of the turn (Turn Coef
												//       of ZV design spreadsheet)
	SLong slTurnEntry;							// 32.00 turn entry distance used by
												//       the forward profiler when
												//       it has to adjust motion
												//       changes
	SLong slTurnExit;							// 32.00 turn exit distance used to
												//       communicate this info.
												//       to the forward profiler
	SLong slTurnExitMMCnt;						// 10.00 turn exit mm count used to
												//       communicate this info.
												//       to the forward profiler
	SLong slTurnCoef;							// 12.00 turn coefficient
	SLong slRotVel;								// 11.16 rotational velocity
	SLongLong sllRotPosRP;						// 32.16 desired rotational position
	SLong slRotPos;								// 32.00 desired rotational position
	SLongLong sllActRotPosRP;					// 32.16 actual rotational position
	SLong slActRotPos;							// 32.00 actual rotational position
	SLong slGyroVel;							// ??.?? gyro velocity
    SLong slFeedFwd;                            // 18.13 feed forward
} RotProfiler;

#ifdef SERVOPRF_C

#define	K_LOOP_SHIFT		(13)				// scale the output by 8192

#define	K_Y_N_1_SHIFT		(8)					// scale the output by 256 before
												// assigning it to output last

#define	K_VBAT_DIV_SHIFT	(10)				// scale VBat by this before doing
												// the divide

#define	K_VBAT_CALC_SHIFT	(5)					// scale inputs into VBat calculation
												// to have only 5 bits of fraction
												// before doing the VBat calc.

#define ROT_PROF_FWD_VEL_SHIFT	(16-5)			// convert FwdVel from 11.16 to 11.5 for
												// TurnCoef * FwdVel computation

#define	ROT_PROF_ROT_VEL_SHIFT	(49-16)			// convert RotVel from 60:11.49 to
												// 27:11.16

#define	FWD_FF_ACCEL_SCALE_SHIFT	(16-8)		// convert acceleration from 6.16 to
												// 6.8

#define FWD_VEL_FRAC_BITS           (16)        // number of fractional bits in slFwdVel

#define	FWD_FF_VEL_SCALE_SHIFT		(FWD_VEL_FRAC_BITS-8)
                                                // convert velocity from 11.16 to
												// 11.8

#define	FWD_AFF_VFF_OUTPUT_SHIFT	(3)			// scale the rotational motor velocity
												// feed forward and acceleration
												// feed forward output from 16.16
												// to 16.13 normalize it with
												// the output of the servo loop

#define	ROT_FF_ACCEL_SCALE_SHIFT	(16-8)		// convert acceleration from 6.16 to
												// 6.8

#define	ROT_FF_VEL_SCALE_SHIFT		(16-8)		// convert velocity from 11.16 to
												// 11.8

#define	ROT_AFF_VFF_OUTPUT_SHIFT	(3)			// scale the rotational motor velocity
												// feed forward and acceleration
												// feed forward output from 16.16
												// to 16.13 normalize it with
												// the output of the servo loop

Loop sFwdLoop;                                  // forward servo loop
Loop sRotLoop;									// rotation servo loop

FwdProfiler sFwdProf;							// forward profiler
RotProfiler sRotProf;							// rotation profiler

volatile ULong ulSysFlags;

volatile BITBAND(g_bbMotionComplete);
                                        // 1 = motion completed
                                        // 0 = motion in progress

volatile BITBAND(g_bbProfilingUTurn);
                                        // 1 = use uturn profiler to
                                        //      profile a uturn
                                        // 0 = don't do anything with the
                                        //      uturn profiler

volatile BITBAND(g_bbAlignUTurn);
                                        // 1 = do sensor based alignment of
                                        //      the mouse
                                         // 0 = don't do any alignment

volatile BITBAND(g_bbProfilingTurn);
                                        // 1 = profile turn
                                        // 0 = don't profile turn

volatile BITBAND(g_bbTurnRightnotLeft);
                                        // turn direction flag
                                        // 1 = right turn
                                        // 0 = left turn

volatile BITBAND(g_bbDbgReadSensorsEveryMS);
                                        // Since the ADCs are shared by
                                        // the servo and sensor routines,
                                        // this flag is used to read the
                                        // sensors every millisecond after
                                        // the servo code finishes running
                                        // 1 = read sensors every ms
                                        // 0 = default behavior

volatile BITBAND(g_bbFwdServoErrorTooHigh);
                                        // 1 = forward servo caused mouse to
                                        //      stop
                                        // 0 = forward servo did not cause
                                        //      the mouse to stop

volatile BITBAND(g_bbRotServoErrorTooHigh);
                                        // 1 = rotation servo caused mouse
                                        //      to stop
                                        // 0 = rotation servo did not cause
                                        //      the mouse to stop

volatile BITBAND(g_bbVBatTooLow);
                                        // 1 = battery voltage is too low,
                                        //      stop the mouse
                                        // 0 = battery voltage is fine

volatile BITBAND(g_bbPanicButtonPressed);
                                        // 1 = panic button pressed,
                                        // 0 = panic button not pressed

volatile BITBAND(g_bbPWMOverride);
                                        // disable the loop's PWM output
                                        // and override it with user set values
                                        // 1 = disable the loop's output
                                        // 0 = enable the loop's output

volatile BITBAND(g_bbMouseStopped);
                                        // one of the events that requires
                                        // the mouse to be stopped is true
                                        // 1 = some event requested mouse
                                        //      to be stopped - look at the
                                        //      flags to figure out which one(s)
                                        // 0 = no event has requested that
                                        //      the mouse be stopped

volatile BITBAND(g_bbEnableProfilerNServo);
                                        // 1 = profiler and servo enabled
                                        // 0 = profiler and servo disabled

volatile BITBAND(g_bbEnableRotServo);
                                        // 1 = rotational servo enabled
                                        // 0 = rotational servo disabled

volatile BITBAND(g_bbEnableFwdServo);
                                        // 1 = fwd drive servos enabled
                                        // 0 = fwd drive servos disabled

volatile BITBAND(g_bbEnableLatCorrector);
                                        // 1 = lateral correction enabled
                                        // 0 = lateral correction disabled

volatile BITBAND(g_bbEnableLongCorrector);
                                        // 1 = longitudinal correction enabled
                                        // 0 = longitudinal correction disabled

#else
extern	Loop sFwdLoop;			// forward servo loop
extern	Loop sRotLoop;					// rotation servo loop

extern	FwdProfiler sFwdProf;			// forward profiler
extern	RotProfiler sRotProf;			// rotation profiler

extern	volatile ULong ulSysFlags;

extern  volatile Bool g_bbMotionComplete;
extern  volatile Bool g_bbProfilingUTurn;
extern  volatile Bool g_bbAlignUTurn;
extern  volatile Bool g_bbProfilingTurn;
extern  volatile Bool g_bbTurnRightnotLeft;
extern  volatile Bool g_bbDbgReadSensorsEveryMS;
extern  volatile Bool g_bbFwdServoErrorTooHigh;
extern  volatile Bool g_bbRotServoErrorTooHigh;
extern  volatile Bool g_bbVBatTooLow;
extern  volatile Bool g_bbPanicButtonPressed;
extern  volatile Bool g_bbPWMOverride;
extern  volatile Bool g_bbMouseStopped;
extern  volatile Bool g_bbEnableProfilerNServo;
extern  volatile Bool g_bbEnableRotServo;
extern  volatile Bool g_bbEnableFwdServo;
extern  volatile Bool g_bbEnableLatCorrector;
extern  volatile Bool g_bbEnableLongCorrector;
#endif

// moved out here because it is used by other modules
#define	ROT_PROF_ACT_ROT_POS_SHIFT	(16)		// get integer portion of sllActRotPosRP

void vSuppressDeceleration(void);
void vEnableDeceleration(void);

void vResetVBatLow(void);

void vInitServoProfStruct(void);

void vZeroFwdPosProfTPU(void);

void vZeroRotPos(void);

void vSetPWMOverrideValue(SLong slLeftOutput, SLong slRightOutput);

SLong slFwdDistLeftGet(void);

SLong slDecelReqdGet(SLong slAdditionalDist);

SLong slFwdServoErrorGet(void);

SLong slRotServoErrorGet(void);

void vDisableProfilerNServo(void);

void vEnableProfilerNServo(void);

void vPropagateRotPos(SLong slDeltaPos);

void vServoProf(void);

#endif /* INCServoPrfH */
