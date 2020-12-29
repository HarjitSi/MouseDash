/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:
*
* Written By:  	Harjit Singh
*
* Date:        	11/9/97, 02/14/10
*
* Purpose:     	This file is the header file for all mouse specific constants
*
* Notes:       	We are not going to move some of the HW stuff here because
*				it is too tightly coupled with the driver code.
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

#ifndef INCZVICnstH
#define INCZVICnstH

#include "Common\Include\types.h"
#include "sensors\sensors.h"
#include "Common\loop\servoprf.h"
#include <core_cm3.h>

// name of mouse
#define	MOUSE_NAME	" ZVI"

#define	SAVE_N_DIS_ALL_INT		ULong _ulIrqStatus = __get_PRIMASK();__disable_irq()
#define	RESTORE_INT				__set_PRIMASK(_ulIrqStatus)

// interrupt priority levels for different modules
#define	ENCODER_INT_PRIORITY	(1)		// The encoder increment/decrement high word
										// interrupts are currently the highest
										// priority interrupts

#define	I2C_INT_PRIORITY		(2)		// The interrupt priority - at the time of this
										// writing, it is the second highest priority
										// interrupt because of the crappy I2C
										// implementation


#define	SYSTICK_INT_PRIORITY 	(4)		// The encoder high word increment
										// routine is higher priority (level 1)
										// level 0 is currently unused

#define	SPI_INT_PRIORITY		(5)		// The SPI interrupts can be interrupted
										// by the servo and other stuff

#define	SERVO_PROF_INT_PRIORITY	(SYSTICK_INT_PRIORITY)
										// The ADC interrupt goes off when
										// we are done reading the battery,
										// gyro and flash voltages and then
										// it runs the servo profiler.
										// Thus the ADC interrupt runs at
										// the same priority as the servo
										// timer interrupt's priority

#define	SENSOR_INT_PRIORITY		(SYSTICK_INT_PRIORITY)
										// The interrupt priority for the sensor
										// interrupt is the same as the
										// millisecond interrupt, the gyro
										// conv. interrupt.
										// The sensor interrupt runs the
										// motion code

#define	SPI_DMA_INT_PRIORITY	(SPI_INT_PRIORITY)
#define	SPI_TIMER_INT_PRIORITY	(SPI_INT_PRIORITY)
										// The interrupt priority for the SPI is
										// lower than the servo code because
										// we don't want it to interrupt the servo
										// code but the servo code can interrupt it


//	Forward loop constants from ZVILoop.mac
//
//	Model:
//				1142
//	h1(s) = ————————
//			 s(0.129s+1)
//
//      TargPhase = 65
//
//		TargBW = bandwidth(h1f)*0.9
//
//		K1 = 5.53051 * 8192 = 45306
//		K2 = 5.3639 * 8192 = 43941
//		K3 = 0.621167 * 8192 = 5089
//

//	Rotation loop constants from ZVILoop.mac
//
//	Model:
//				576.2
//	h1(s) = ————————
//			 s(0.075s+1)
//
//		TargBW = bandwidth(h1f)*0.9
//
//		K1 = 5.33937 * 8192 = 43740
//		K2 = 5.17273 * 8192 = 42375
//		K3 = 0.674937 * 8192 = 5529
//
//		// detuned at APEC: got rid of 0.9 above and changed phase margin to 45
//		K1 = 4.17088 * 8192 = 34168
//		K2 = 3.93162 * 8192 = 32208
//		K3 = 0.772058 * 8192 = 6325
//
//      // below rotation numbers are with TargPhase = 80

#define	KERR_N_FWD			(45306)				// forward servo E(n) gain * 8192
#define	KERR_N_1_FWD		(43941)				// forward servo E(n-1) gain * 8192
#define	K_Y_N_1_FWD			(5089)				// forward servo Y(n-1) gain * 8192

//
// 80 degree phase margin with 0.9 multiplier loop coefficients
//
#define	KERR_N_ROT			(74879)				// rotational servo E(n) gain * 8192
#define	KERR_N_1_ROT		(73706)				// rotational servo E(n-1) gain * 8192
#define	K_Y_N_1_ROT			(3597)				// rotational servo Y(n-1) gain * 8192

												// error exceeds this, then skip
												// acceleration
												// The servo loop code has +/-4095
												// of guard band
												// TODO: This needs to be
												// smooth... Going in and out
												// of this makes the mouse
												// chatter and unstable

//
// See note book on 9/24/10 for details on the computation
//
// Acceleration: 	 6.8 * Slope: 9.8 = 15.16 => scaled to 15.13
// Velocity: 		11.8 * Slope: 3.8 = 14.16 => scaled to 14.13
//
// All the acceleration and velocity slope/intercepts are Vbat compensated i.e. add
// to the loop output before Vbat has been compenstated so that these can be
// scaled by the Vbat compensation.
//

// based on columns B and C of FwdStep summary i.e. no Vbat comp.

#define	FWD_AFF_SLOPE				(24165)		// 9.8 forward motor acceleration feed
												// forward slope
#define	FWD_AFF_INTERCEPT			(741335)	// 8.16 forward motor acceleration feed
												// forward intercept

#define	FWD_DFF_SLOPE				(21905)		// 9.8 forward motor deceleration feed

#define	FWD_VFF_SLOPE				(215)		// 3.8 forward motor velocity feed
												// forward slope

#define	FWD_VFF_INTERCEPT			(1278402)	// 9.16 forward motor velocity feed
												// forward intercept


#define	FWD_AFF_VFF_SHIFT			(3)			// scale the forward motor velocity
												// feed forward and acceleration
												// feed forward by this amount
												// to normalize it with the output
												// of the loop

#define	ROT_AFF_SLOPE				(15714)		// 9.8 rotational motor acceleration feed
												// forward slope
#define	ROT_AFF_INTERCEPT			(3565537)	// 9.16 rotational motor acceleration feed
												// forward intercept

#define	ROT_VFF_SLOPE				(365)		// 3.8 rotational motor velocity feed
												// forward slope
#define	ROT_VFF_INTERCEPT			(9719809/10)
                                                // 8.16 rotational motor velocity feed
												// forward intercept

//
// This section contains constants that affect the output of the loop and profiler
//

#define	PWM_MAX_OUTPUT		        (PWM_PERIOD)// used to limit the maximum
	   											// PWM output

#define	VBAT_NORM			        (15569)		// 12.0 this is Vbat when we did the
												// step response testing

#define	VBAT_CUTOFF_THRESHOLD	    (11702)		// 6.0V cut off - see "analog stuff" tab
												// for calculation details

#define	VBAT_CUTOFF_COUNT	        (100)		// Battery voltage has to be below
												// VBAT_CUTOFF_THRESHOLD for
												// this many ms before we'll
												// recognize the battery as being
												// too low.
#define CONV_VBAT_CNTS_TO_VOLTS(s)  ((s*32+1000)/2000+1)
                                                // convert VBat ADC * 4 to
                                                // 3.5 Volts

#define	CRASH_FWD_ERROR_THRESHOLD	(1500)		// forward error threshold
#define	CRASH_ROT_ERROR_THRESHOLD	(8*COUNTS_PER_DEGREE)
                                                // rotation servo error threshold
#define	CRASH_FWD_ROT_ERROR_COUNT	(25)		// crash detection time - the mouse
												// servo error has to be exceeded
												// for this much time before
												// a crash is declared

//
// This section contains constants that define the motion
//

#define	COUNTS_PER_CELL				(50908L)	// encoder counts per cell

#define COUNTS_PER_CELL_DFWD		(36608L)	// encoder counts per diag
												// forward move

#define	MM_PER_CELL					(178)		// this is to make the ratio of
												// COUNTS_PER_CELL and
												// MM_PER_CELL an integer

#define MM_PER_CELL_DFWD			(128)		// this is to make the ratio of
												// COUNTS_PER_CELL_DFWD
												// and MM_PER_CELL_DFWD
												// an integer

#define	COUNTS_PER_MM				((COUNTS_PER_CELL+MM_PER_CELL/2)/MM_PER_CELL)
												// encoder counts per mm

#if(COUNTS_PER_CELL!=MM_PER_CELL*COUNTS_PER_MM)
#error COUNTS_PER_CELL != MM_PER_CELL * COUNTS_PER_MM
#endif

#if(COUNTS_PER_CELL_DFWD!=MM_PER_CELL_DFWD*COUNTS_PER_MM)
#error COUNTS_PER_CELL_DFWD != MM_PER_CELL_DFWD * COUNTS_PER_MM
#endif
#define	CALC_MM(x) 					((x+COUNTS_PER_MM/2)/COUNTS_PER_MM)
												// compute mm value for x

#define COUNTS_PER_DEGREE           ((UTURN_LENGTH + 180 / 2)/ 180)

#warning figure out why the COUNTS_PER_RADIAN is not coming up to be 8904/4
#define	COUNTS_PER_RADIAN			(8904L/4)
#define	COUNTS_PER_RADIAN1			((SLong)((COUNTS_PER_DEGREE*(57.3*256))/256/4))
                                                // counts per radian
												// /4 added to match ZV gain

#define	GYRO_ZERO_ROT_REF			(8163)		// gyro zero rotation reference
												// "C:\Depot\ZVI\Data\Gyro\GyroStill_Direc0_Direc180.xlsx""

//#define	GYRO_ENC_GAIN				((SLong)((1<<ROT_PROF_ACT_ROT_POS_SHIFT)/(36.4529432)+0.5))
#define	GYRO_ENC_GAIN					(((1L<<ROT_PROF_ACT_ROT_POS_SHIFT)*2L)/83L)
												// used the 768 PWM value because that is
												// the only one that has a decent gyro and
												// encoder match

#define	GYRO_DEAD_BAND				(3*14)		// if the gyro reading is within
												// this much of the
												// GYRO_ZERO_ROT_REF, don't
												// bother doing anything
												// we want to have a deadband of
												// 3 sigma. The stdev is 14 from
                                                // "C:\Depot\ZVI\Data\Gyro\GyroStill_Direc0_Direc180.xlsx""

#define WHEEL_INPUT_THRESHOLD       (5*COUNTS_PER_DEGREE)
                                                // when the user moves the wheel
                                                // by this amount, change to the
                                                // next/previous menu item

#define MENU_PWM_SETTING_TIME_SEC   (10)        // in the menu system, we can
                                                // turn on the motors for this
                                                // many seconds at a
                                                // particular PWM percentage

#define MENU_PWM_SETTING_PERCENTAGE (25)        // in the menu system, we can
                                                // turn on the motors at this
                                                // particular PWM percentage


//
// This section contains the constants that define the turn parameters.
// The calculations for the turn parameters can be found in
// ZVMouseDesign.xls->Turn Coef
//

#define TURN_ENTRY_SLOW_DOWN_DIST_MM    (MM_PER_CELL/4)
                                                // slow down the mouse to the
                                                // turn turn velocity this
                                                // much before the turn starts

#define TURN_ENTRY_SLOW_DOWN_DIST       (TURN_ENTRY_SLOW_DOWN_DIST_MM*COUNTS_PER_MM)

#define TURN_SLIDE_COUNT_SHIFT      (8)         // the slide count is an 8.8
                                                // value
//
// Turn: 45 deg. straight to diag. or 45 deg. diag. to straight
//
#define	TURN_45_ENTRY				(6157-4*COUNTS_PER_MM)
                                                // turn entry distance
#define	TURN_45_EXIT				(16878-4*COUNTS_PER_MM)
                                                // turn exit distance
#define	TURN_45_LEN					(37852)		// turn length
#define	TURN_45_DECEL_DIST			(0)			// distance into the turn we
												// can decelerate
#define	TURN_45_ACCEL_DIST			(0)			// distance into the turn we
												// can start accelerating
#define	TURN_45_FIN_VEL				(7926)		// turn velocity at 1m/s^2
												// centripetal acceleration
#define	TURN_45_COEF				(13611)		// turn profiler coefficient

#define	TURN_45_ENTRY_MM_CNT		(CALC_MM(COUNTS_PER_CELL-TURN_45_ENTRY))
												// Turn entry mm count for
												// when doing diag to straight
#define	TURN_45_EXIT_MM_CNT			(CALC_MM(COUNTS_PER_CELL_DFWD-TURN_45_EXIT))
												// Turn exit mm count for
												// when doing straight to diag

#define	TURN_45_PEG_RISING_EDGE		(0)			// during a turn a rising edge should
												// occur at this mm count
#define	TURN_45_PEG_FALLING_EDGE	(0)			// during a turn a fallinging edge should
												// occur at this mm count
#define TURN_45_SLIDE_COUNT         (COUNTS_PER_MM*0)
                                                // subtract this times accel
                                                // from turn entry at high
                                                // accelerations

//
// Turn: 90 deg. learning turn straight to straight
//
#define	TURN_90L_ENTRY				(28040-COUNTS_PER_CELL-1*COUNTS_PER_MM)
												// turn entry distance
#define	TURN_90L_EXIT				(TURN_90L_ENTRY)
												// turn exit distance
#define	TURN_90L_LEN				(39208)		// turn length
#define	TURN_90L_DECEL_DIST			(0)			// distance into the turn we
												// can decelerate
#define	TURN_90L_ACCEL_DIST			(0)			// distance into the turn we
												// can start accelerating
#define	TURN_90L_FIN_VEL			(4105)		// turn velocity at 1m/s^2
												// centripetal acceleration
#define	TURN_90L_COEF				(24495)		// turn profiler coefficient

#define	TURN_90L_ENTRY_MM_CNT		(CALC_MM(COUNTS_PER_CELL+TURN_90L_ENTRY))
												// Used for long corr when
												// there is a front wall
#define	TURN_90L_EXIT_MM_CNT		(CALC_MM(-TURN_90L_EXIT))
												// Turn exit mm count
												// when finishing the turn

#define	TURN_90L_PEG_RISING_EDGE	(0)			// during a turn a rising edge should
												// occur at this mm count
#define	TURN_90L_PEG_FALLING_EDGE	(0)			// during a turn a fallinging edge should
												// occur at this mm count
#define TURN_90L_SLIDE_COUNT        (COUNTS_PER_MM*0)
                                                // subtract this times accel
                                                // from turn entry at high
                                                // accelerations

//
// Turn: 90 deg. speed run turn straight to straight
//
#define	TURN_90S_ENTRY				(12942)		// turn entry distance
#define	TURN_90S_EXIT				(12942)		// turn exit distance
#define	TURN_90S_LEN				(64158)		// turn length
#define	TURN_90S_DECEL_DIST			(0)			// distance into the turn we
												// can decelerate
#define	TURN_90S_ACCEL_DIST			(0)			// distance into the turn we
												// can start accelerating
#define	TURN_90S_FIN_VEL			(6717)		// turn velocity at 1m/s^2
												// centripetal acceleration
#define	TURN_90S_COEF				(5570)		// turn profiler coefficient
                                                // left wants 5550
                                                // right wants 5590

#define	TURN_90S_ENTRY_MM_CNT		(CALC_MM(COUNTS_PER_CELL-TURN_90S_ENTRY))
												// Isn't needed for this
												// turn because the turn starts
												// at zero
#define	TURN_90S_EXIT_MM_CNT		(CALC_MM(COUNTS_PER_CELL-TURN_90S_EXIT))
												// Turn exit mm count
												// when finishing the turn

#define	TURN_90S_PEG_RISING_EDGE	(0)			// during a turn a rising edge should
												// occur at this mm count
#define	TURN_90S_PEG_FALLING_EDGE	(0)			// during a turn a fallinging edge should
												// occur at this mm count
												// this is the mm count
#define TURN_90S_SLIDE_COUNT        (COUNTS_PER_MM*0)
                                                // subtract this times accel
                                                // from turn entry at high
                                                // accelerations

//
// Turn: 90 deg. diag. to diag.
//
#define	TURN_90D_ENTRY				(11296)		// turn entry distance
#define	TURN_90D_EXIT				(11296)		// turn exit distance
#define	TURN_90D_LEN				(41822)		// turn length
#define	TURN_90D_DECEL_DIST			(0)			// distance into the turn we
												// can decelerate
#define	TURN_90D_ACCEL_DIST			(0)			// distance into the turn we
												// can start accelerating
#define	TURN_90D_FIN_VEL			(4379)		// turn velocity at 1m/s^2
												// centripetal acceleration
#define	TURN_90D_COEF				(20183)		// turn profiler coefficient

#define	TURN_90D_ENTRY_MM_CNT		(CALC_MM(COUNTS_PER_CELL_DFWD-TURN_90D_ENTRY))
												// Turn exit mm count
												// when finishing the turn
#define	TURN_90D_EXIT_MM_CNT		(CALC_MM(COUNTS_PER_CELL_DFWD-TURN_90D_EXIT))
												// Turn exit mm count
												// when finishing the turn

#define	TURN_90D_PEG_RISING_EDGE	(0)			// during a turn a rising edge should
												// occur at this mm count
#define	TURN_90D_PEG_FALLING_EDGE	(0)			// during a turn a fallinging edge should
												// occur at this mm count
												// this is the mm count
#define TURN_90D_SLIDE_COUNT        (0)         // subtract this times accel
                                                // from turn entry at high
                                                // accelerations

//
// Turn: 135 deg. straight to diag. or 135 deg. diag. to straight
//
#define	TURN_135_ENTRY				(12806-3*COUNTS_PER_MM)
                                                // turn entry distance
#define	TURN_135_EXIT				(6470+4*COUNTS_PER_MM)
                                                // turn exit distance
#define	TURN_135_LEN				(71503)		// turn length
#define	TURN_135_DECEL_DIST			(0)			// distance into the turn we
												// can decelerate
#define	TURN_135_ACCEL_DIST			(0)			// distance into the turn we
												// can start accelerating
#define	TURN_135_FIN_VEL			(4991)		// turn velocity at 1m/s^2
												// centripetal acceleration
#define	TURN_135_COEF				(6058)		// turn profiler coefficient

#define	TURN_135_ENTRY_MM_CNT		(CALC_MM(COUNTS_PER_CELL-TURN_135_ENTRY))
												// Turn entry mm count for
												// when finishing the turn
#define	TURN_135_EXIT_MM_CNT		(CALC_MM(COUNTS_PER_CELL_DFWD-TURN_135_EXIT))
												// Turn exit mm count
												// when finishing the turn

#define	TURN_135_PEG_RISING_EDGE	(0)			// during a turn a rising edge should
												// occur at this mm count
#define	TURN_135_PEG_FALLING_EDGE	(0)			// during a turn a fallinging edge should
												// occur at this mm count
												// this is the mm count
#define TURN_135_SLIDE_COUNT        (0)         // subtract this times accel
                                                // from turn entry at high
                                                // accelerations

//
// Turn: 180 deg. straight to straight
//
#define	TURN_180_ENTRY				(12942-14*COUNTS_PER_MM)
                                                // turn entry distance
#define	TURN_180_EXIT				(TURN_180_ENTRY)
                                                // turn exit distance
#define	TURN_180_LEN				(106498)	// turn length
#define	TURN_180_DECEL_DIST			(0)			// distance into the turn we
												// can decelerate
#define	TURN_180_ACCEL_DIST			(0)			// distance into the turn we
												// can start accelerating
#define	TURN_180_FIN_VEL			(5575)		// turn velocity at 1m/s^2
												// centripetal acceleration
#define	TURN_180_COEF				(2435)		// turn profiler coefficient
                                                // right wants 2445

#define	TURN_180_ENTRY_MM_CNT		(CALC_MM(COUNTS_PER_CELL-TURN_180_ENTRY))
												// Isn't needed for this
												// turn because the turn starts
												// at zero
#define	TURN_180_EXIT_MM_CNT		(CALC_MM(COUNTS_PER_CELL-TURN_180_EXIT))
												// Turn exit mm count
												// when finishing the turn

#define	TURN_180_PEG_RISING_EDGE	(0)			// during a turn a rising edge should
												// occur at this mm count
#define	TURN_180_PEG_FALLING_EDGE	(0)			// during a turn a fallinging edge should
												// occur at this mm count
												// this is the mm count
#define TURN_180_SLIDE_COUNT        (0)         // subtract this times accel
                                                // from turn entry at high
                                                // accelerations

//
// Turn: Uturn 180 deg. straight to straight
//
// This turn is different than the rest because it is an inplace turn. Only the
// Entry and Exit entries are interesting and needed by vForward().
//
#define	TURN_UTRN_ENTRY				(0)			// turn entry distance
#define	TURN_UTRN_EXIT				(0)			// turn exit distance
#define	TURN_UTRN_LEN				(0)			// turn length
#define	TURN_UTRN_DECEL_DIST		(0)			// distance into the turn we
												// can decelerate
#define	TURN_UTRN_ACCEL_DIST		(0)			// distance into the turn we
												// can start accelerating
#define	TURN_UTRN_FIN_VEL			(0)			// turn velocity at 0.10g
												// centripetal acceleration
#define	TURN_UTRN_COEF				(0)			// turn profiler coefficient

#define	TURN_UTRN_ENTRY_MM_CNT		(0)			// Doesn't apply for this turn
#define	TURN_UTRN_EXIT_MM_CNT		(0)			// mm count on finishing turn
#define	TURN_UTRN_PEG_RISING_EDGE	(0)			// Doesn't apply for this turn
#define	TURN_UTRN_PEG_FALLING_EDGE	(0)			// Doesn't apply for this turn
#define TURN_UTRN_SLIDE_COUNT       (0)         // Doesn't apply for this turn

// The conversion factors for position and velocity are in the Encoder and
// Profiler Variables tab of ZVIMouseDesign.xls.
//
// To convert 2*Cnts/ms^2 to m/s^2: 3.477
// To convert m/s^2 to 2*Cnts/ms^2: 0.288
//
// To convert 2*Cnts/ms to m/s: 0.003477
// To convert m/s to 2*Cnts/ms: 287.602


#define	ACCEL_MPSS_CPMSS			(18848)		// 16:0.16 convert acceleration
												// 			from 8.8 m/s^2 to
												// 			6.16 2*Cnts/ms^2

#define	ACCEL_MPSS_CPMSS_SHIFT		(8)			// Conversion factor is 16:0.16
												// and acceleration is 16:8.8
												// which means we have to
												// shift the product right by
												// this many bits to end up
												// with an acceleration of
												// xx:y.16

#define	CONV_ACCEL_MPSS_CPMSS(x)	(((x)*ACCEL_MPSS_CPMSS)>>ACCEL_MPSS_CPMSS_SHIFT)

#define VEL_MPS_TO_CPMS				(73626)		// 17:9.8 convert velocity
												//			from 3.8 m/s to
												//			11.16 2*Cnts/ms

#define CONV_VEL_MPS_TO_CPMS(x)		((x)*VEL_MPS_TO_CPMS);

#define CONV_VEL_CPMS_TO_MPS(x)     ((x)/VEL_MPS_TO_CPMS)
                                                // CPMS velocity is 11.16
                                                // So, divide by the
                                                // MPS_TO_CPMS constant (3.8)
                                                // to get it to 3.8

#define	UTURN_ACCEL					(0x00008000L)
												// 0.5 c/ms^2 = 813 deg/sec
												// see calc. in design spreadsheet
												// Profiler Variables tab

#define	UTURN_LENGTH				(28116)		// rotational counts to do a Uturn

#define	UTURN_ALIGN_START			(15)		// wait for this long after forward
												// completes before trying to
												// align it
#define	UTURN_ALIGN_STOP			(UTURN_ALIGN_START+200)
												// for up to 200 ms after the turn has
												// completed, try to align it

#define	UTURN_START_DELAY			(UTURN_ALIGN_STOP+150)
												// wait for this long before starting
												// the uturn
#define	UTURN_COMPLETE_DELAY		(UTURN_START_DELAY+150)
												// wait for this long after the
												// Uturn before starting the
												// next move

#define UTURN_ALIGN_ERROR_OFFSET	(0)			// sensor delta offset to make
												// uturn straight

#define UTURN_ALIGN_ERROR_STOP		(3 << SENSOR_DIST_SHIFT)
                                                // if the sensors are within +/- 1mm
												// then stop correcting

#define	UTURN_ALIGN_ERROR_TOO_HIGH	(0x2000)	// if the difference in discance between,
												// the left and right sensor is more
												// 32mm, then don't do uturn
												// alignment because something
												// must be really wrong

#define	UTURN_ALIGN_DELTA_POS		(12<<ROT_PROF_ROT_POS_SHIFT)
												// 12 counts/ms => correct at
												// 86deg/s

//
// Sensor constants
//
// P33_* is for when the drive level is 33
// P66_* is for when the drive level is 66
//
#define	P33_OFFSET_LEFT_DIAG		(-123572L)	// left diag sensor offset
#define	P33_SLOPE_LEFT_DIAG			(333132L)	// left diag sensor slope
#define	P33_INTERCEPT_LEFT_DIAG		(-101)		// left diag sensor intercept

#define	P66_OFFSET_LEFT_DIAG		(-138618L)	// left diag sensor offset
#define	P66_SLOPE_LEFT_DIAG			(301177L)	// left diag sensor slope
#define	P66_INTERCEPT_LEFT_DIAG		(2906)		// left diag sensor intercept

#define	P33_OFFSET_RIGHT_DIAG		(-132084L)	// right diag sensor offset
#define	P33_SLOPE_RIGHT_DIAG		(283379L)	// right diag sensor slope
#define	P33_INTERCEPT_RIGHT_DIAG	(3738)		// right diag sensor intercept

#define	P66_OFFSET_RIGHT_DIAG		(-140313L)	// right diag sensor offset
#define	P66_SLOPE_RIGHT_DIAG		(251306L)	// right diag sensor slope
#define	P66_INTERCEPT_RIGHT_DIAG	(7795)		// right diag sensor intercept

#define	P33_OFFSET_LEFT_FRONT		(-107956L)	// left front sensor offset
#define	P33_SLOPE_LEFT_FRONT		(270939L)	// left front sensor slope
#define	P33_INTERCEPT_LEFT_FRONT	(2253)		// left front sensor intercept

#define	P66_OFFSET_LEFT_FRONT		(-121612L)	// left front sensor offset
#define	P66_SLOPE_LEFT_FRONT		(226559L)	// left front sensor slope
#define	P66_INTERCEPT_LEFT_FRONT	(9193)		// left front sensor intercept

#define	P33_OFFSET_RIGHT_FRONT		(-169128L)	// right front sensor offset
#define	P33_SLOPE_RIGHT_FRONT		(284547L)	// right front sensor slope
#define	P33_INTERCEPT_RIGHT_FRONT	(1108)		// right front sensor intercept

#define	P66_OFFSET_RIGHT_FRONT		(-176204L)	// right front sensor offset
#define	P66_SLOPE_RIGHT_FRONT		(255460L)	// right front sensor slope
#define	P66_INTERCEPT_RIGHT_FRONT	(5501)		// right front sensor intercept

#define	MIN_LEFT_DIAG   			(578)		// distance noise level is very
                                                // high if we don't have at least
												// this much light power

#define	MIN_RIGHT_DIAG  			(554)		// distance noise level is very
                                                // high if we don't have at least
												// this much light power

#define	MIN_LEFT_FRONT  			(1113)		// distance noise level is very
                                                // high if we don't have at least
												// this much light power

#define	MIN_RIGHT_FRONT 			(978)		// distance noise level is very
                                                // high if we don't have at least
												// this much light power

#define	FRONT_DRIVE_LOW_DIST    	(49<<SENSOR_DIST_SHIFT)
												// if the sensor distance is
												// lower than this, then use
												// the low emitter drive setting

#define	FRONT_DRIVE_HIGH_DIST       (51<<SENSOR_DIST_SHIFT)
												// if the sensor distance is
												// lower than this, then use
												// the low emitter drive setting

#if (FRONT_DRIVE_LOW_DIST > FRONT_DRIVE_HIGH_DIST)
#error FRONT_DRIVE_LOW_DIST should be less than FRONT_DRIVE_HIGH_DIST
#endif

#define	MAX_DIAG_SENS_DIST			(29978)		// the diagonal sensors have to be
												// less than this value for us
												// to consider using them.
												// When the mouse is against a
												// wall the opposide side reads
												// 148.9 (theoretically), this is
												// rounded up. See notebook
												// 10/16/10

#define	MAX_FWD_SENS_DIST			(28186)		// the front sensors have to be
												// less than this value for us
												// to consider using them.

#define	MIN_SIDE_WALL_DIAG_SENSOR_READING	(275)
												// if the sensor power is more
												// than this, then we have a
												// near wall

#define	NUM_FRONT_WALL_SENSORS		(2)			// number of front wall sensors

#define MIN_FRONT_WALL_SENSOR_READING	(350)	// more than this, then we have
												// a wall, less than this and we
												// dont

#define DIAG_SENSOR_ANGLE_SINE     (147)        // the diagonal sensor has an
#if(SENSOR_DIST_SHIFT != 8)
#error DIAG_SENSOR_ANGLE_SINE as defined above assumes SENSOR_DIST_SHIFT == 8
#endif

#define LAT_CORR_PROPORTIONAL_GAIN      (COUNTS_PER_DEGREE*2)
                                                // lateral correction PD loop's
                                                // proportional gain

#define LAT_CORR_DERIVATIVE_GAIN        (COUNTS_PER_DEGREE)
//#define LAT_CORR_DERIVATIVE_DIVIDE      (3)     // lateral correction PD loop's
#define LAT_CORR_DERIVATIVE_DIVIDE      (30)     // lateral correction PD loop's
                                                // derivative gain divide
                                                // factor

#define	LAT_CORR_FWD_OFFSET_TARG_CNT	(82<<SENSOR_DIST_SHIFT)
												// the value of the sensors when
	   											// the mouse is centered

#define LAT_CORR_FWD_SENSOR_OFFSET		(0<<SENSOR_DIST_SHIFT)
												// the right sensor reads high
												// by this much and the left
												// sensor reads low by this
												// much

#define	LAT_CORR_FWD_DEADBAND			(1<<SENSOR_DIST_SHIFT)
												// if the lateral error is
												// within this amount,
												// then do no correction

#define LAT_CORR_ERROR_MAX			    (20<<SENSOR_DIST_SHIFT)
												// limit maximum orthogonal lateral
												// correction to this

#define	LAT_CORR_DIAG_OFFSET_TARG_CNT	(57<<SENSOR_DIST_SHIFT)
												// the value of the sensors when
	   											// the mouse is centered

#define LAT_CORR_DIAG_OFFSET_MAX			(8192*COUNTS_PER_RADIAN)
												// limit maximum diagonal lateral
												// correction to this

#define	DIAG_CORR_FRONT_SENS_THRESHOLD	(200)	// if the diagonal sensor sees
												// anything greater than this
												// then we need to start
												// correction

#define DIAG_LAT_CORR_SENSOR_GAIN       (10)    // used to convert front sensor
                                                // power to corrector input
                                                // for diagonal moves

#define	MIN_LONG_CORR_PEG			(500)		// the edge detection logic has to
												// a difference of this much
												// between the max and min
												// edge detected value to
												// declare a peg/edge

#define	MIN_DIAG_LONG_CORR			(400)		// the diagonal peak detection
												// logic has to see a sensor
												// reading of this much to
												// consider a peak as existing

#define	FWD_PEG_CENTER_FE_OFFSET	(2800+5*COUNTS_PER_MM)
                                                // a falling edge occurs this many
												// counts after the center of the
												// peg

#define	DIAG_PEG_CENTER_PEAK_OFFSET	(-11*COUNTS_PER_MM)
												// The peg power peaks this many
												// counts after the mouse has moved
												// into a cell
												// The negative means that the peak
												// happens 7mm after the mouse
												// has moved past the diag. edge

#define	LEFT_LONG_CORR_FWD_OFFSET_TARG_CNT  (LAT_CORR_FWD_OFFSET_TARG_CNT+(0<<SENSOR_DIST_SHIFT))
#define	RIGHT_LONG_CORR_FWD_OFFSET_TARG_CNT (LAT_CORR_FWD_OFFSET_TARG_CNT+(0<<SENSOR_DIST_SHIFT))
												// the value of the sensors when
	   											// the mouse is centered

#define LONG_CORR_MAX				(35*COUNTS_PER_MM)
												// if the longitudinal correction
												// is more than this, then limit
												// it to this

#define	LONG_CORR_TO_CENTER_FRONT_WALL_APEC	(14672)
#define	LONG_CORR_TO_CENTER_FRONT_WALL_HOME	(14453)
#define	LONG_CORR_TO_CENTER_FRONT_WALL_TAIWAN	(14939)
#define	LONG_CORR_TO_CENTER_FRONT_WALL	(43<<SENSOR_DIST_SHIFT)
												// This is for Taiwan based on one wall at home
												// when the mouse is in the center
												// of a cell the front wall is this
												// many mm * 256 away

#define	LONG_CORR_LEARNING_TURN_FRONT_WALL (((MM_PER_CELL-TURN_90L_ENTRY_MM_CNT)<<SENSOR_DIST_SHIFT)+LONG_CORR_TO_CENTER_FRONT_WALL)
												// when the mouse is ready to do
												// a learning turn in a cell with a
												// front wall, this is how many
												//  mm * 256 away it is

#define	LONG_CORR_WALL_NO_WALL_CNT	(2+1)		// We want to the first number
												// of walls / no walls to
												// declare a falling / rising
												// edge.
												// The +1 is to allow an errant
												// reading

#define	LONG_CORR_LOOK_FE_ADD_DIST		(10)    // If we are going to turn and
												// haven't seen the falling
												// edge, look some more

#ifdef DIFFERENCE_LONG_CORR_EDGE_TYPE
#define	EDGE_SAMPLE_WALL_FROM_PEAK		(20)	// look at the sensor reading this much
												// before and after the peak to
												// figure out if we had a wall or
												// not
#endif

												// if we change the learning motion
												// parameters, rebuild the code with
												// ENABLE_FWD_LOG_OUTPUT
												// and then check that we can
												// decelerate and stop reasonably
#define	LEARNING_ACCEL_MPSS			(0x0500)	// 16: 8.8 set acceleration to 3.0 m/s^2
#define	LEARNING_DECEL_MPSS			(0x0500)	// 16: 8.8 set deceleration to 3.0 m/s^2
#define	LEARNING_MAX_VEL_MPS		(0x00D0)	// 16: 8.8 set max.vel to 0.81 m/s

#define	BACKTRACKING_MAX_VEL_MPS_INC (0x0100)	// 16: 8.8 back track at 1 m/s more
                                                //			than the learning
												//			velocity


#define	SPEEDRUN_ACCEL_MPSS			(0x0700)	// 16: 8.8 set acceleration to 6.0 m/s^2
#define	SPEEDRUN_DECEL_MPSS			(0x0700)	// 16: 8.8 set deceleration to 6.0 m/s^2
#define	SPEEDRUN_START_MAX_VEL_MPS	(0x0200)	// 16: 8.8 set max.vel to 2.0 m/s at

#define	SPEEDRUN_MAX_FWD_MAX_VEL_MPS (0x0480)	// 16: 8.8 don't let max.vel exceed
												//			4.5 m/s because that
												//			is what the HW is
												//			capable of
												// The real limit might be 5.Xm/s

#define	SPEEDRUN_DIAG_MAX_VEL_MPS	(0x0280)	// 16: 8.8 don't let max.vel exceed
												//			2.5 m/s on diagonals

#define	SPEEDRUN_ACCEL_MPSS_INC     (0x0380)	// increase acceleration by 3.5 m/s^2
#define	SPEEDRUN_DECEL_MPSS_INC     (0x0280)	// increase deceleration by 2.5 m/s^2
#define	SPEEDRUN_MAX_VEL_MPS_INC	(0x0100)	// increase max. vel by 2.00 m/s

												// due to current implementation
												// math constraints, the mult
												// must stay less than four bits
#define	SPEEDRUN_TURN_FWD_ACCEL_MULT	(6)		// make speedrun turn accel
#define	SPEEDRUN_TURN_FWD_ACCEL_DIV		(4)		// 1.00x forward accel

#define SPEEDRUN_TURN_ACCEL_LIMIT   (0x100*15)  // limit turns to no more than
                                                // 15.0 m/s^2

#define DECEL_MAX_MPSS              (0x100*15)  // never try to forward
                                                // decelerate harder than this
                                                // 15.0 m/s^2

#endif /* INCZVICnstH */

