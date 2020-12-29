/*
	Module:
	Maze.h

	Author:
	Harjit Singh

	Description:
	This module contains the include file for stuff that has anything to do
	with mazes.  It has some other stuff also because I was too lazy to
	create a separate header file.

	Date:
	March 22, 1993
	File created.

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

#ifndef MAZE_H
#define MAZE_H

#include "Common\Include\types.h"

#define	MAZE_ARRAY_SIZE	17	/* The maze array is 17 by 17 */

#ifndef MAZE_WIDTH
#define MAZE_WIDTH  (MAZE_ARRAY_SIZE -1)	/* Maze width, measured in unit squares */
#endif
#ifndef MAZE_HEIGHT
#define MAZE_HEIGHT (MAZE_ARRAY_SIZE -1)	/* Maze height, measured in unit squares */
#endif

/*
(MIN_MAZE_X,MIN_MAZE_Y) is the origin of the maze.  The origin is the unit
square in the southwest corner of the maze, since the maze is in quadrant
I of the maze coordinate system.  (MAX_MAZE_X,MAX_MAZE_Y) is the unit
square in the northeast corner of the maze.
*/

#define MIN_MAZE_X 0
#define MIN_MAZE_Y 0
#define MAX_MAZE_X (MAZE_WIDTH-1)
#define MAX_MAZE_Y (MAZE_HEIGHT-1)


#define	WALL_EXIST	0		/* wall exists */
#define	WALL_NONE	1		/* no wall exists */

                            // the packed versions are used when accessing
                            // the maze variable through the non-bit band
                            // aperture
#define WALL_EXIST_PACKED   0x00000000
#define WALL_NONE_PACKED    0xffffffff

#define WALL_OFF_E	0		/* wall offset for east walls in array */
#define WALL_OFF_N	256		/* wall offset for north walls in array */
#define WALL_OFF_W	512		/* west walls */
#define	WALL_OFF_S	768		/* south walls */

#define UNMAPPED     0x00   /* No edges have been mapped */

#define MAPPED_NORTH 0x80   /* North edge has been mapped */
#define MAPPED_EAST  0x40   /* East edge has been mapped */
#define MAPPED_SOUTH 0x20   /* South edge has been mapped */
#define MAPPED_WEST  0x10   /* West edge has been mapped */

#define MAPPED_ALL   (MAPPED_NORTH|MAPPED_EAST|MAPPED_SOUTH|MAPPED_WEST)

#define WALL_NORTH   0x08   /* North edge is a wall */
#define WALL_EAST    0x04   /* East edge is a wall */
#define WALL_SOUTH   0x02   /* South edge is a wall */
#define WALL_WEST    0x01   /* West edge is a wall */

#define	START_SQ	0x00	/* start square */
#define	GOAL_SQ		0x88	/* goal square */

#if GOAL_SQ != 0x88
#warning **********************************************************
#warning ******** FIX MAZE.H: GOAL_SQ TO 0x88 ********************
#warning **********************************************************
#endif

#endif


