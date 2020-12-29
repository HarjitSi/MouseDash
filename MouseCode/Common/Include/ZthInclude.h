/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2010
*				 Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File: 		ZthInclude.h
*
* Written By:	Harjit Singh
*
* Date: 		08/30/11
*
* Purpose:		This file is a header file that is used to indirectly include the actual mouse
*				constants file.
*
* Notes:
*
* To Be Done:
*
*-Date--------Author-----Description--------------------------------------
* 08/30/11		HarjitS		Created file.
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

#ifndef INCZthIncludeH
#define INCZthIncludeH

#ifdef MOUSE_ZTHV
#include "Include\ZVCnst.h"
#endif

#ifdef MOUSE_ZTHVI
#include "Include\ZVICnst.h"
#endif

#ifndef MOUSE_ZTHV
#ifndef MOUSE_ZTHVI
#error MOUSE_ZTHV and MOUSE_ZTHVI not defined
#endif
#endif

#endif /* INCZthIncludeH */




