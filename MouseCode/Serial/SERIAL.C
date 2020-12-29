/**************************************************************************
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        Serial.c
*
* Written By:  Harjit Singh
*
* Date:        04/10/2010
*
* Purpose:     This file contains serial input/output routines for the USART and SPI
*			  serial interfaces
*
* Notes:
*
* To Be Done:
*
* Modification History:
*-Date--------Author-----Description--------------------------------------
* 09/05/11		Harjit		Adapting code from ZV to ZVI.
*
*							Deleted I2C code since DMA for I2C unit overlaps with
*							DMA for UART and I want to keep the UART DMA and
*							there are not I2C peripherals on ZVI.
*
*							Moved UART from UART3 to UART2.
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
#include <stddef.h>
#include <stdio.h>

#define SERIAL_C
#include "serial\serial.h"

#include "Common\display\display.h"

#include "simcode\simcode.h"
#include "Common\Include\ZthInclude.h"

#include "Common\loop\log.h"

// only needed when using log.c
#include "Common\loop\servoPrf.h"

typedef	struct sSPICmdBuf
{
	UByte ubSPICmd;
	UByte ubSPIData;
	UWord uwSPIDelay;
}__attribute__((packed)) sSPICmdBuf_t;

// SPI constants and variables
#define	SPI_CMD_BUF_LEN		(0x40)
#define	SPI_CMD_BUF_MASK	(0x3f)

// make sure SPI_CMD_BUF_LEN is a power of two
#if (SPI_CMD_BUF_LEN & (SPI_CMD_BUF_LEN - 1))
#error SPI_CMD_BUF_LEN is not a power of two
#endif
// Allocate the SPI buffer
static sSPICmdBuf_t SPICmdBuf[SPI_CMD_BUF_LEN];
static volatile ULong ulSPIHead, ulSPITail;
static volatile Bool bSPIEmbedOnlyInQueue;

static volatile SPI_STATE ssSPIState;

static volatile Bool bSPILockAvailable = FALSE;

// Shadow variables for running the buffered SPI transfer
// Using 'S' in the name to indicate shadow
static UByte* pSWriteBuf;
static UWord  uwSBytesToWrite;
static UByte* pSReadBuf;
static UWord  uwSBytesToRead;

// USART variables
#define	TX_BUF_LEN	(0x1000)
#define	TX_BUF_MASK	(TX_BUF_LEN-1)

// make sure TX_BUF_LEN is a power of two
#if (TX_BUF_LEN & (TX_BUF_LEN - 1))
#error TX_BUF_LEN is not a power of two
#endif


#define	RX_BUF_LEN	(0x80)
#define	RX_BUF_MASK	(0x7f)

// Allocate the UART transmit and receive buffers
static UByte ubTxOut[TX_BUF_LEN];
static UByte ubRxIn[RX_BUF_LEN];

static volatile ULong ulTxHead, ulTxTail, ulRxTail, ulRxPeekTail;

static inline void vEnableTimerSPI(void);

static inline void vEnableTimerSPI(void)
{
	// enable the timer
	TIM6->CR1 |= TIM_CR1_CEN;

	return;
}

static inline void vSetDelayTimerSPI(UWord uwDelay)
{
	// update the timer delay
	TIM6->ARR = uwDelay;

	return;
}

void vPrintHeadTailSPI(void)
{
	SPI_SERIAL_PRINTF("H:%2x T:%2x\n\r", ulSPIHead, ulSPITail);

	return;
}

static inline void vWriteSPI (UByte* pWriteBuf, UWord uwBytesToWrite)
{
	static UByte ubDummyRx;

	// Setup the transmit DMA channel

	// the DMA destination is setup one time in vSetupSPI since it is always
	// the SPI data register
	DMA1_Channel5->CMAR = (ULong) pWriteBuf;			// the DMA source
	DMA1_Channel5->CNDTR = uwBytesToWrite;				// DMA transfer length

	// Not memory to memory transfer, channel is low priority, size is 8 bits
	// Increment the memory address, do not increment the peripheral address
	// We do not want circular mode, read from memory and write to peripheral
	// Do not enable any interrupts and do not enable the channel right now.
	DMA1_Channel5->CCR = DMA_CCR5_MINC | DMA_CCR5_DIR;

	// Setup the receive DMA channel - this is being used just for the purposes
	// of generating the end of message interrupt

	// the DMA source is setup one time in vSetupSPI since it is always
	// the SPI data register
	DMA1_Channel4->CMAR = (ULong) &ubDummyRx;			// the DMA destination
	DMA1_Channel4->CNDTR = uwBytesToWrite;				// DMA transfer length

	// Not memory to memory transfer, channel is low priority, size is 8 bits
	// Do not increment the memory address, do not increment the peripheral
	// address. We do not want circular mode, read from peripheral and write to
	// memory. Do not enable any interrupts and do not enable the channel right
	// now.
	DMA1_Channel4->CCR = 0x0000;

	// Enable the SPI receive and transmit DMA channels. The transmit must be
	// enabled after the receive because when the transmit starts, we start to
	// recieve data at the same time and so if an interrupt happens between
	// the transmit and the receive, the receive won't happen.
	// For the receive DMA, enable the transfer complete interrupt which is used
	// to sequence the SPI state machine.
	DMA1_Channel4->CCR |= DMA_CCR4_EN | DMA_CCR4_TCIE;
	DMA1_Channel5->CCR |= DMA_CCR5_EN;

	return;
}

static inline void vReadSPI (UByte* pReadBuf, UWord uwBytesToRead)
{
	static UByte ubDummyTx = 0;

	// Setup the transmit DMA channel

	// the DMA destination is setup one time in vSetupSPI since it is always
	// the SPI data register
	DMA1_Channel5->CMAR = (ULong) &ubDummyTx; 			// the DMA source
	DMA1_Channel5->CNDTR = uwBytesToRead;				// DMA transfer length

	// not memory to memory transfer, channel is low priority, size is 8 bits,
	// do not increment the memory address, do not increment the peripheral
	// address. we do not want circular mode, read from memory and write to
	// peripheral. wo not enable any interrupts and do not enable the channel
	// right now.
	DMA1_Channel5->CCR = DMA_CCR5_DIR;

	// setup the receive DMA channel - this is being used just for the purposes
	// of generating the end of message interrupt

	// the DMA source is setup one time in vSetupSPI since it is always
	// the SPI data register
	DMA1_Channel4->CMAR = (ULong) pReadBuf;			// the DMA destination
	DMA1_Channel4->CNDTR = uwBytesToRead;				// DMA transfer length

	// not memory to memory transfer, channel is low priority, size is 8 bits,
	// do increment the memory address, do not increment the peripheral
	// address. we do not want circular mode, read from peripheral and write to
	// memory. do not enable any interrupts and do not enable the channel right now
	DMA1_Channel4->CCR = DMA_CCR4_MINC;

	ssSPIState = SPI_READ;

	// Enable the SPI transmit and receive DMA channels.
	// For the receive DMA, enable the transfer complete interrupt which is used
	// to sequence the SPI state machine.
	DMA1_Channel4->CCR |= DMA_CCR4_EN | DMA_CCR4_TCIE;
	DMA1_Channel5->CCR |= DMA_CCR5_EN;

	return;
}

// interrupt handler for SPI RX DMA channel.
// this routine is used to finish both the write phase and any read phase.
void DMA1_Channel4_IRQHandler (void)
{
	SPI_SERIAL_PRINTF("D");

	// disable the DMA channels - regardless of whether we just did a write or
	// a read. this way, if we only need to do a write (with no reads), then we will
	// still end up with DMA disabled.
	DMA1_Channel4->CCR &= ~(DMA_CCR4_EN | DMA_CCR4_TCIE);
	DMA1_Channel5->CCR &= ~DMA_CCR5_EN;

	// clear the DMA interrupt transfer completed flag
	DMA1->IFCR = DMA_IFCR_CTCIF4;

	// if we just finished writing, and we need to read something
	// the embedded write and write onlys are (obviously) covered by the else path
	if ((SPI_WRITE == ssSPIState) && (0 != uwSBytesToRead))
	{
		SPI_SERIAL_PRINTF("R");
		// setup the DMA and transfer complete interrupt for reading info.
		// from the device
		vReadSPI(pSReadBuf, uwSBytesToRead);
	}
	else
	{
		SPI_SERIAL_PRINTF("N\n\r");
		// deassert the chip selects - it is faster to deselect both than do a
		// compare and branch
		// drive the Flash chip and display chip selects high by setting the port bit
		GPIOB->BSRR = PORTB_nFLASHCS;
		GPIOA->BSRR = PORTA_nDISPCS;

		SPI2->CR1	= SPI_DEFAULT_CR1;		// Disable the SPI port

		// since we just finished a transfer check to see if it was an embedded one
		// if it wasn't then, we must have finished a non-embedded transfer, so
		// indicate that we only have embedded items in the queue
		if (SPI_WRITE_EMBED != ssSPIState)
		{
			bSPIEmbedOnlyInQueue = TRUE;
		}

		ssSPIState = SPI_IDLE;

		// The timer runs in one shot mode, so enable the timer
		// so that  logging and SPI routines run again
		vEnableTimerSPI();
	}

	return;
}

void TIM6_IRQHandler(void)
{
	SPI_SERIAL_PRINTF("T");

	// clear the timer interrupt flag
	// the timer runs in one shot mode. vDoSPI or DMA1_Channel4_IRQ
	// will resetup the timer
	TIM6->SR = ~TIM_SR_UIF;

	// check and run data logging transfers
	vCheckNWriteLogToFlash();

	// check and run SPI transfers
	// vDoSPI and DMA1_Channel4_IRQ resetup timer 6
	vDoSPI();

	return;
}

void vSetupSPI (void)
{
	// setup the SPI transaction delay timer
	// it is used as a one shot to create delays between transactions so that
	// all time isn't used up servicing SPI transactions
	// enable the clock to Timer 6
	RCC->APB1ENR |= RCC_APB1ENR_TIM6EN;

	// disable the counter
	// disable the ARR double buffer
	// count up
	// use one pulse mode
	// only a counter overflow/underflow generates an update interrupt
	TIM6->CR1 = TIM_CR1_OPM | TIM_CR1_URS;

	// set TRGO to reset mode since it isn't needed
	TIM6->CR2 = 0x0000;

	// clear any previous interrupt flags
	TIM6->SR = 0x0000;

	TIM6->CNT = 0x0000;

	// use the timer's prescaler to create a 1MHz clock for the timer
	TIM6->PSC = (TIMER6_CLK_FREQ / SPI_DELAY_TIMEBASE) -1;

	TIM6->ARR = SPIDLY_NORMAL;				// Set freq to normal rate

	TIM6->DIER = TIM_DIER_UIE;				// Enable the interrupt from
											// the timer moude

	NVIC_SetPriority(TIM6_IRQn, SPI_TIMER_INT_PRIORITY);

	// enable Timer 6 interrupt at the interrupt controller
	NVIC_EnableIRQ(TIM6_IRQn);

	// setup the queue indicies
	ulSPIHead = ulSPITail = 0;
	bSPIEmbedOnlyInQueue = TRUE;			// indicate we only have embedded
											// transactions in the queue

	// Enable the clock to the SPI2, SPI2_RX DMA and SPI2_TX DMA blocks
	RCC->APB1ENR	|= RCC_APB1ENR_SPI2EN;
	RCC->AHBENR		|= RCC_AHBENR_DMA1EN;

	SPI2->CR1		= SPI_DEFAULT_CR1;		// Disable the SPI port for now
	SPI2->CR2 		= SPI_CR2_TXDMAEN | SPI_CR2_RXDMAEN;
											// Enable SPI transmit and receive
											// DMA requests

	// reset the DMA channels
	DMA1_Channel4->CCR = 0x00000000;
	DMA1_Channel5->CCR = 0x00000000;

	// setup the transmit DMA channel's fixed parameters
	DMA1_Channel5->CPAR = (ULong) &(SPI2->DR);		// the DMA destination

	// setup the receive DMA channel's fixed parameters
	DMA1_Channel4->CPAR = (ULong) &(SPI2->DR);		// the DMA source

	// Setup the DMA interrupt level
	NVIC_SetPriority(DMA1_Channel4_IRQn, SPI_DMA_INT_PRIORITY);

	// At the interrupt controller, clear any pending DMA1 Ch4 interrupt
	NVIC_ClearPendingIRQ(DMA1_Channel4_IRQn);

	// Enable DMA1 Ch 4 interrupt at the interrupt controller
	NVIC_EnableIRQ(DMA1_Channel4_IRQn);

	// init SPI to idle state
	ssSPIState = SPI_IDLE;

	// make the SPI bus available
	bSPILockAvailable = TRUE;

	// enable the SPI timer/state machine
	vEnableTimerSPI();

	return;
}


static void vPutCmdSPI(sSPICmdBuf_t * pscbSPICmd)
{
	SAVE_N_DIS_ALL_INT;

	SPICmdBuf[ulSPIHead] = *pscbSPICmd;

	ulSPIHead++;
	ulSPIHead &= SPI_CMD_BUF_MASK;

	RESTORE_INT;

	return;
}

/**************************************************************************
* Routine:     	vWriteOneByteSPI
*
* Description: 	Writes the SPI command into the SPI queue.
*				If one byte is being written, then the byte to be written is included in
*				the data field of the command.
*
* Notes:
*
* To Be Done:
*
*/
void vWriteOneByteSPI(SPI_DEVICE sdSPIDev, UByte ubData, UWord uwDelay)
{
	sSPICmdBuf_t scbSPICmd;
	// copy the SPI target device for use by the SPI interrupt code
	// set the data source to embedded
	scbSPICmd.ubSPICmd = (sdSPIDev == SPI_FLASH ? SPICMD_DEST_FLASH : SPICMD_DEST_DISP) |
							SPICMD_DATA_SRC_EMBEDDED;

	// copy the data into the embedded data area of the structure
	scbSPICmd.ubSPIData = ubData;

	// copy the delay
	scbSPICmd.uwSPIDelay = uwDelay;

	vPutCmdSPI(&scbSPICmd);

	return;
}

/**************************************************************************
* Routine: 		vTransactSPI
*
* To Be Done:
*
*-Date--------Author-----Description--------------------------------------
* 09/09/12		Harjit		Documented the routine
*/
void vTransactSPI(SPI_DEVICE sdSPIDev, UByte * pWriteBuf, UWord uwBytesToWrite, UByte * pReadBuf, UWord uwBytesToRead, UWord uwDelay)
{
	sSPICmdBuf_t scbSPICmd;

	// copy write parameters into local shadow variables for the SPI dispatcher to
	// use to do the write phase
	pSWriteBuf = pWriteBuf;
	uwSBytesToWrite = uwBytesToWrite;

	// copy read parameters into local shadow variables for SPI write interrupt to
	// use to setup the read phase
	pSReadBuf = pReadBuf;
	uwSBytesToRead = uwBytesToRead;

	// copy the SPI target device for use by the SPI interrupt code
	// set the data source to the shadow registers
	scbSPICmd.ubSPICmd = (sdSPIDev == SPI_FLASH ? SPICMD_DEST_FLASH : SPICMD_DEST_DISP) |
							SPICMD_DATA_SRC_SHADOW_REG;

	// copy the delay
	scbSPICmd.uwSPIDelay = uwDelay;

	// we will write a multibyte request to the SPI queue, so block any more
	// requests until we run this one
	bSPIEmbedOnlyInQueue = FALSE;

	vPutCmdSPI(&scbSPICmd);

	return;
}

Bool bDoneSPI(void)
{
	return ((ulSPITail == ulSPIHead) && (SPI_IDLE == ssSPIState));
}

void vDoSPI(void)
{
	SPI_SERIAL_PRINTF("S");

	// if the SPI bus is idle, then check to see if there are more SPI transfers that
	// need to be done. otherwise wait to let the SPI transfer in progress complete
	if (SPI_IDLE == ssSPIState)
	{
		if (ulSPITail != ulSPIHead)
		{
			// get a request from the SPI command queue
			sSPICmdBuf_t scbSPICmd = SPICmdBuf[ulSPITail];

			ulSPITail++;
			ulSPITail &= SPI_CMD_BUF_MASK;

			// figure out if we need to get the data from the SPI command
			// structure or the shadow variables
			if (SPICMD_DATA_SRC_EMBEDDED == (scbSPICmd.ubSPICmd & \
												SPICMD_DATA_SRC_MASK))
			{
				// set state to embedded write so that SPI DMA interrupt routine
				// won't try to do a read phase
				ssSPIState = SPI_WRITE_EMBED;

				// setup the DMA and transfer complete interrupt for writing info. to the device
				vWriteSPI(&scbSPICmd.ubSPIData, 1);
			}
			else
			{
				// set state to write so that SPI DMA interrupt routine can check and
				// run a read phase if needed
				ssSPIState = SPI_WRITE;

				// setup the DMA and transfer complete interrupt for writing info. to the device
				vWriteSPI(pSWriteBuf, uwSBytesToWrite);
			}

			// based on the device, assert its chip select
			if (SPICMD_DEST_FLASH == (scbSPICmd.ubSPICmd & SPICMD_DEST_MASK))
			{
				// drive the Flash chip select low by resetting the port bit
				GPIOB->BSRR = PORTB_nFLASHCS << 16;

				// configure and enable the SPI interface
				SPI2->CR1 = SPI_FLASH_CR1;
			}
			else
			{
				// drive the display chip select low by resetting the port bit
				GPIOA->BSRR = PORTA_nDISPCS << 16;

				// configure and enable the SPI interface
				SPI2->CR1 = SPI_DISP_CR1;
			}

			// setup the timer for the delay between SPI commands
			// We don't have to disable the timer because it is a one
			// shot timer. The DMA1_Channel4_IRQ, on transfer
			// completion will setup the timer
			vSetDelayTimerSPI(scbSPICmd.uwSPIDelay);
		}
		else
		{
			// since we don't have a command to process, check again
			// in a bit
			vSetDelayTimerSPI(SPIDLY_NORMAL);
			vEnableTimerSPI();
		}
	}
	else
	{
		//
		// Theoretically, we should never be able to reach
		// the next statement. However, since SPI is not functioning properly,
		// we cannot log it or really display it... Give it a try though.
		//

        // turn on the red LED since that should work
        RED_LED_ON;

		vDispChar(0, 'I');
	}

	return;
}

/**************************************************************************
* Routine: 		bGetNonEmbeddedLockSPI
*
* Purpose: 		Used to find out if the SPI bus is in use or not. If it isn't in use, then claim
* Purpose: 		Stub routine to make development easy between ZV and ZVI
*
* Arguments:	None
*
* RETURNS: 	void
*
* Notes:
*
* To Be Done:
*
*-Date--------Author-----Description--------------------------------------
* 09/01/12		Harjit		Created
*/
Bool bGetNonEmbeddedLockSPI(void)
{
	Bool bGotLock = FALSE;

	SAVE_N_DIS_ALL_INT;

	if (bSPILockAvailable)
	{
		bSPILockAvailable = FALSE;

		bGotLock = TRUE;
	}

	RESTORE_INT;

	return (bGotLock);
}

void vReturnNonEmbeddedLockSPI(void)
{
	bSPILockAvailable = TRUE;

	return;
}

// this function tells us if there is an embedded SPI transaction in the queue
// (SPICmdBuf) already or not. if there is, then no more SPI non-embedded transactions
// should be added to the queue
inline Bool bEmbedOnlyInQueueSPI(void)
{
	return (bSPIEmbedOnlyInQueue);
}

void vSetupI2C(void)
{
	return;
}


/**************************************************************************
* Routine:     vSetupUART
*
* Description: Setup the serial port and its queues
*
* Notes:
*
* To Be Done:
*
*/
void vSetupUART(void)
{
	// setup the queue indicies
	ulTxHead = ulTxTail = ulRxTail = ulRxPeekTail = 0;

	// Enable the clock to USART2, USART_TX DMA and USART_RX DMA blocks
	RCC->APB1ENR	|= RCC_APB1ENR_USART2EN;
	RCC->AHBENR		|= RCC_AHBENR_DMA1EN;

	// switch nRTS (PA1), TXD (PA2) to alt. function so that we don't have any
	// glitches on these lines. For both, set the drive level to 10MHz.
	//
	// nCTS and RXD are already inputs with pull down/pull up respectively
	GPIOA->CRL		|= (GPIOA->CRL & \
						~(GPIO_CRL_CNF2 |	GPIO_CRL_MODE2 | \
						GPIO_CRL_CNF1 	| 	GPIO_CRL_MODE1))| \
						GPIO_CRL_MODE2_0 * (GPIO_MODE_OUT_10MHz | GPIO_CNF_ALTPP) |\
						GPIO_CRL_MODE1_0 * (GPIO_MODE_OUT_10MHz | GPIO_CNF_ALTPP);

	// Enable the USART, 8 bits, wake doesn't matter, no parity, disable parity
	// interrupt, disable transmit interrupt, disable transmit complete interrupt,
	// disable receive interrupt, disable idle interrupt, go ahead and enable the
	// transmitter and receiver
	USART2->CR1		= USART_CR1_UE | USART_CR1_TE | USART_CR1_RE;

	// Don't use LIN mode, we want one stop bit, leave CK pin disabled since
	// we want UART mode, not USART mode, clock polarity and phase,
	// last bit clock pulse, LIN stuff, ADD are all don't cares,
	USART2->CR2		= 0x00000000;

	// Don't enable CTS interrrupt, do enable CTS and RTS, DMA for transmit and
	// for receive, disable smartcard mode, we want full duplex, no IrDA stuff,
	// disable error interrupt
	USART2->CR3		= USART_CR3_CTSE | USART_CR3_RTSE | \
						USART_CR3_DMAT | USART_CR3_DMAR;

	// Set the baud rate to 115.2K or 460.8K or 921.6K
	USART2->BRR		= SERIAL_BAUD_921K;

	// Setup the transmit DMA channel's fixed parameters
	DMA1_Channel7->CPAR = (ULong) &(USART2->DR);		// the DMA destination

	DMA1_Channel7->CCR = UART_DMA_CCR_SETUP;

	// Setup the receive DMA channel's fixed parameters
	DMA1_Channel6->CPAR = (ULong) &(USART2->DR);	// the DMA source
	DMA1_Channel6->CMAR = (ULong) &ubRxIn[0];		// the DMA destination
	DMA1_Channel6->CNDTR = RX_BUF_LEN;				// the receive buffer size

	// Not memory to memory, channel is low priority, size is 8 bits,
	// increment the memory address, do not increment the peripheral address,
	// We want circular mode, read from peripheral and write to memory,
	// Do not enable any interrupts and enable the channel.
	DMA1_Channel6->CCR = DMA_CCR6_MINC | DMA_CCR6_CIRC | DMA_CCR6_EN;

	return;
}

/**************************************************************************
* Routine:     ulGetLeftUARTOut
*
* Description: Returns the number of empty bytes in the serial output queue
*
* Notes:
*
* To Be Done:
*
*/
ULong ulGetLeftUARTOut(void)
{
	return (TX_BUF_LEN - ((ulTxHead - ulTxTail) & TX_BUF_MASK));
}

/**************************************************************************
* Routine:     ulCheckUARTIn
*
* Description: Returns the number of bytes in the serial input queue
*
* Notes:
*
* To Be Done:
*
*/
ULong ulCheckUARTIn(void)
{
	return ((RX_BUF_LEN - DMA1_Channel6->CNDTR - ulRxTail) & RX_BUF_MASK);
}

/**************************************************************************
* Routine:		ubGetUART
*
* Description:	Returns with a byte from the serial input queue.
*
* Notes:		If the queue is empty, it will not return till a character
*				comes in.
*
* To Be Done:
*
*/
UByte ubGetUART(void)
{
	UByte ubDataIn;

	while (0 == ulCheckUARTIn())
		;

	ubDataIn = ubRxIn[ulRxTail];

	ulRxTail++;
	ulRxTail &= RX_BUF_MASK;

	return(ubDataIn);
}


/**************************************************************************
* Routine:		ubSyncPeekUART
*
* Description:	Synchronizes the peek UART to the first entry in the UART RX buffer
*
* Notes:
*
* To Be Done:
*
*/
void vSyncPeekUART(void)
{
	ulRxPeekTail = ulRxTail;

	return;
}

/**************************************************************************
* Routine:		ubPeekUART
*
* Description:	Returns with a byte from the serial input queue but doesn't take it out
*				of the queue. Use ubGetUART for that
*
* Notes:		If the queue is empty, it will not return till a character
*				comes in.
*
* To Be Done:
*
*/
UByte ubPeekUARTBlock(void)
{
	UByte ubDataIn;

	while (0 == ulCheckUARTIn())
		;

	ubDataIn = ubRxIn[ulRxPeekTail];

	ulRxPeekTail++;
	ulRxPeekTail &= RX_BUF_MASK;

	return(ubDataIn);
}
/**************************************************************************
* Routine:		vPutUART
*
* Description:	Outputs the passed character over the serial port
*
* Notes:
*
* To Be Done:
*
*/
void vPutUART(UByte ubCharOut)
{
	SAVE_N_DIS_ALL_INT;

	ubTxOut[ulTxHead] = ubCharOut;
    ulTxHead = (ulTxHead + 1) & TX_BUF_MASK;

    RESTORE_INT;

	return;
}

void vPutStringUART(char *pString)
{
	while (NULL != *pString)
	{
		vPutUART(*pString);
		pString++;
	}

	return;
}

void vDoUART(void)
{
	ULong ulCount = ulTxHead - ulTxTail;

	// if previous DMA has completed, and the Tx buffer is not empty,
	// then setup another one, otherwise wait for the next time tick
	if ((0 != ulCount) && (0 == DMA1_Channel7->CNDTR))
	{
		// disable the DMA channel so that we can program it
		DMA1_Channel7->CCR 	= UART_DMA_CCR_DISABLE;

		// setup the DMA source and length and enable the transfer
		// need to do this before we change tail below
		DMA1_Channel7->CMAR = (ULong) &ubTxOut[ulTxTail];

		// when the head wraps around, break the DMA into two pieces
		// one from tail to the end of the buffer and then another from
		// the start of the buffer to the head
		if (ulTxTail > ulTxHead)
		{
			ulCount = TX_BUF_LEN - ulTxTail;
			ulTxTail = 0;
		}
		else
		{
			ulTxTail = ulTxHead;
		}

		// setup the rest of the DMA registers and enable DMA
		DMA1_Channel7->CNDTR = ulCount;
		DMA1_Channel7->CCR = UART_DMA_CCR_ENABLE;
	}

	return;
}

void vTestUART(void)
{
	UByte ucChar;

	// echo data on the serial port
	while (1 != 0)
	{
		ucChar = ubGetUART();
		vPutUART(ucChar);
		vDispChar(0, ucChar);
	}

	// this routine never returns!
	return;
}

