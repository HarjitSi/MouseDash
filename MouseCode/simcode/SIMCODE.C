/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        SIMCODE.C
*
* Written By:  Harjit Singh
*
* Date:        01/14/96; 01/31/10
*
* Purpose:   This program will be used to initialize the SIM on the chip.
*			Adapting it from ZIV to ZV.
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

#include "Common\Include\types.h"

#include <stm32f10x.h>

#define SIMCODE
#include "simcode.h"

#include <core_cm3.h>
#include "Common\Include\dwt.h"

void vSetupCPUCycleCntr(void)
{
	// enable the trace and debug block so that we can use the clock cycle counting
	// register
	CoreDebug->DEMCR |= CoreDebug_DEMCR_TRCENA_Msk;

	// enable the clock cycle counter
	DWT->CTRL |= DWT_CTRL_CYCCNTENA;

	return;
}

inline ULong ulGetCycleCnt(void)
{
	return(DWT->CYCCNT);
}

inline void vDelayUS(ULong ulDly)
{
	ULong ulInitialCnt = ulGetCycleCnt();
	ULong ulDlyCnt = ulDly * CLK_CNT_PER_US;

	while (ulDlyCnt > (ulGetCycleCnt() - ulInitialCnt))
		;

	return;
}

inline void vDelayMS(ULong ulDly)
{
	vDelayUS(ulDly * 1000);

	return;
}
