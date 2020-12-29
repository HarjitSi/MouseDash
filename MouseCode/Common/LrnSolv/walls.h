/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 1995
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        Walls.H
*
* Written By:  Harjit Singh
*
* Date:        2/27/99
*
* Purpose:     This file contains the maze wall related routines
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

#ifndef INCwallsH
#define INCwallsH

#include "Common\Include\types.h"

#include "Common\lrnsolv\maze.h"

#define	WALL_NO_FRONT_LEFT_RIGHT		(0x00)
											// no walls exist
#define	WALL_FRONT						(0x01)
											// front wall exists
#define	WALL_LEFT						(0x02)
											// left wall exists
#define	WALL_RIGHT						(0x04)
											// right wall exists

#define	WALL_FRONT_LEFT_RIGHT			(WALL_FRONT|WALL_LEFT|WALL_RIGHT)

void vMarkCell(UByte ubCellCoord, UByte ubWalls);

#endif /* INCwallsH */



