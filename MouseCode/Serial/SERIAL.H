/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        		Serial.h
*
* Written By:  	Harjit Singh
*
* Date:        	07/04/10
*
* Purpose:     	Header file for serial input/output routines
*
*
* To Be Done:
*
*-Date--------Author-----Description--------------------------------------
* 07/04/10		HS			Started adaptation for ZV
* 09/05/11		HS			Adapted for ZVI.
*							Removed I2C support.
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

#ifndef INCSerialH
#define INCSerialH

#include <stm32f10x.h>
#include "qadccode\qadccode.h"

#ifdef DEBUG
#include <stdio.h>
#include <cross_studio_io.h>
#endif

#ifdef SERIAL_C
// This section contains information related to the SPI interface
//
// Since the SPI port is connected to two devices, and they are different
// configurations the SPI configuration register settings.

// The default init value for SPI_CR1 is SPI port disabled. The most important
// thing to do is disable the SPI port, set software NSS control, set master mode,
// to make the clock format the same between the display and the serial flash
// use CPOL = 0, CPHA = 0;
#define	SPI_DEFAULT_CR1		(SPI_CR1_SSM | SPI_CR1_SSI | SPI_CR1_MSTR)

// For the flash, we want BIDIMODE, BIDIOE is a don't care, CRC disabled
// CRCNEXT is a don't care, eight bit data format, Full duplex mode,
// Set Software Slave Management bit and make the SSI bit one,
// MSB transmitted first, enable the SPI port, set the baud rate to
// /2 mode (16MHz), master mode, to make the clock format the same
// between the display and the serial flash use CPOL = 0, CPHA = 0
#define	SPI_FLASH_CR1		(SPI_CR1_SSM | SPI_CR1_SSI | SPI_CR1_SPE | SPI_CR1_MSTR)

// For the display, we want BIDIMODE, BIDIOE is a don't care, CRC disabled
// CRCNEXT is a don't care, eight bit data format, Full duplex mode,
// Set Software Slave Management bit and make the SSI bit one,
// LSB transmitted first, enable the SPI port, set the baud rate to
// /8 mode (36/8 = 4.5MHz), master mode, to make the clock format the same
// between the display and the serial flash use CPOL = 0, CPHA = 0
#define	SPI_DISP_CR1		(SPI_CR1_SSM | SPI_CR1_SSI | SPI_CR1_LSBFIRST | SPI_CR1_SPE | SPI_CR1_BR_1 | SPI_CR1_MSTR)

#define	SPICMD_DEST_MASK		(0x80)	// when this bit is clear, we are doing a display xfer
										// when this bit is set, we are doing a flash xfer
#define	SPICMD_DEST_FLASH		(0x80)	// destination is flash
#define	SPICMD_DEST_DISP		(0x00)	// destination is display

#define	SPICMD_DATA_SRC_MASK	(0x40)	// when this bit is clear, we are using the shadow
										// registers to run the SPI transfer
										// when this bit is set, we are doing a single
										// byte write to either the display or flash
										// and the byte is embedded in the scbSPIBuf
										// array
#define	SPICMD_DATA_SRC_EMBEDDED (0x40)
#define	SPICMD_DATA_SRC_SHADOW_REG	(0x00)

#define	TIMER6_CLK_FREQ			(APB1_CLK_FREQ * 2)
										// Timer 6's input clock freq.

#define	SPI_DELAY_TIMEBASE		(1000000L)
										// SPI delay can be programmed in us

// This section contains information related to the UART Serial interface

#define	SERIAL_BAUD_115K		(278)	// See BaudRate tab of ZVMouseDesign.xls
#define	SERIAL_BAUD_461K		(69)	// See BaudRate tab of ZVMouseDesign.xls
#define	SERIAL_BAUD_921K		(35)	// See BaudRate tab of ZVMouseDesign.xls

// Not memory to memory, channel is low priority, size is 8 bits,
// increment the memory address, do not increment the peripheral address,
// We do not want circular mode, read from memory and write to peripheral,
// Do not enable any interrupts and do not enable the channel right now.
#define	UART_DMA_CCR_SETUP		(DMA_CCR2_MINC | DMA_CCR2_DIR)

#define	UART_DMA_CCR_DISABLE	(UART_DMA_CCR_SETUP)
										// make sure the DMA_CCR2_EN bit is clear
										// but keep the rest of the DMA configuration

#define	UART_DMA_CCR_ENABLE		(UART_DMA_CCR_SETUP | DMA_CCR2_EN)
										// make sure the DMA_CCR2_EN bit is set
										// and keep the rest of the DMA configuration
#endif

#ifdef DEBUG_SERIAL
#define SERIAL_PRINTF(...) {snprintf(cString, CSTRING_LEN, __VA_ARGS__);vPutStringUART(cString);}
#else
#define SERIAL_PRINTF(...)
#endif

// This define is used to enable output of SPI debug info.
#ifdef ENABLE_SPI_DEBUG_OUTPUT
#define SPI_SERIAL_PRINTF(...) SERIAL_PRINTF(__VA_ARGS__)
#else
#define SPI_SERIAL_PRINTF(...)
#endif

typedef enum
{
	SPI_FLASH,
	SPI_DISP
} SPI_DEVICE;

typedef enum
{
	SPI_IDLE,
	SPI_WRITE_EMBED,
	SPI_WRITE,
	SPI_READ
} SPI_STATE;

#define	SPIDLY_MIN				(5)		// this is the minimum delay between commands
										// it shouldn't be set to zero becaues then the
										// timer won't start
#define	SPIDLY_NORMAL			(200)	// normal delay

void vPrintHeadTailSPI(void);

void vSetupSPI(void);
void vWriteOneByteSPI(SPI_DEVICE sdSPIDev, UByte ubData, UWord uwDelay);
void vTransactSPI(SPI_DEVICE sdSPIDev, UByte * pWriteBuf, UWord uwBytesToWrite, UByte * pReadBuf, UWord uwBytesToRead, UWord uwDelay);
Bool bEmbedOnlyInQueueSPI(void);
Bool bDoneSPI(void);
void vDoSPI(void);
Bool bGetNonEmbeddedLockSPI(void);
void vReturnNonEmbeddedLockSPI(void);

void vSetupI2C(void);

void vSetupUART(void);
ULong ulGetLeftUARTOut(void);
ULong ulCheckUARTIn(void);
UByte ubGetUART(void);
void vSyncPeekUART(void);
UByte ubPeekUARTBlock(void);
void vPutUART(UByte ubCharOut);
void vPutStringUART(char *);
void vDoUART(void);
void vTestUART(void);

#endif /* INCSerialH */

