/**************************************************************************
*
*            NOTICE OF COPYRIGHT
*             Copyright (C) 2020
*                Team Zeetah
*            ALL RIGHTS RESERVED
*
* File:             LogMsg.h
*
* Written By:   Harjit Singh
*
* Date:         12/03/2013
*
* Purpose:      This file contains the values used when logging to flash. This
*               file is used by the mouse code and the PC dash code.
*
* Notes:        Sharing files between C & C#:
*               http://stackoverflow.com/questions/954321/is-it-possible-to-share-an-enum-declaration-between-c-sharp-and-unmanaged-c
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
*  12/03/13     HarjitS     Created
*/
/*
 * Copyright 2020 Harjit Singh
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

#if __LINE__

#define public

#else

namespace MyConstNamespace
{

#endif // __LINE__

    public enum LMVersion
    {
        LMCurVersion                        = 4
    };

    public enum LMCmd
    {
        //
        // This command is used to send an arbitrary set of characters to the
        // PC for printing to the display.
        //
        // The payload a byte that is the length followed by the character data.
        // It is *NOT* NULL terminated.
        //
        LMCmdPrintString                    = 0,

        //
        // This command is used to communicate that an error or an "interesting"
        // event occurred.
        //
        // The payload is a byte that is used by the PC application to print
        // the corresponding error/event string.
        //
        LMCmdError,                         // 0x01

        //
        // This command logs that the mouse just marked the wall.
        //
        // The payload is a byte with the bit pattern of the walls that were
        // just marked.
        //
        LMCmdMarkedWalls,                   // 0x02

        //
        // This command logs the mouse's current cell location.
        //
        // The payload is a byte where the upper nibble is the y coordinate
        // and the lower nibble is the x coordinate.
        //
        LMCmdCellLoc,                       // 0x03

        //
        // This command logs the mouse's current orientation.
        //
        // The payload is in the enumerated type below.
        //
        LMCmdOrient,                        // 0x04

        //
        // This command logs that while learning, we found a front wall and
        // need to abort the current path.
        //
        // 03/08/15:    Replaced this with LMCmdAbortPathFrontWall and
        //              LMCmdAbortPathSideWall
        LMCmdAbortPath,                     // 0x05

        //
        // This command logs the UTurn front wall based correction error in the
        // payload.
        //
        // Payload for LMCmdUTurn is as follows:
        //
        // error = Left front sensor distance - Right front sensor distance
        // > 0 => turn right to correct
        // < = => turn left to correct
        // (mm)                             6.2
        //
        // If the error is larger than 6.2, it will saturate to 6.2
        //
        LMCmdUTurn,                         // 0x06

        //
        // This command contains the conversion factors between the mouse's
        // internal values and Human friendly units.
        //
        // The payload is as follows:
        // Version:                         8
        // mm per cell:                     8
        // counts per cell:                 16
        // counts per UTurn:                16
        // Sensor Dist When centered(mm):   8.8
        //
        LMCmdConvFactors,                   // 0x07

        //
        // This command logs the Profiler and Servo variables only
        //
        // The payload is as follows:
        // Forward Position Error:          6.2     mm
        // Desired Rotational Position:     9.7     deg
        //                                          Range: 0 to 359.9921875
        // Desired Forward Velocity:        0.8     m/s
        // Rotational Position Error:       0.8     deg
        // Desired Forward Velocity:        3       three integer bits of FVel
        // Rotational Position Error:       5       sign + four integer bits of
        //                                          Rotational Position error
        // Battery Voltage:                 3.5     Volts
        //
        LMCmdProfServoOnly,                 // 0x08

        //
        // This command logs the Profiler, Servo and sensor variables only
        //
        // The payload is as follows:
        // Forward Position Error:          6.2     mm
        // Desired Rotational Position:     9.7     deg
        //                                          Range: 0 to 359.9921875
        // Desired Forward Velocity:        0.8     m/s
        // Rotational Position Error:       0.8     deg
        // Desired Forward Velocity:        3       three integer bits of FVel
        // Rotational Position Error:       5       sign + four integer bits of
        //                                          Rotational Position error
        // Battery Voltage:                 3.5     Volts
        // mm                               8       mm
        // Left Front Power:                8       Encoded ADC counts
        // Left Diagonal Power:             8       Encoded ADC counts
        // Right Diagonal Power:            8       Encoded ADC counts
        // Right Front Power:               8       Encoded ADC counts
        // Left Front Distance:             8       mm
        // Left Diagonal Distance:          8       mm
        // Right Diagonal Distance:         8       mm
        // Right Front Distance:            8       mm
        // Distance fractions in above
        // order:                           0.2     LFD (LSB)
        //                                  0.2     LDP
        //                                  0.2     RDP
        //                                  0.2     RFP (MSB)
        //
        LMCmdProfServoSensors,              // 0x09

        // TODO: Update payload range
        //
        // This command logs the lateral corrector state variables
        //
        // The payload is as follows:
        // Error                            8.8     position/angle error in mm
        // Output                           6.10    rotational position in counts
        //
        LMCmdCorrector,                     // 0x0a

        //
        // This command logs the Run parameters
        //
        // The payload is as follows:
        // Forward Acceleration             5.3     m/s^2
        // Max Othogonal Forward Velocity   3.5     m/s
        // Max Diagonal Forward Velocity    3.5     m/s
        // Lateral Acceleration             5.3     m/s^2
        // Run number                       8
        // Run mode                         8       0 = learning,
        //                                          1 = start to center
        //                                          2 = center to start
        // Run Start Time                   32.0    milliseconds
        //
        LMCmdRunStart,                      // 0x0b

        //
        // This command is sent at the end of a run
        //
        // Run time                         32.0    milliseconds
        //
        LMCmdRunEndSuccess,                 // 0x0c
        LMCmdRunEndFail,                    // 0x0d

        //
        // This command logs the turn parameters
        //
        // The payload is as follows:
        // Turn type                        8       enum of turn type
        // Forward Velocity through turn    3.5     m/s
        // Lateral Acceleration             5.3     m/s^2
        // Turn time                        16.0    ms
        //
        LMCmdTurnParam,                     // 0x0e

        //
        // This command logs the path being run
        //
        // The payload is as follows:
        // Path length                      8       # of elements in path
        // Path coordinates                 8
        //
        LMCmdPath,                          // 0x0f

        //
        // This command logs the move the mouse is executing.
        //
        // The payload is in the enumerated type below.
        //
        LMCmdMove,                          // 0x10

        //
        // This command logs that the sensor drive level changed from
        // high to low.
        //
        LMCmdSensDriveLow,                  // 0x11

        //
        // This command logs that the sensor drive level changed from
        // high to low.
        //
        LMCmdSensDriveHigh,                 // 0x12

        //
        // This command is not used on the mouse side. If we have to manually
        // patch a file on the host, we fill unused locations with this
        // command.
        //
        LMCmdNOP,                           // 0x13

        //
        // This command logs that while learning, we found a front wall and
        // need to abort the current path.
        //
        // 03/08/15:    These commands replace LMCmdAbortPath
        LMCmdAbortPathFrontWall,            // 0x14
        LMCmdAbortPathSideWall,             // 0x15

        //
        // This command logs the lateral corrector state variables
        //
        // The payload is as follows:
        // Offset                           16.16     offset position in c^2/rad
        // Angle                            16.16     25:12.16 angle in c
        //
        LMCmdCorrector2,                    // 0x16

        //
        // This command logs the format for LMCmdLogData
        //
        // This is recorded at the start of the session.
        //
        // The payload a byte that is the raw data length followed by
        // the length of the string that describes the format followed by
        // information in printf format
        //
        // It is *NOT* NULL terminated.
        //
        LMCmdLogDataFormat,                 // 0x17
                                            // length of raw data
                                            // length of formatted data
                                            // b, w, l => Signed
                                            // B, W, L => Unsigned (upper case!)
                                            // f, F => float
                                            // d, D => double
        //
        // This command logs a block of data
        //
        // The payload is data.
        //
        // NOTE: The payload length is not sent because it was sent as
        //       part of LMCmdLogDataFormat so that we don't have to send
        //       it every time
        //
        LMCmdLogData,                       // 0x18

        //
        // This command is used to skip data in the log
        //
        // The parameter is the four byte address we skip to.
        //
        LMCmdNextAddr,                      // 0x19



        LMCmdLatCorrEnable                  = 0xc0,
        LMCmdLatCorrDisable,
        LMCmdLatCorrHold,
        LMCmdLatCorrStart,
        LMCmdLatCorrAlreadyEnabled,
        LMCmdLongCorrAlreadyEnabled,
        LMCmdLongLeftDiagSensorPeakIndexIsZero,     // <- no longer used but keep for old data files
        LMCmdLongRightDiagSensorPeakIndexIsZero,    // <- no longer used but keep for old data files
        LMCmdLongCorrDataReset,

        //
        // This command indicates that diagonal correction is in progress.
        //
        // To minimize the size of the command, the payload is combined into the
        // lower nibble. So, this command takes up sixteen entries.
        //
        LMCmdDiagCorrInitLeft               = 0xd0,
        LMCmdDiagCorrInitRight,             // 0xd1
        LMCmdDiagLatCorrHold,               // 0xd2
        LMCmdDiagLatCorr2Sample1,           // 0xd3
        LMCmdDiagLatCorr2Sample2Do,         // 0xd4
        LMCmdDiagLongCorrDo,                // 0xd5
        LMCmdDiagMoveUpdate,                // 0xd6
        LMCmdDiagLatCorr1Sample1,           // 0xd7
        LMCmdDiagLatCorr1Sample2Do,         // 0xd8
        LMCmdDiagLatCorrDoLeft,             // 0xd9
        LMCmdDiagLatCorrDoRight,            // 0xda
        LMCmdDiagLatCorrDoLeftOffset,       // 0xdb
        LMCmdDiagLatCorrDoRightOffset,      // 0xdc
        // Payload for LMCmdDiagLatCorrDoLeftOffset and LMCmdDiagLatCorrDoRightOffset
        // is the error in SByte 6.2 mm format.
        // error = slLeftDist - slRightDist:
        // > 0 => go left to correct
        // < = => go right to correct
        LMCmdDiagLongCorrCorrection,        // 0xdd
        // Payload for LMCmdDiagLongCorrCorrection is SWord that is the error.
        // > 0 => go further
        // < = => stop sooner
        LMCmdDiagCorrNotUsed,               // 0xde
        LMCmdDiagCorrSkip                   = 0xdf,
        //
        // This command indicates that longitudinal correction is in progress.
        //
        // To minimize the size of the command, the payload is combined into the
        // lower nibble. So, this command takes up sixteen entries.
        //
        LMCmdLongCorrEnable                 = 0xe0,
        LMCmdLongCorrSkip,                  // 0xe1
        LMCmdLongCorrDataGather,            // 0xe2
        LMCmdLongCorrLeftNoEdges,           // 0xe3
        LMCmdLongCorrLeftFallingEdge,       // 0xe4
        LMCmdLongCorrLeftRisingEdge,        // 0xe5
        LMCmdLongCorrLeftBothEdges,         // 0xe6
        LMCmdLongCorrRightNoEdges,          // 0xe7
        LMCmdLongCorrRightFallingEdge,      // 0xe8
        LMCmdLongCorrRightRisingEdge,       // 0xe9
        LMCmdLongCorrRightBothEdges,        // 0xea
        LMCmdLongCorrCorrection,            // 0xeb
        // Payload for LMCmdLongCorrCorrection is SWord that is the error.
        // > 0 => go further
        // < = => stop sooner
        LMCmdLongCorrAdd10mm,               // 0xec
        LMCmdLongCorrHold,                  // 0xed
        LMCmdLongCorrDisable,               // 0xee
        LMCmdLongCorrError,                 // 0xef

        //
        // This command indicates that lateral correction is in progress.
        //
        // To minimize the size of the command, the payload is combined into the
        // lower nibble. So, this command takes up sixteen entries.
        //
        LMCmdLatCorrInitReadWalls1          = 0xf0,
        LMCmdLatCorrDoReadWalls2,           // 0xf1
        LMCmdLatCorrDoReadWalls3NM_NoBT,    // 0xf2
        LMCmdLatCorrDoReadWalls3NM_WithBT,  // 0xf3
        // NoBT    = WithOut Back Track
        // WithBT  = With Back Track
        LMCmdLatCorrBothWalls,              // 0xf4
        LMCmdLatCorrLeftWallOnly,           // 0xf5
        LMCmdLatCorrRightWallOnly,          // 0xf6
        LMCmdLatCorrPegs,                   // 0xf7
        LMCmdLatCorrFrontWallDontUsePegs,   // 0xf8
        LMCmdLatCorrNoCorrDeadBand,         // 0xf9
        LMCmdLatCorrCorrection,             // 0xfa
        // Payload for LMCmdLatCorrCorrection is the error in SByte 5.2 mm format.
        // error = slRightDist - slLeftDist:
        // > 0 => go right to correct
        // < 0 => go left to correct
        LMCmdLatCorrDo,                     // 0xfb
        // 3/19/16: LatCorrDo is deprecated because we correct every mm
        LMCmdLatCorrDone,                   // 0xfc
        LMCmdLatCorrCorrection2,            // 0xfd
        // Payload for LMCmdLatCorrCorrection2 is the error in SByte 5.2 mm format.
        // error = slRightDist - slLeftDist:
        // > 0 => go right to correct
        // < 0 => go left to correct
        // Second byte with wall status:
        // 0 = no wall
        // 1 = left wall
        // 2 = right wall
        // 3 = left wall with no right wall and in the lateral correction zone
        // 4 = right wall with no left wall and in the lateral correction zone
        LMCmdLatCorrNotUsed14,
        LMCmdLatCorrError                   = 0xff,

    };

    // Payload for LMCmdError
    public enum LMPylError
    {
        LMPlyErrorFlashUnknownDeviceID,     // 0x00
        LMPlyErrorLogSizeZero,              // 0x01
        LMPlyErrorAnalyzePathDeltaPosZero,  // 0x02
        LMPlyErrorNoPathToGoal,             // 0x03
        LMPlyErrorOrientBadUpdateCoord,     // 0x04
        LMPlyErrorOrientBadInConvWalls,     // 0x05
        LMPlyErrorslCoordXYBad,             // 0x06
        LMPlyErrorLongCorrTooMuchData,      // 0x07
        LMPlyErrorUTurnAlignErrorTooHigh,   // 0x08
        LMPlyErrorTurnFinVelIsZero,         // 0x09 <- no longer used but keep for old data files
        LMPlyErrorUnknownMotionCommand,     // 0x0a
        LMPlyErrorLatCorrDontUnderstand1,   // 0x0b <- no longer used but keep for old data files
        LMPlyErrorLatCorrDontUnderstand2,   // 0x0c <- no longer used but keep for old data files
        LMPlyErrorMapMazeDontUnderstand1,   // 0x0d
        LMPlyErrorBatteryLow,               // 0x0e
        LMPlyErrorFwdErrorHigh,             // 0x0f
        LMPlyErrorRotErrorHigh,             // 0x10
        LMPlyErrorSetupStopInCenter1,       // 0x11
        LMPlyErrorSetupStopInCenter2,       // 0x12
        LMPlyErrorMouseUpsideDown,          // 0x13
        LMPlyErrorEnd                       // 0x14
    };

    // Payload for LMCmdMarkedWalls
    public enum LMPylMarked
    {
        LMPylMarkedNorthWall                = 0x08,
        LMPylMarkedEastWall                 = 0x04,
        LMPylMarkedSouthWall                = 0x02,
        LMPylMarkedWestWall                 = 0x01,
    };

    // Payload for LMCmdOrient
    public enum LMPylOrient
    {
        LMPylOrientNorth                    = 0x00,
        LMPylOrientNorthEast,
        LMPylOrientEast,
        LMPylOrientSouthEast,
        LMPylOrientSouth,
        LMPylOrientSouthWest,
        LMPylOrientWest,
        LMPylOrientNorthWest,
        LMPylOrientEnd,
    };


    // Payload for LMCmdRunStart
    public enum LMPylRunMode
    {
        LMPylRunModeLearn                   = 0x00,
        LMPylRunStartToCenter,
        LMPylRunCenterToStart
    };

    // Payload for LMCmdMove
    public enum LMPylMove
    {
        LMPylStop                          = 0x00,
        LMPylStopCenter                    = 0x1e,
        LMPylRtlrn                         = 0x81,
        LMPylLtlrn                         = 0x91,
        LMPylUturn                         = 0x86,
        LMPylSdrt45                        = 0xa0,
        LMPylSdlt45                        = 0xb0,
        LMPylSsrt90                        = 0x82,
        LMPylSslt90                        = 0x92,
        LMPylSdrt135                       = 0xa4,
        LMPylSdlt135                       = 0xb4,
        LMPylSsrt180                       = 0x85,
        LMPylSslt180                       = 0x95,
        LMPylDsrt45                        = 0xc0,
        LMPylDslt45                        = 0xd0,
        LMPylDdrt90                        = 0xe3,
        LMPylDdlt90                        = 0xf3,
        LMPylDsrt135                       = 0xc4,
        LMPylDslt135                       = 0xd4,
        LMPylFwd1                          = 0x01,
        LMPylFwd2                          = 0x02,
        LMPylFwd3                          = 0x03,
        LMPylFwd4                          = 0x04,
        LMPylFwd5                          = 0x05,
        LMPylFwd6                          = 0x06,
        LMPylFwd7                          = 0x07,
        LMPylFwd8                          = 0x08,
        LMPylFwd9                          = 0x09,
        LMPylFwd10                         = 0x0a,
        LMPylFwd11                         = 0x0b,
        LMPylFwd12                         = 0x0c,
        LMPylFwd13                         = 0x0d,
        LMPylFwd14                         = 0x0e,
        LMPylFwd15                         = 0x0f,
        LMPylDfwd1                         = 0x61,
        LMPylDfwd2                         = 0x62,
        LMPylDfwd3                         = 0x63,
        LMPylDfwd4                         = 0x64,
        LMPylDfwd5                         = 0x65,
        LMPylDfwd6                         = 0x66,
        LMPylDfwd7                         = 0x67,
        LMPylDfwd8                         = 0x68,
        LMPylDfwd9                         = 0x69,
        LMPylDfwd10                        = 0x6a,
        LMPylDfwd11                        = 0x6b,
        LMPylDfwd12                        = 0x6c,
        LMPylDfwd13                        = 0x6d,
        LMPylDfwd14                        = 0x6e,
        LMPylDfwd15                        = 0x6f,
        LMPylDfwd16                        = 0x70,
        LMPylDfwd17                        = 0x71,
        LMPylDfwd18                        = 0x72,
        LMPylDfwd19                        = 0x73,
        LMPylDfwd20                        = 0x74,
        LMPylDfwd21                        = 0x75,
        LMPylDfwd22                        = 0x76,
        LMPylDfwd23                        = 0x77,
        LMPylDfwd24                        = 0x78,
        LMPylDfwd25                        = 0x79,
        LMPylDfwd26                        = 0x7a,
        LMPylDfwd27                        = 0x7b,
        LMPylDfwd28                        = 0x7c,

        LMPylFwd0                          = 0x1f,
        LMPylDFwd0                         = 0x7f,
    };

#if !__LINE__

    class MyStrings
    {
        public string[] sCmd = {
                "CmdPrintString",
                "CmdError",                         // 0x01
                "CmdMarkedWalls",                   // 0x02
                "CmdCellLoc",                       // 0x03
                "CmdOrient",                        // 0x04
                "CmdAbortPath",                     // 0x05
                "CmdUTurn",                         // 0x06
                "CmdConvFactors",                   // 0x07
                "CmdProfServoOnly",                 // 0x08
                "CmdProfServoSensors",              // 0x09
                "CmdCorrector",                     // 0x0a
                "CmdRunStart",                      // 0x0b
                "CmdRunEndSuccess",                 // 0x0c
                "CmdRunEndFail",                    // 0x0d
                "CmdTurnParam",                     // 0x0e
                "CmdPath",                          // 0x0f
                "CmdMove",                          // 0x10
                "CmdSensDriveLow",                  // 0x11
                "CmdSensDriveHigh",                 // 0x12
                "CmdNOP",                           // 0x13
                "CmdAbortPathFrontWall",            // 0x14
                "CmdAbortPathSideWall",             // 0x15
                "CmdCorrector2",                    // 0x16
                "CmdLogDataFormat",                 // 0x17
                "CmdLogData",                       // 0x18
                "LMCmdNextAddr",                    // 0x19

                "CmdNone_0x1a",
                "CmdNone_0x1b",
                "CmdNone_0x1c",
                "CmdNone_0x1d",
                "CmdNone_0x1e",
                "CmdNone_0x1f",

                "CmdNone_0x20",
                "CmdNone_0x21",
                "CmdNone_0x22",
                "CmdNone_0x23",
                "CmdNone_0x24",
                "CmdNone_0x25",
                "CmdNone_0x26",
                "CmdNone_0x27",
                "CmdNone_0x28",
                "CmdNone_0x29",
                "CmdNone_0x2a",
                "CmdNone_0x2b",
                "CmdNone_0x2c",
                "CmdNone_0x2d",
                "CmdNone_0x2e",
                "CmdNone_0x2f",

                "CmdNone_0x30",
                "CmdNone_0x31",
                "CmdNone_0x32",
                "CmdNone_0x33",
                "CmdNone_0x34",
                "CmdNone_0x35",
                "CmdNone_0x36",
                "CmdNone_0x37",
                "CmdNone_0x38",
                "CmdNone_0x39",
                "CmdNone_0x3a",
                "CmdNone_0x3b",
                "CmdNone_0x3c",
                "CmdNone_0x3d",
                "CmdNone_0x3e",
                "CmdNone_0x3f",

                "CmdNone_0x40",
                "CmdNone_0x41",
                "CmdNone_0x42",
                "CmdNone_0x43",
                "CmdNone_0x44",
                "CmdNone_0x45",
                "CmdNone_0x46",
                "CmdNone_0x47",
                "CmdNone_0x48",
                "CmdNone_0x49",
                "CmdNone_0x4a",
                "CmdNone_0x4b",
                "CmdNone_0x4c",
                "CmdNone_0x4d",
                "CmdNone_0x4e",
                "CmdNone_0x4f",

                "CmdNone_0x50",
                "CmdNone_0x51",
                "CmdNone_0x52",
                "CmdNone_0x53",
                "CmdNone_0x54",
                "CmdNone_0x55",
                "CmdNone_0x56",
                "CmdNone_0x57",
                "CmdNone_0x58",
                "CmdNone_0x59",
                "CmdNone_0x5a",
                "CmdNone_0x5b",
                "CmdNone_0x5c",
                "CmdNone_0x5d",
                "CmdNone_0x5e",
                "CmdNone_0x5f",

                "CmdNone_0x60",
                "CmdNone_0x61",
                "CmdNone_0x62",
                "CmdNone_0x63",
                "CmdNone_0x64",
                "CmdNone_0x65",
                "CmdNone_0x66",
                "CmdNone_0x67",
                "CmdNone_0x68",
                "CmdNone_0x69",
                "CmdNone_0x6a",
                "CmdNone_0x6b",
                "CmdNone_0x6c",
                "CmdNone_0x6d",
                "CmdNone_0x6e",
                "CmdNone_0x6f",

                "CmdNone_0x70",
                "CmdNone_0x71",
                "CmdNone_0x72",
                "CmdNone_0x73",
                "CmdNone_0x74",
                "CmdNone_0x75",
                "CmdNone_0x76",
                "CmdNone_0x77",
                "CmdNone_0x78",
                "CmdNone_0x79",
                "CmdNone_0x7a",
                "CmdNone_0x7b",
                "CmdNone_0x7c",
                "CmdNone_0x7d",
                "CmdNone_0x7e",
                "CmdNone_0x7f",

                "CmdNone_0x80",
                "CmdNone_0x81",
                "CmdNone_0x82",
                "CmdNone_0x83",
                "CmdNone_0x84",
                "CmdNone_0x85",
                "CmdNone_0x86",
                "CmdNone_0x87",
                "CmdNone_0x88",
                "CmdNone_0x89",
                "CmdNone_0x8a",
                "CmdNone_0x8b",
                "CmdNone_0x8c",
                "CmdNone_0x8d",
                "CmdNone_0x8e",
                "CmdNone_0x8f",

                "CmdNone_0x90",
                "CmdNone_0x91",
                "CmdNone_0x92",
                "CmdNone_0x93",
                "CmdNone_0x94",
                "CmdNone_0x95",
                "CmdNone_0x96",
                "CmdNone_0x97",
                "CmdNone_0x98",
                "CmdNone_0x99",
                "CmdNone_0x9a",
                "CmdNone_0x9b",
                "CmdNone_0x9c",
                "CmdNone_0x9d",
                "CmdNone_0x9e",
                "CmdNone_0x9f",

                "CmdNone_0xa0",
                "CmdNone_0xa1",
                "CmdNone_0xa2",
                "CmdNone_0xa3",
                "CmdNone_0xa4",
                "CmdNone_0xa5",
                "CmdNone_0xa6",
                "CmdNone_0xa7",
                "CmdNone_0xa8",
                "CmdNone_0xa9",
                "CmdNone_0xaa",
                "CmdNone_0xab",
                "CmdNone_0xac",
                "CmdNone_0xad",
                "CmdNone_0xae",
                "CmdNone_0xaf",

                "CmdNone_0xb0",
                "CmdNone_0xb1",
                "CmdNone_0xb2",
                "CmdNone_0xb3",
                "CmdNone_0xb4",
                "CmdNone_0xb5",
                "CmdNone_0xb6",
                "CmdNone_0xb7",
                "CmdNone_0xb8",
                "CmdNone_0xb9",
                "CmdNone_0xba",
                "CmdNone_0xbb",
                "CmdNone_0xbc",
                "CmdNone_0xbd",
                "CmdNone_0xbe",
                "CmdNone_0xbf",

                "CmdLatCorrEnable",                   // 0xc0
                "CmdLatCorrDisable",
                "CmdLatCorrHold",
                "CmdLatCorrStart",
                "CmdLatCorrAlreadyEnabled",
                "CmdLongCorrAlreadyEnabled",
                "CmdLongLeftDiagSensorPeakIndexIsZero",     // <- no longer used but keep for old data files
                "CmdLongRightDiagSensorPeakIndexIsZero",    // <- no longer used but keep for old data files
                "CmdLongCorrDataReset",

                "CmdNone_0xc9",
                "CmdNone_0xca",
                "CmdNone_0xcb",
                "CmdNone_0xcc",
                "CmdNone_0xcd",
                "CmdNone_0xce",
                "CmdNone_0xcf",

                //
                // This command indicates that diagonal correction is in progress.
                //
                // To minimize the size of the command", the payload is combined into the
                // lower nibble. So", this command takes up sixteen entries.
                //
                "CmdDiagCorrInitLeft",              // 0xd0
                "CmdDiagCorrInitRight",             // 0xd1
                "CmdDiagLatCorrHold",               // 0xd2
                "CmdDiagLatCorr2Sample1",           // 0xd3
                "CmdDiagLatCorr2Sample2Do",         // 0xd4
                "CmdDiagLongCorrDo",                // 0xd5
                "CmdDiagMoveUpdate",                // 0xd6
                "CmdDiagLatCorr1Sample1",           // 0xd7
                "CmdDiagLatCorr1Sample2Do",         // 0xd8
                "CmdDiagLatCorrDoLeft",             // 0xd9
                "CmdDiagLatCorrDoRight",            // 0xda
                "CmdDiagLatCorrDoLeftOffset",       // 0xdb
                "CmdDiagLatCorrDoRightOffset",      // 0xdc
                // Payload for "CmdDiagLatCorrDoLeftOffset and "CmdDiagLatCorrDoRightOffset
                // is the error in SByte 6.2 mm format.
                // error = slLeftDist - slRightDist:
                // > 0 => go left to correct
                // < = => go right to correct
                "CmdDiagLongCorrCorrection",        // 0xdd
                // Payload for "CmdDiagLongCorrCorrection is SWord that is the error.
                // > 0 => go further
                // < = => stop sooner
                "CmdDiagCorrNotUsed",               // 0xde
                "CmdDiagCorrSkip",                  // 0xdf
                //
                // This command indicates that longitudinal correction is in progress.
                //
                // To minimize the size of the command", the payload is combined into the
                // lower nibble. So", this command takes up sixteen entries.
                //
                "CmdLongCorrEnable",                // 0xe0
                "CmdLongCorrSkip",                  // 0xe1
                "CmdLongCorrDataGather",            // 0xe2
                "CmdLongCorrLeftNoEdges",           // 0xe3
                "CmdLongCorrLeftFallingEdge",       // 0xe4
                "CmdLongCorrLeftRisingEdge",        // 0xe5
                "CmdLongCorrLeftBothEdges",         // 0xe6
                "CmdLongCorrRightNoEdges",          // 0xe7
                "CmdLongCorrRightFallingEdge",      // 0xe8
                "CmdLongCorrRightRisingEdge",       // 0xe9
                "CmdLongCorrRightBothEdges",        // 0xea
                "CmdLongCorrCorrection",            // 0xeb
                // Payload for "CmdLongCorrCorrection is SWord that is the error.
                // > 0 => go further
                // < = => stop sooner
                "CmdLongCorrAdd10mm",               // 0xec
                "CmdLongCorrHold",                  // 0xed
                "CmdLongCorrDisable",               // 0xee
                "CmdLongCorrError",                 // 0xef

                //
                // This command indicates that lateral correction is in progress.
                //
                // To minimize the size of the command", the payload is combined into the
                // lower nibble. So", this command takes up sixteen entries.
                //
                "CmdLatCorrInitReadWalls1",         // 0xf0
                "CmdLatCorrDoReadWalls2",           // 0xf1
                "CmdLatCorrDoReadWalls3NM_NoBT",    // 0xf2
                "CmdLatCorrDoReadWalls3NM_WithBT",  // 0xf3
                // NoBT    = WithOut Back Track
                // WithBT  = With Back Track
                "CmdLatCorrBothWalls",              // 0xf4
                "CmdLatCorrLeftWallOnly",           // 0xf5
                "CmdLatCorrRightWallOnly",          // 0xf6
                "CmdLatCorrPegs",                   // 0xf7
                "CmdLatCorrFrontWallDontUsePegs",   // 0xf8
                "CmdLatCorrNoCorrDeadBand",         // 0xf9
                "CmdLatCorrCorrection",             // 0xfa
                // Payload for "CmdLatCorrCorrection is the error in SByte 5.2 mm format.
                // error = slRightDist - slLeftDist:
                // > 0 => go right to correct
                // < 0 => go left to correct
                "CmdLatCorrDo",                     // 0xfb
                // 3/19/16: LatCorrDo is deprecated because we correct every mm
                "CmdLatCorrDone",                   // 0xfc
                "CmdLatCorrCorrection2",            // 0xfd
                // Payload for "CmdLatCorrCorrection2 is the error in SByte 5.2 mm format.
                // error = slRightDist - slLeftDist:
                // > 0 => go right to correct
                // < 0 => go left to correct
                // Second byte with wall status:
                // 0 = no wall
                // 1 = left wall
                // 2 = right wall
                // 3 = left wall with no right wall and in the lateral correction zone
                // 4 = right wall with no left wall and in the lateral correction zone
                "CmdLatCorrNotUsed14",
                "CmdLatCorrError"                   // 0xff
            };

        public string LM_STRING_MARKED_NORTH_WALL = "North";
        public string LM_STRING_MARKED_EAST_WALL = "East";
        public string LM_STRING_MARKED_SOUTH_WALL = "South";
        public string LM_STRING_MARKED_WEST_WALL = "West";

        public string[] sOrient =  {
                                            "North",
                                            "NorthEast",
                                            "East",
                                            "SouthEast",
                                            "South",
                                            "SouthWest",
                                            "West",
                                            "NorthWest"
                                    };

        public string[] sError =    {
                                            "Flash Unknown DeviceID",
                                            "Log Size is Zero",
                                            "AnalyzePath DeltaPos is Zero",
                                            "No Path To Goal",
                                            "Orient Bad in Update Coord",
                                            "Orient Bad In ConvWalls",
                                            "slCoordXY Bad",
                                            "LongCorr Too Much Data",
                                            "UTurn Align Error Too High",
                                            "Turn FinVel Is Zero",
                                            "Unknown Motion Command",
                                            "LatCorr Dont Understand1",
                                            "LatCorr Dont Understand2",
                                            "MapMaze Dont Understand1",
                                            "Battery Low",
                                            "Fwd Error High",
                                            "Rot Error High",
                                            "Setup Stop In Center Dont Understand1",
                                            "Setup Stop In Center Dont Understand2",
                                            "Mouse Upside Down"
                                };

        public string[] sLongCorr = {
                                            "Enable",
                                            "Skip",
                                            "Data Gather",
                                            "No Left Edges",
                                            "Left Falling Edge",
                                            "Left Rising Edge",
                                            "Left Peg",
                                            "No Right Edges",
                                            "Right Falling Edge",
                                            "Right Rising Edge",
                                            "Right Peg",
                                            "Correction",
                                            "Added 10mm",
                                            "Hold",
                                            "Disable",
                                            "ERROR"
                               };

        public string[] sLatCorr = {
                                            "Init Read Walls 1",
                                            "Read Walls 2",
                                            "Read Walls 3 No Backtrack",
                                            "Read Walls 3 With Backtrack",
                                            "Both Walls",
                                            "Left Wall",
                                            "Right Wall",
                                            "Pegs",
                                            "Front Wall so Dont Use Pegs",
                                            "No Correction Dead Band",
                                            "Correction",
                                            "Do",
                                            "Done",
                                            "Not Used 13",
                                            "Not Used 14",
                                            "ERROR"
                                };

        public string[] sLatCorrAbbr = {
                                            "RW1",
                                            "RW2",
                                            "RW3NBT",
                                            "RW3BT"
                                };

        public string[] sLatCorr2Walls = {
                                            "Pegs",
                                            "Left Wall",
                                            "Right Wall",
                                            "Left Wall Zone",
                                            "Right Wall Zone"
                                        };

        public string[] sLatCorr2WallsAbbr = {
                                            "P:",
                                            "L:",
                                            "R:",
                                            "LZ:",
                                            "RZ:"
                                        };

        public string[] sDiagCorr = {
                                            "Peak on Left",
                                            "Peak on Right",
                                            "Hold",
                                            "Window 2 Sample 1",
                                            "Window 2 Sample 2",
                                            "Long Corr Do",
                                            "Move update",
                                            "Window 1 Sample 1",
                                            "Window 1 Sample 2",
                                            "Front Left clear",
                                            "Front Right clear",
                                            "Front Left adjust",
                                            "Front Right adjust",
                                            "Correction",
                                            "ERROR: Not Used",
                                            "Skip"
                                };

        public string[] sLatCorrNew = {
                                            "Enable",
                                            "Disable",
                                            "Hold",
                                            "Start",
                                            "Already Enabled"
                                };


        public string[] sLatCorrNewAbbr = {
                                            "En",
                                            "Dis",
                                            "Hold",
                                            "Start",
                                            "AE"
                                };


        public string[] sLongCorrNew = {
                                            "Already Enabled",
                                            "LeftDiagSensorPeakIndexIsZero",
                                            "RightDiagSensorPeakIndexIsZero",
                                            "Data Reset"
                                 };
    };
#endif // !__LINE__

#if __LINE__
#undef public
#else
}
#endif // __LINE__
