/**************************************************************************
* $Id$
* $Source$
*
*			 NOTICE OF COPYRIGHT
*			  Copyright (C) 2020
*			     Team Zeetah
*			 ALL RIGHTS RESERVED
*
* File:        simcode.h
*
* Written By:  Harjit Singh
*
* Date:        09/3/11
*
* Purpose:     This file contains the SIM code definitions and is being adapted from ZV to ZVI.
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

#ifndef INCsimcodeH
#define INCsimcodeH

#include <stm32f10x.h>

// The following defines are used to assist with configuring GPIOs
#define GPIO_MODE_IN			(0) 	// Input mode
#define GPIO_MODE_OUT_10MHz		(1) 	// Output mode, max 10MHz
#define GPIO_MODE_OUT_2MHz 		(2) 	// Output mode, max 2MHz
#define GPIO_MODE_OUT_50MHz		(3) 	// Output mode, max 50MHz

#define GPIO_CNF_AIN			(0<<2) 	// Analog input
#define GPIO_CNF_FIN			(1<<2) 	// Floating input
#define GPIO_CNF_PULL			(2<<2) 	// Input with pull up/down
#define GPIO_CNF_RESV			(3<<2) 	// Reserved

#define GPIO_CNF_OUTPP 			(0<<2) 	// Output push/pull
#define GPIO_CNF_OUTOD 			(1<<2) 	// Output open drain
#define GPIO_CNF_ALTPP			(2<<2) 	// Alt output push/pull
#define GPIO_CNF_ALTOD			(3<<2) 	// Alt output open drain

// STM32 part specific internal oscillator adjustment using HSITRIM
// if we replace the 16 with 13, it makes the oscillator freq. the closest to 8MHz
// however that has a higher residual error for the baud rate calc. than when
// we set it to 16. At a setting of 16, we get 8.07MHz which has almost no
// baud rate error at 921K. So, we are going to use 16.
//
#define	ZVI_RCC_CR_HSITRIM	(16)

// Define Port A usage:
#define	PORTA_UNUSED_PA12	(1<<12)		// Unused port
#define	PORTA_nDISPCS		(1<<11)		// Display chip select - active low
#define	PORTA_nPANIC_BTN	(1<<10)		// Panic button status
										// 0 = Panic button pressed
										// 1 = Button not pressed
#define	PORTA_ENCA_LFT		(1<<9)		// Left wheel encoder A input
#define	PORTA_ENCB_LFT		(1<<8)		// Left wheel encoder B input
#define	PORTA_GYRO			(1<<7)		// ADC Gyro input
#define	PORTA_VBAT_DIV_2P5	(1<<6)		// ADC Vbat / 2.5 input
#define	PORTA_EMT_FRNT_DRV	(1<<5)		// DAC used for front emitter drive level
#define	PORTA_EMT_DIAG_DRV	(1<<4)		// DAC used for diag. emitter drive level
#define	PORTA_USART2_RXD	(1<<3)		// USART 2' RXD signal
#define	PORTA_USART2_TXD	(1<<2)		// USART 2' TXD signal
#define	PORTA_USART2_RTS	(1<<1)		// USART 2' nRTS signal
#define	PORTA_USART2_CTS	(1<<0)		// USART 2' nCTS signal

// Define Port B usage:
#define	PORTB_MOSI			(1<<15)		// Serial flash chip Master Out Slave In
#define	PORTB_MISO			(1<<14)		// Serial flash chip Master In Slave Out
#define	PORTB_SCK			(1<<13)		// Serial flash chip clock
#define	PORTB_nFLASHCS		(1<<12)		// Serial flash chip select - active low
#define	PORTB_PWM_LFT_MTR	(1<<11)		// PWM for left motor
#define	PORTB_PWM_RT_MTR	(1<<10)		// PWM for right motor
#define	PORTB_I2C1_SDA		(1<<9)		// I2C data
#define	PORTB_I2C1_SCL		(1<<8)		// I2C clock
#define	PORTB_ENCA_RT		(1<<7)		// Right wheel encoder A input
#define	PORTB_ENCB_RT		(1<<6)		// Right wheel encoder B input
#define	PORTB_nGREEN_LED	(1<<5)		// Controls green LED
										// Low = off
										// High = on
#define	PORTB_nTRST			(1<<4)		// JTAG: nTRST
#define	PORTB_JTDO			(1<<3)		// JTAG: JTDO
#define	PORTB_nRED_LED		(1<<2)		// Controls red LED
										// Low = off
										// High = on
#define	PORTB_SENS_LFT_FRNT_A1	(1<<1)	// ADC: Sensor LFT_FRNT_A1
#define	PORTB_SENS_LFT_FRNT_A2	(1<<0)	// ADC: Sensor LFT_FRNT_A2

// TODO: update what low and high means for the motors below.
// Define Port C usage:
#define	PORTC_UNUSED_PC15	(1<<15)		// Unused port
#define	PORTC_UNUSED_PC14	(1<<14)		// Unused port
#define	PORTC_UNUSED_PC13	(1<<13)		// Unused port
#define	PORTC_TP_PC12		(1<<12)		// Unused port but has a test point
#define PORTC_DRC_RT_MTR	(1<<11)		// Right motor's direction line
										// Low =
										// High =
#define PORTC_DRC_LFT_MTR	(1<<10)		// Left motor's direction line
										// Low =
										// High =

#define	PORTC_BZR			(1<<9)		// Buzzer drive
#define	PORTC_TP_EMT_DIS	(1<<8)		// TP37: Emitter disable edge
#define	PORTC_TP_EMT_EN		(1<<7)		// TP36: Emitter enable edge
#define	PORTC_TP_ADC_SAMPLE	(1<<6)		// TP39: ADC sample edge
#define	PORTC_SENS_LFT_DIAG_A2	(1<<5)	// ADC: Sensor LFT_DIAG_A2
#define	PORTC_SENS_LFT_DIAG_A1	(1<<4)	// ADC: Sensor LFT_DIAG_A1
#define	PORTC_SENS_RT_DIAG_A2	(1<<3)	// ADC: Sensor RT_DIAG_A2
#define	PORTC_SENS_RT_DIAG_A1	(1<<2)	// ADC: Sensor RT_DIAG_A1
#define	PORTC_SENS_RT_FRNT_A1	(1<<1)	// ADC: Sensor RT_FRNT_A1
#define	PORTC_SENS_RT_FRNT_A2	(1<<0)	// ADC: Sensor RT_FRNT_A2

// Define Port D usage:
#define	PORTD_TP_PD2		(1<<2)		// Unused port but has a test point
#define	PORTD_UNUSED_PD1	(1<<1)		// Unused port
#define	PORTD_UNUSED_PD0	(1<<0)		// Unused port

#define	GREEN_LED_OFF		GPIOB->BRR = PORTB_nGREEN_LED;
#define	GREEN_LED_ON		GPIOB->BSRR = PORTB_nGREEN_LED;
#define	GREEN_LED_TOGGLE	if(GPIOB->ODR & PORTB_nGREEN_LED){GREEN_LED_OFF;}else{GREEN_LED_ON;}

#define	RED_LED_OFF			GPIOB->BRR = PORTB_nRED_LED;
#define	RED_LED_ON			GPIOB->BSRR = PORTB_nRED_LED;
#define	RED_LED_TOGGLE		if(GPIOB->ODR & PORTB_nRED_LED){RED_LED_OFF;}else{RED_LED_ON;}

#define	PANIC_BUTTON_PRESSED (0 == (GPIOA->IDR & PORTA_nPANIC_BTN))

// SYS_CORE_FREQ touches the following blocks and files:
// SPI: 			Serial.c
// I2C: 		Serial.c
// UART: 		Serial.c
// Sensors: 	Sensor.h / Sensor.c
// Systick: 		Loop.c
// ADC:		Simcode.c / Qadccode.c
// SOC:		Simcode.c

#define	SYS_CORE_FREQ		((HSI_VALUE / 2) * 16)
												// System core freq
#define	AHB_CLK_FREQ		(SYS_CORE_FREQ)		// AHB System bus speed
#define	APB1_CLK_FREQ		(SYS_CORE_FREQ / 2)	// APB Peripheral bus 1 (low) speed
#define	APB2_CLK_FREQ		(SYS_CORE_FREQ)		// APB Peripheral bus 2 (high) speed

#define	CLK_CNT_PER_US		(SYS_CORE_FREQ / 1000000L)

/* DMA channel assignment for Zeetah VI
	DMA1 Channel	Description
		1			ADC 1 & 2 for sensors
		2			Not used
		3			Not used
		4			SPI2_RX
		5			SPI2_TX
		6			USART2_RX
		7			USART2_TX

	DMA2 Channel	Description
		1			TIM8_CH3 - Update DAC to disable emitters (see TPUCode.h)
		2			Not used
		3			TIM8_CH1 - update TIM8_CCR1 (see TPUCode.h)
		4			Not used
		5			TIM8_CH2 - Update DAC to enable emitters (see TPUCode.h)
*/

// function prototypes
void vSetupCPUCycleCntr(void);
ULong ulGetCycleCnt(void);

void vDelayUS(ULong);
void vDelayMS(ULong);

#endif /* INCsimcodeH */
