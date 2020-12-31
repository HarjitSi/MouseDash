using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using MySerialMsgNamespace;
using System.Threading;
using System.IO;
using System.Diagnostics;

// serial port code: http://stackoverflow.com/questions/13754694/what-is-the-correct-way-to-read-a-serial-port-using-net-framework
// Serial library: <https://serial.codeplex.com/SourceControl/latest#source/Port.cs> 



namespace MouseDashNameSpace
{
    public partial class SerialUI : Form
    {
        static SerialPort serialPort;

        // every time we have a packet error, increment this
        int packetError = 0;

        int chipSize = 0;
        int packetNumber = 0;

        // the packet size is only one byte in length, so the max array is one byte - 1 since we can only go to 255
        byte[] logData = new byte[255];

        Queue<byte> receiveBuffer = new Queue<byte>();

        List<byte> receiveArray = new List<byte>();
        
        static string sFileDebug = @"C:\Depot\Source\ZVII\ProtocolDebug.txt";
        static string sFileData = @"C:\Depot\Source\ZVII\Data.bin";

        StreamWriter streamWriterDebug;

        int bytesReceived = 0;

        long dataRate = 0;
        long dataRateBytesReceived = 0;

        Stopwatch watch = new Stopwatch();

        public SerialUI()
        {
            InitializeComponent();

            // only allow one item to be selected
            listBoxCOMPort.SelectionMode = SelectionMode.One;

            // prevent the listbox from drawing after every item is added
            listBoxCOMPort.BeginUpdate();

            List<string> COMPorts = new List<string>();
            foreach (String portName in SerialPort.GetPortNames())
            {
                COMPorts.Add(portName);
                listBoxCOMPort.Items.Add(portName);
            }

            // allow updates of the listbox again
            listBoxCOMPort.EndUpdate();
        }

        private void buttonConnect_click(object sender, EventArgs e)
        {
            if (listBoxCOMPort.Items.Count == 0)
            {
                return;
            }

            if (serialPort != null)
            {
                // must not have gotten a response from the mouse, so, try again
                if (serialPort.IsOpen)
                {
                    sendCmdConnection();

                    return;
                }
            }

            string selectedPort = "";

            listBoxCOMPort.SetSelected(0, true);

            selectedPort = listBoxCOMPort.SelectedItem.ToString();

            serialPort = new SerialPort(selectedPort, 3000000, Parity.None, 8, StopBits.One);

            serialPort.Handshake = Handshake.RequestToSend;

            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Serial Port (" + selectedPort.ToString() + ") Open Error: " + ex.ToString());
            }

            if (serialPort.IsOpen)
            {
                serialPort.DataReceived += SerialPort_DataReceived;

                sendCmdConnection();
            }

            streamWriterDebug = File.CreateText(sFileDebug);

            watch.Start();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = serialPort.BytesToRead;
            bytesReceived += bytesToRead;

            dataRateBytesReceived += bytesToRead;

            if (watch.ElapsedMilliseconds >= 300)
            {
                dataRate = dataRateBytesReceived * 1000 / watch.ElapsedMilliseconds;

                watch.Restart();
                dataRateBytesReceived = 0;

                Invoke((MethodInvoker)delegate { textBoxRate.Text = dataRate.ToString(); });
                Invoke((MethodInvoker)delegate { textBoxReadBufLen.Text = bytesToRead.ToString(); });
                Invoke((MethodInvoker)delegate { textBoxWriteBufLen.Text = serialPort.BytesToWrite.ToString(); });
            }

            byte[] data = new byte[bytesToRead];
            serialPort.Read(data, 0, data.Length);
            data.ToList().ForEach(b => receiveBuffer.Enqueue(b));

            processSerialData();
        }

        private void sendCmdConnection()
        {
            byte[] cmd = new byte[] { (byte)SMCmd.SMCmdHeaderOne, (byte)SMCmd.SMCmdHeaderTwo, (byte)SMCmd.SMCmdConnection, 0 };

            byte checksum = 0;
            foreach (byte element in cmd)
            {
                checksum += element;
            }
            cmd[cmd.Length - 1] = (byte)-checksum;

            serialPort.Write(cmd, 0, cmd.Length);
        }

        private void AcknowledgePacket(int packetNum)
        {
            byte[] cmd = new byte[] { (byte)SMCmd.SMCmdHeaderOne, (byte)SMCmd.SMCmdHeaderTwo, (byte)SMCmd.SMCmdAcknowledge, 0, 0, 0, 0 };

            cmd[3] = (byte)(packetNum & 0xff);
            cmd[4] = (byte)((packetNum >> 8) & 0xff);
            cmd[5] = (byte)((packetNum >> 16) & 0xff);

            byte checksum = 0;
            foreach (byte element in cmd)
            {
                checksum += element;
            }
            cmd[cmd.Length - 1] = (byte)-checksum;

            serialPort.Write(cmd, 0, cmd.Length);

            streamWriterDebug.WriteLine("Ack: " + packetNum.ToString());

            if ((packetNumber % 100) == 0)
            {
                Invoke((MethodInvoker)delegate { textBoxStatus.Text = packetNumber.ToString(); });
            }

            return;
        }

        private void processSerialData()
        {
            if (receiveBuffer.Count < (byte)SMCmdLen.SMRESP_MIN_LEN)
                return;

            bool keepProcessing = true;

            //ERROR add flag and use it to bail from the while loop if we don't have enough bytes for a command

            while ((receiveBuffer.Count >= (byte) SMCmdLen.SMRESP_MIN_LEN) && keepProcessing)
            {
                byte rxData = receiveBuffer.ElementAt(0);

                if (rxData != (byte)SMCmd.SMRespHeaderOne)
                {
                    receiveBuffer.Dequeue();
                    packetError++;
                    Invoke((MethodInvoker)delegate { textBoxError.Text = "SMRespHeaderOne"; });
                    continue;
                }
                rxData = receiveBuffer.ElementAt(1);

                if (rxData != (byte)SMCmd.SMRespHeaderTwo)
                {
                    receiveBuffer.Dequeue();
                    packetError++;
                    Invoke((MethodInvoker)delegate { textBoxError.Text = "SMRespHeaderTwo"; });
                    continue;
                }

                SMCmd rxCmd = (SMCmd) receiveBuffer.ElementAt(2);

                byte checksum = (byte)SMCmd.SMRespHeaderOne + (byte)SMCmd.SMRespHeaderTwo;
                checksum += (byte)rxCmd;

                switch (rxCmd)
                {
                    case SMCmd.SMRespConnection:
                        if (receiveBuffer.Count() < (int)SMCmdLen.SMRespConnectionLen)
                        {
                            keepProcessing = false;
                            break;
                        }
                        else
                        {
                            checksum += receiveBuffer.ElementAt(3);
                            checksum += receiveBuffer.ElementAt(4);
                            checksum += receiveBuffer.ElementAt(5);
                            checksum += receiveBuffer.ElementAt(6);
                            checksum += receiveBuffer.ElementAt(7);

                            if (checksum != 0)
                            {
                                // strip off the header and try parsing the rest again
                                receiveBuffer.Dequeue();
                                receiveBuffer.Dequeue();
                                packetError++;
                                Invoke((MethodInvoker)delegate { textBoxError.Text = "RespConnection Checksum"; });

                                chipSize = 0;
                            }
                            else
                            {
                                byte[] temp = new byte[] { receiveBuffer.ElementAt(3), receiveBuffer.ElementAt(4), receiveBuffer.ElementAt(5), receiveBuffer.ElementAt(6) }; ;

                                chipSize = BitConverter.ToInt32(temp, 0);

                                for (var i = 0; i < (int)SMCmdLen.SMRespConnectionLen; i++)
                                {
                                    receiveBuffer.Dequeue();
                                }

                                Invoke((MethodInvoker)delegate {
                                    textBoxStatus.Text = chipSize.ToString("D");

                                    buttonConnect.Enabled = false;
                                    buttonEraseChip.Enabled = true;
                                    buttonEraseData.Enabled = true;
                                    buttonDownload.Enabled = true;
                                });
                            }
                        }
                        break;

                    case SMCmd.SMRespDownload:
                        if (receiveBuffer.Count() < (int)SMCmdLen.SMRespDownloadLen)
                        {
                            keepProcessing = false;
                            break;
                        }
                        // handle the case where there is no payload
                        else if (receiveBuffer.ElementAt(6) == 0)
                        {
                            checksum += receiveBuffer.ElementAt(3);
                            checksum += receiveBuffer.ElementAt(4);
                            checksum += receiveBuffer.ElementAt(5);
                            checksum += receiveBuffer.ElementAt(6);
                            checksum += receiveBuffer.ElementAt(7);

                            if (checksum == 0)
                            {
                                byte[] temp = new byte[] { receiveBuffer.ElementAt(3), receiveBuffer.ElementAt(4), receiveBuffer.ElementAt(5), 0 };

                                packetNumber = BitConverter.ToInt32(temp, 0);

                                streamWriterDebug.WriteLine("Rcv: " + packetNumber.ToString());

                                AcknowledgePacket(packetNumber);

                                for (var i = 0; i < (int)SMCmdLen.SMRespDownloadLen; i++)
                                {
                                    receiveBuffer.Dequeue();
                                }
                            }
                            else
                            {
                                // strip off the header and try parsing the rest again
                                receiveBuffer.Dequeue();
                                receiveBuffer.Dequeue();
                                packetError++;

                                streamWriterDebug.WriteLine("DwnErr: " + packetError.ToString());

                                Invoke((MethodInvoker)delegate { textBoxError.Text = "RespDownload Checksum 1"; });
                            }
                        }
                        else if (receiveBuffer.Count() >= (int)SMCmdLen.SMRespDownloadLen + receiveBuffer.ElementAt(6))
                        {
                            checksum += receiveBuffer.ElementAt(3);
                            checksum += receiveBuffer.ElementAt(4);
                            checksum += receiveBuffer.ElementAt(5);
                            checksum += receiveBuffer.ElementAt(6);

                            var count = receiveBuffer.ElementAt(6);

                            for (var i = 0; i < count; i++)
                            {
                                checksum += receiveBuffer.ElementAt(i + 7);
                            }

                            // the - 1 is to convert count to last item
                            checksum += receiveBuffer.ElementAt((int)SMCmdLen.SMRespDownloadLen + count - 1);

                            if (checksum == 0)
                            {
                                byte[] temp = new byte[] { receiveBuffer.ElementAt(3), receiveBuffer.ElementAt(4), receiveBuffer.ElementAt(5), 0 };

                                packetNumber = BitConverter.ToInt32(temp, 0);

                                AcknowledgePacket(packetNumber);
                                
                                // get rid of the header and command stuff
                                // the -1 is to account for the checksum
                                for (var i = 0; i < (int)SMCmdLen.SMRespDownloadLen - 1; i++)
                                {
                                    receiveBuffer.Dequeue();
                                }

                                for (var i = 0; i < count; i++)
                                {
                                    logData[i] = receiveBuffer.Dequeue();
                                }

                                receiveArray.AddRange(logData);

                                // get and dump the checksum
                                receiveBuffer.Dequeue();
                            }
                            else
                            {
                                // strip off the header and try parsing the rest again
                                receiveBuffer.Dequeue();
                                receiveBuffer.Dequeue();
                                packetError++;
                                Invoke((MethodInvoker)delegate { textBoxError.Text = "RespDownload Checksum 2"; });
                            }
                        }
                        else 
                        {
                            keepProcessing = false;
                            break;
                        }

                        break;

                    case SMCmd.SMRespErase:
                        if (receiveBuffer.Count() < (int)SMCmdLen.SMRespEraseLen)
                        {
                            keepProcessing = false;
                            break;
                        }
                        else
                        {
                            checksum += receiveBuffer.ElementAt(3);
                            checksum += receiveBuffer.ElementAt(4);
                            checksum += receiveBuffer.ElementAt(5);
                            checksum += receiveBuffer.ElementAt(6);
                            checksum += receiveBuffer.ElementAt(7);

                            if (checksum != 0)
                            {
                                // strip off the header and try parsing the rest again
                                receiveBuffer.Dequeue();
                                receiveBuffer.Dequeue();
                                packetError++;
                                Invoke((MethodInvoker)delegate { textBoxError.Text = "RespErase Checksum"; });
                            }
                            else
                            {
                                byte[] temp = new byte[] { receiveBuffer.ElementAt(3), receiveBuffer.ElementAt(4), receiveBuffer.ElementAt(5), 0 };

                                var eraseNumber = BitConverter.ToInt32(temp, 0);

                                bool eraseMessage = (receiveBuffer.ElementAt(6) != 0) ? true : false;

                                if (eraseMessage)
                                {
                                    Invoke((MethodInvoker)delegate { textBoxStatus.Text = "Done"; });
                                }
                                else
                                {
                                    Invoke((MethodInvoker)delegate { textBoxStatus.Text = eraseNumber.ToString(); });
                                }

                                for (var i = 0; i < (int)SMCmdLen.SMRespEraseLen; i++)
                                {
                                    receiveBuffer.Dequeue();
                                }
                            }
                        }
                        break;

                    case SMCmd.SMRespPrintString:
                        if (receiveBuffer.Count() < (int)SMCmdLen.SMRespPrintStringLen)
                        {
                            keepProcessing = false;
                            break;
                        }
                        // handle the case where there is no payload
                        else if (receiveBuffer.ElementAt(3) == 0)
                        {
                            checksum += receiveBuffer.ElementAt(3);
                            checksum += receiveBuffer.ElementAt(4);

                            if (checksum == 0)
                            {
                                for (var i = 0; i < (int)SMCmdLen.SMRespPrintStringLen; i++)
                                {
                                    receiveBuffer.Dequeue();
                                }
                            }
                            else
                            {
                                // strip off the header and try parsing the rest again
                                receiveBuffer.Dequeue();
                                receiveBuffer.Dequeue();
                                packetError++;
                                Invoke((MethodInvoker)delegate { textBoxError.Text = "RespPrintString Checksum 1"; });
                            }
                        }
                        else if (receiveBuffer.Count() >= (int)SMCmdLen.SMRespPrintStringLen + receiveBuffer.ElementAt(3))
                        {
                            checksum += receiveBuffer.ElementAt(3);

                            var count = receiveBuffer.ElementAt(3);

                            for (var i = 0; i < count; i++)
                            {
                                checksum += receiveBuffer.ElementAt(i + 4);
                            }

                            // the - 1 is to convert count to last item
                            checksum += receiveBuffer.ElementAt((int)SMCmdLen.SMRespPrintStringLen + count - 1);

                            if (checksum == 0)
                            {
                                // get rid of the header and command stuff
                                // the -1 is to account for the checksum
                                for (var i = 0; i < (int)SMCmdLen.SMRespPrintStringLen - 1; i++)
                                {
                                    receiveBuffer.Dequeue();
                                }

                                for (var i = 0; i < count; i++)
                                {
                                    logData[i] = receiveBuffer.Dequeue();
                                }

                                string stringMessage = Encoding.UTF8.GetString(logData, 0, count);

                                // get and dump the checksum
                                receiveBuffer.Dequeue();
                            }
                            else
                            {
                                // strip off the header and try parsing the rest again
                                receiveBuffer.Dequeue();
                                receiveBuffer.Dequeue();
                                packetError++;
                                Invoke((MethodInvoker)delegate { textBoxError.Text = "RespPrintString Checksum 2"; });
                            }
                        }
                        break;

                    default:
                        continue;
                }
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            byte[] cmd = new byte[] { (byte)SMCmd.SMCmdHeaderOne, (byte)SMCmd.SMCmdHeaderTwo, (byte)SMCmd.SMCmdDownload, 0 };

            byte checksum = 0;
            foreach (byte element in cmd)
            {
                checksum += element;
            }
            cmd[cmd.Length - 1] = (byte)-checksum;

            serialPort.Write(cmd, 0, cmd.Length);
        }

        private void buttonEraseData_Click(object sender, EventArgs e)
        {
            byte[] cmd = new byte[] { (byte)SMCmd.SMCmdHeaderOne, (byte)SMCmd.SMCmdHeaderTwo, (byte)SMCmd.SMCmdSectorErase, 0 };

            byte checksum = 0;
            foreach (byte element in cmd)
            {
                checksum += element;
            }
            cmd[cmd.Length - 1] = (byte)-checksum;

            serialPort.Write(cmd, 0, cmd.Length);
        }

        private void buttonEraseChip_Click(object sender, EventArgs e)
        {
            byte[] cmd = new byte[] { (byte)SMCmd.SMCmdHeaderOne, (byte)SMCmd.SMCmdHeaderTwo, (byte)SMCmd.SMCmdBulkErase, 0 };

            byte checksum = 0;
            foreach (byte element in cmd)
            {
                checksum += element;
            }
            cmd[cmd.Length - 1] = (byte)-checksum;

            serialPort.Write(cmd, 0, cmd.Length);
        }

        private void formSerialUI_Closing(object sender, FormClosingEventArgs e)
        {
            if (streamWriterDebug != null)
            {
                streamWriterDebug.Close();
            }

            File.WriteAllBytes(sFileData, receiveArray.ToArray());

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    Thread closeThread = new Thread(new ThreadStart(ThreadClose));
                    closeThread.Start();
                }
            }
        }

        private static void ThreadClose()
        {
            serialPort.Close();
        }
    }
}
