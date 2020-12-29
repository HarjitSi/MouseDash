/**************************************************************************
* $Id$
* $Source$
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        types.h
*
* Written By:  Harjit Singh
*
* Date:        Sep 03, 2020
*
* Purpose:     Standard types used by my programs.
*
* Notes:
*
* To Be Done:
*
* Modification History:
* $Log$
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

#ifndef INCtypesH
#define INCtypesH

#include <stdint.h>

typedef uint8_t		UByte;
typedef int8_t   	SByte;
typedef uint16_t	UWord;
typedef int16_t 	SWord;
typedef uint32_t	ULong;
typedef int32_t		SLong;
typedef	uint64_t	ULongLong;
typedef	int64_t		SLongLong;

typedef enum {FALSE = 0, TRUE = !FALSE} Bool;

typedef uint32_t    BBool;

#define	MAX(a,b)	(((a)>(b))?(a):(b))
#define	MIN(a,b)	(((a)<(b))?(a):(b))

#define MAKE_MASK_FROM_ENUM(x)      (1<<(x-1))

#define	BYTE0(n)	((n)&0xff)
#define	BYTE1(n)	(((n)>>8)&0xff)
#define	BYTE2(n)	(((n)>>16)&0xff)
#define	BYTE3(n)	(((n)>>24)&0xff)

#define SATURATE(x,min,max)   if(x>max){x=max;}else if(x<min){x = min;}

#define NUM_ELEMENTS(x) (sizeof(x)/sizeof(x[0]))

#define BITBAND(var)  BBool var __attribute__ ((section ("bbalias")))

#endif /* INCtypesH */

