/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        		Flash.h
*
* Written By:  	Harjit Singh
*
* Date:        	04/01/96
*
* Purpose:     	Header file for flash routines
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

#ifndef INCFlashH
#define INCFlashH

#include "Common\Include\types.h"

#ifdef FLASH_C

#define	FLASH_CMD_WRITE_SR			(0x01)      // write to status register
#define	FLASH_CMD_PROGRAM			(0x02)		 // program flash
#define	FLASH_CMD_READ				(0x03)		 // read flash
#define	FLASH_CMD_READ_SR			(0x05)		 // read status register
#define	FLASH_CMD_WRITE_ENB			(0x06)		 // write enable
#define	FLASH_CMD_ERASE_4K			(0x20)      // erase 4K
#define	FLASH_CMD_READ_MANUF_DEV_ID (0x9F)		// read manuf. and device ID
#define	FLASH_CMD_CHIP_ERASE		(0xC7)		// erase entire Atmel and Micron chips
#define	FLASH_CMD_ERASE_64K			(0xD8)		// erase 64K

#define	FLASH_RSR_WEL               (0x02)		// 1 when Write Enable Latch is
                                                // set
#define	FLASH_RSR_nRDY_BSY			(0x01)		// 1 when busy, 0 when ready

#define	FLASH_MFG_DEVID_MASK		(0x00ffffff)
												// Mask for manuf. and device ID
#define	FLASH_MFG_DEVID_AT25DF321	(0x00471f)	// Manuf. and device ID for Atmel 32Mb part
#define	FLASH_MFG_DEVID_AT25DF321A	(0x01471f)	// Manuf. and device ID for Atmel 32Mb part
#define	FLASH_MFG_DEVID_M25Q128		(0x18BA20)	// Manuf. and device ID for Micron 128Mb part

#define	FLASH_SIZE_AT25DF321		(4*1024*1024)
												// AT25DF321 is 4MB
#define	FLASH_SIZE_M25Q128			(16*1024*1024)
												// M25Q128 is 16MB

#define	FLASH_PAGE_PROGRAM_DELAY		(500)	// Delay when polling for flash
												// page program completion

#define	FLASH_CHIP_ERASE_TIME_AT25DF321 (56)	// Max time to erase entire chip
#define	FLASH_CHIP_ERASE_TIME_M25Q128	(250)	// Max time to erase entire chip

#define	FLASH_CHIP_ERASE_TIME_MAX	(MAX(FLASH_CHIP_ERASE_TIME_AT25DF321,FLASH_CHIP_ERASE_TIME_M25Q128))

ULong ulSizeFlash = 0;							// size of the flash
ULong ulEndFlash;								// end addr of the flash

const ULong ulStartFlash = 0;					// base address of the flash

#else
extern const ULong ulStartFlash;				// base address of the flash
extern const ULong ulSizeFlash;					// make them const. so that
extern const ULong ulEndFlash;					// they can only be changed
												// in flash.c
#endif

#define	SIZE_ERASE_64K_BLOCK		(65536)		// size of the erase block
#define	SIZE_WRITE_PAGE				(256)		// size of the flash write page

#define	FLASH_WRITE_BUF_OFFSET		(4)			// data offset in the flash write buffer

#define	FLASH_SPI_BUF_LEN			(FLASH_WRITE_BUF_OFFSET+SIZE_WRITE_PAGE)
										// buffer for spi flash writes:
										// 1 byte for serial flash command
										// 3 bytes for address
										// 256 bytes for data payload
										// flash.c: vWriteFlash() uses this info.
#if FLASH_SPI_BUF_LEN != 260
#error Flash code expects SPI_BUF_LEN to be a certain size/format.
#endif

Bool bFlashReady(UByte ubFlashStatus);

void vInitFlashBlock(void);

ULong ulReadMfgDevIdFlashBlock(void);

ULong ulReadStatusRegFlashBlock(void);

void vReadStatusRegFlash(UByte * pubCmdBuf, UByte * pReadBuf);

void vReadFlashBlock(ULong ulFlashAddr, UByte * pReadBuf, UWord uwBytesToRead);

void vUnProtectFlashBlock(void);

void vWriteFlashBlock(ULong ulFlashAddr, UByte * pWriteBuf, UWord uwBytesToWrite);

void vWriteFlash(ULong ulFlashAddr, UByte * pWriteBuf, UWord uwBytesToWrite);

void vErase64KFlashBlock(ULong ulFlashAddr);

void vChipEraseFlashBlock(void);

void vTestFlash(void);

#endif /* INCFlashH */



