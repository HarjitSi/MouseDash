/**************************************************************************
*
*            NOTICE OF COPYRIGHT
*             Copyright (C) 2010
*                Team Zeetah
*            ALL RIGHTS RESERVED
*
* File:         SerialMsg.cs
*
* Written By:   Harjit Singh
*
* Date:         12/26/2016
*
* Purpose:      This file contains the values used when communicating between
*               PC and the mouse. This file is used by the mouse code and the
*               PC dash code
*
* Notes:        Sharing files between C & C#:
*               http://stackoverflow.com/questions/954321/is-it-possible-to-share-an-enum-declaration-between-c-sharp-and-unmanaged-c
*
* To Be Done:
*
*-Date----------Author------Description--------------------------------------
*  12/25/16     HarjitS     Created from LogMsg.cs
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

namespace MySerialMsgNamespace
{

#endif // __LINE__

    public enum SMVersion
    {
        SMCurVersion                        = 1
    };

    //
    // All commands are documented in OneNote:Micromouse->Mouse Code->
    // Logging And Dash->Serial Protocol
    //
    public enum SMCmd
    {
        SMCmdHeaderOne                      = 'Z',
        SMCmdHeaderTwo                      = '7',

        SMRespHeaderOne                     = 'z',
        SMRespHeaderTwo                     = '7',

        //
        // This command is used to test the connection between the PC and the
        // mouse
        //
        SMCmdConnection                     = 'C',
        SMRespConnection                    = 'c',

        // This command is used to download data from the mouse to the PC
        SMCmdDownload                       = 'D',
        SMRespDownload                      = 'd',

        //
        // This command is used to acknowledge that the PC has received a
        // packet
        //
        SMCmdAcknowledge                    = 'A',

        //
        // This command is used to erase the flash sectors that are currently
        // in use
        //
        SMCmdSectorErase                    = 'S',
        // This command is used to erase the entire chip
        SMCmdBulkErase                      = 'B',
        SMRespErase                         = 'e',

        //
        // This command is used to send an arbitrary set of characters to the
        // PC for printing to the display
        //
        SMRespPrintString                   = 'p',
    };

    public enum SMCmdLen
    {
        //
        // Values used in command/response parsing
        // NOTE: These don't include, if applicable, the variable payload
        //
        SMCMD_MIN_LEN                       = 4,

        SMRESP_MIN_LEN                      = 4,

        //
        // This command is used to test the connection between the PC and the
        // mouse
        //
        SMCmdConnectionLen                  = 4,    // Z7 C <checksum>
        SMRespConnectionLen                 = 8,    // z7 c <chip size> <checksum>

        // This command is used to download data from the mouse to the PC
        SMCmdDownloadLen                    = 4,    // Z7 D <checksum>
        SMRespDownloadLen                   = 8,    // z7 d <packet number>
                                                    // <byte count> <payload>
                                                    // <checksum>

        //
        // This command is used to acknowledge that the PC has received a
        // packet
        //
        SMCmdAcknowledgeLen                 = 7,    // Z7 A <packet number>
                                                    // <checksum>

        //
        // This command is used to erase the flash sectors that are currently
        // in use
        //
        SMCmdSectorEraseLen                 = 4,    // Z7 S <checksum>
        // This command is used to erase the entire chip
        SMCmdBulkEraseLen                   = 4,    // Z7 B <checksum>
        SMRespEraseLen                      = 8,    // z7 e <packet number>
                                                    // <done> <checksum>

        SMRespPrintStringLen                = 5,    // z7 p <byte count>
                                                    // <payload> <checksum>
    };

#if __LINE__
#undef public
#else
}
#endif // __LINE__
