/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        	Dwt.h
*
* Written By:  	Harjit Singh
*
* Date:        	08/02/2010
*
* Purpose:     	This file contains routines to enable the instruction counter
*               in the processor
*
* Notes:
*
* To Be Done:
*
* Modification History:
* 09/21/10		Created
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

// This functionality should be in core_cm3.h but it wasn't when this was written

/* Data Watchpoint and Trace unit (DWT) */

#define	DWT_CTRL_CYCCNTENA				((ULong)0x00000001)	// Cycle counter enable

typedef struct
{
	volatile ULong CTRL;
	volatile ULong CYCCNT;
} DWT_Type;

#define DWT_BASE		(0xE0001000)

#define	DWT				((DWT_Type *) DWT_BASE)
