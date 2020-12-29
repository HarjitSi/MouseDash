/*
	Module:
	Walls.c

	Author:
	Harjit Singh

	Description:
	This module contains code that 'reads the walls' as well as marks the
	maze.

	Date:
	March 20, 1993
	File created.
	Adding panic button support.

	Date:
	April 8, 1993
	Adding function that reads the maze map: bbMazeOptim and bbMazePessim
	and returns the wall status.

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

#define WALLS_C
#include "Common\Include\ZthInclude.h"

#include "Common\lrnsolv\walls.h"

#include "Common\motion\motion.h"

#include "simcode\simcode.h"

#include "Common\loop\log.h"


UByte ubConvWallsFLRToNSEW(UByte ubWalls)
{
	UByte ubWallMaze;

	ubWallMaze = MAPPED_NORTH | MAPPED_EAST | MAPPED_SOUTH | MAPPED_WEST;
											/* mapped all walls */
	switch (ubOrient)
	{
		case UBORIENT_NORTH:
			if (WALL_FRONT & ubWalls)
			{
				ubWallMaze |= WALL_NORTH;
			}

			if (WALL_RIGHT & ubWalls)
			{
				ubWallMaze |= WALL_EAST;
			}

			if (WALL_LEFT & ubWalls)
			{
				ubWallMaze |= WALL_WEST;
			}
			break;
		case UBORIENT_EAST:
			if (WALL_FRONT & ubWalls)
			{
				ubWallMaze |= WALL_EAST;
			}

			if (WALL_RIGHT & ubWalls)
			{
				ubWallMaze |= WALL_SOUTH;
			}

			if (WALL_LEFT & ubWalls)
			{
				ubWallMaze |= WALL_NORTH;
			}
			break;
		case UBORIENT_SOUTH:
			if (WALL_FRONT & ubWalls)
			{
				ubWallMaze |= WALL_SOUTH;
			}

			if (WALL_RIGHT & ubWalls)
			{
				ubWallMaze |= WALL_WEST;
			}

			if (WALL_LEFT & ubWalls)
			{
				ubWallMaze |= WALL_EAST;
			}
			break;
		case UBORIENT_WEST:
			if (WALL_FRONT & ubWalls)
			{
				ubWallMaze |= WALL_WEST;
			}

			if (WALL_RIGHT & ubWalls)
			{
				ubWallMaze |= WALL_NORTH;
			}

			if (WALL_LEFT & ubWalls)
			{
				ubWallMaze |= WALL_SOUTH;
			}
			break;
		default:
            vFilterLogCmd(LMCmdError);
            vPutLog(LMPlyErrorOrientBadInConvWalls);

			vDispChar(0, 'W');
			break;
	}

#if 0
#warning *************
#warning ************* ubConvWallsFLRToNSEW(): vDispChar
#warning *************
			vDisplayWalls(ubWalls);
#endif

	return (ubWallMaze);
}

void vMarkCell(UByte ubCellCoord, UByte ubWalls)
{
	UByte ubCellData;

	ubCellData = ubConvWallsFLRToNSEW(ubWalls);

	vAddPanic(ubCellCoord);

    //
    // log the wall info.
    //
    UByte ubLogWall = 0;

    if (ubCellData & WALL_NORTH)
    {
        ubLogWall |= LMPylMarkedNorthWall;
    }

    if (ubCellData & WALL_EAST)
    {
        ubLogWall |= LMPylMarkedEastWall;
    }

    if (ubCellData & WALL_SOUTH)
    {
        ubLogWall |= LMPylMarkedSouthWall;
    }

    if (ubCellData & WALL_WEST)
    {
        ubLogWall |= LMPylMarkedWestWall;
    }

    // write out the log mark wall command and the payload
    vLogCmd(LMCmdMarkedWalls);
    vPutLog(ubLogWall);

	//
	// dont mark outside walls
	//
	if(ubCellCoord < 0xf0)
		if(ubCellData & MAPPED_NORTH)
		{
			if(ubCellData & WALL_NORTH)
			{
				bbMazeOptim[ubCellCoord + WALL_OFF_N] = WALL_EXIST;
				bbMazeOptim[ubCellCoord + 0x10 + WALL_OFF_S] = WALL_EXIST;
			}
			else
			{
				bbMazePessim[ubCellCoord + WALL_OFF_N] = WALL_NONE;
				bbMazePessim[ubCellCoord + 0x10 + WALL_OFF_S] = WALL_NONE;
			}
		}

	if((ubCellCoord & 0x0f) != 0x0f)
		if(ubCellData & MAPPED_EAST)
		{
			if(ubCellData & WALL_EAST)
			{
				bbMazeOptim[ubCellCoord + WALL_OFF_E] = WALL_EXIST;
				bbMazeOptim[ubCellCoord + 0x01 + WALL_OFF_W] = WALL_EXIST;
			}
			else
			{
				bbMazePessim[ubCellCoord + WALL_OFF_E] = WALL_NONE;
				bbMazePessim[ubCellCoord + 0x01 + WALL_OFF_W] = WALL_NONE;
			}
		}

	if(ubCellCoord >= 0x10)
		if(ubCellData & MAPPED_SOUTH)
		{
			if(ubCellData & WALL_SOUTH)
			{
				bbMazeOptim[ubCellCoord + WALL_OFF_S] = WALL_EXIST;
				bbMazeOptim[ubCellCoord - 0x10 + WALL_OFF_N] = WALL_EXIST;
			}
			else
			{
				bbMazePessim[ubCellCoord + WALL_OFF_S] = WALL_NONE;
				bbMazePessim[ubCellCoord - 0x10 + WALL_OFF_N] = WALL_NONE;
			}
		}

	if((ubCellCoord & 0x0f) != 0x00)
		if(ubCellData & MAPPED_WEST)
		{
			if(ubCellData & WALL_WEST)
			{
				bbMazeOptim[ubCellCoord + WALL_OFF_W] = WALL_EXIST;
				bbMazeOptim[ubCellCoord - 0x01 + WALL_OFF_E] = WALL_EXIST;
			}
			else
			{
				bbMazePessim[ubCellCoord + WALL_OFF_W] = WALL_NONE;
				bbMazePessim[ubCellCoord - 0x01 + WALL_OFF_E] = WALL_NONE;
			}
		}

	return;
}
