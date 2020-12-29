/*

            NOTICE OF COPYRIGHT
             Copyright (C) 2020
                Team Zeetah
            ALL RIGHTS RESERVED

	Module:
	Mapmaze.c

	Author:
	Harjit Singh

	Description:
	This module contains code that is used to map the maze for the mouse
	simulator.

	Date:
	June 28, 1992
	File created.

	Date:
	March 22, 1993
	Finally writing code!

	Date:
	March 30, 1993
	Fixing a bug in explore.  It wasn't using the correct maze when
	computing the fastest path in certain curcumstances.

	Date:
	April 15, 1993
	Adding routine to analyze a path.  It returns zero if there is an
	unexplored wall in the path else it returns the coordinate of the
	cell BEFORE the cell with the unexplored wall.  This should help
	us determine when to quit searching as well as crank when we know
	where we are going i.e. BACKTRACKING.

	Date:
	April 16, 1993
	Actually writing the code for path_analyze.

	Date:
	July 20, 1993
	Fixed bug when the mouse was returning to the start square after it
	was done learning.  It was computing the path using MazeOptim instead
	of MazePessim.

	Date:
	Feb. 24, 1995
	Added check in path_gen_and_motion_1() that checks to see if the current
	coordinate is the start or goal square. If it is, then it or's in the
	CURRENT_TO_START_BEST or CURRENT_TO_CENTER_BEST to status as appropriate.
	This fixes a bug where if the mouse was in the goal cell and it knew the
	best path to the start, it would lock up because it didnt know how to
	deal with the fact that it implicitly knew the best path to the center.
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


#include "Common\loop\log.h"

extern void vWaitConnectWriteLog(void);

// code snippet to log an error
void foo(void)
{
    SAVE_N_DIS_ALL_INT;
    vFilterLogCmd(LMCmdError);
    vPutLog(LMPlyErrorAnalyzePathDeltaPosZero);
    RESTORE_INT;

    return;
}


void foo2(void)
    // turn on the red LED
    RED_LED_ON;

    SAVE_N_DIS_ALL_INT;
    vFilterLogCmd(LMCmdError);
    vPutLog(LMPlyErrorNoPathToGoal);
    RESTORE_INT;

    return;
}

void vLogPath(UByte *pubPath, UByte ubPathLen)
{
    SAVE_N_DIS_ALL_INT;

    vLogCmd(LMCmdPath);

    vPutLog(ubPathLen);

	for (ULong ulI = 0 ; ulI < ubPathLen ; ulI++)
	{
		UByte ubCmd = *pubPath++;

        vPutLog(ubCmd);
	}

    RESTORE_INT;

	return;
}

void foo3(void)
{
    vLogPath(pubPathEnd, ubPathLenOptim);

    return;
}

void foo4(void)
{
    LOG_PRINTF("Fixed up path");

    return;
}
