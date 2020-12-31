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

//
// USE DYNAMIC EXAMPLE AND MAKE SCROLLING SMOOTH
//
// SET IsOptimizedDraw to true that is used to speed up drawing; this happens to be set false by default
// because of compatibility concerns.
//

// NOTES: Links to info. that might be useful for MouseDash 
//
// Wiki:
// file:///C:/Temp/ZedGraph/Wiki/zedgraph.org/wiki/index4875.html?title=Main_Page
//
// to speed up drawing: myCurve.Line.IsOptimizedDraw = true
// file:///C:/Temp/ZedGraph/Wiki/zedgraph.org/wiki/index6cbc.html?title=Speed_up_the_redraw_time
//
// to draw an arbitrary line:
//
//            double threshHoldY = 20;
//
//            LineObj threshHoldLine = new LineObj(
//                                        Color.Red,
//                                        myPane.XAxis.Scale.Min,
//                                        threshHoldY,
//                                        myPane.XAxis.Scale.Max,
//                                        threshHoldY);
//
//            myPane.GraphObjList.Add(threshHoldLine);
//
// to do real time stuff:
// Display_Dynamic_or_Real-Time_Data
// file:///C:/Temp/ZedGraph/Wiki/zedgraph.org/wiki/index3061.html?title=Display_Dynamic_or_Real-Time_Data
// C:\Temp\ZedGraph\Wiki\zedgraph.org\wiki\index1556.html
//
// How to add real-time data in a dual-Y-axis ZedGraph graph using C#?
// http://stackoverflow.com/questions/3478482/how-to-add-real-time-data-in-a-dual-y-axis-zedgraph-graph-using-c
//
// Maybe redo the form update using a background thread
// http://msdn.microsoft.com/en-us/library/ms171728.aspx

// TODO: Add command to get a row of maze info and add a button for it so that we can get the
// TODO: info. after runs.
//
// Launch an external application from c#: http://omegacoder.com/?p=119
//
// Graphics - GDI: http://www.c-sharpcorner.com/UploadFile/mahesh/gdi_plus12092005070041AM/gdi_plus.aspx
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.IO;
using System.Reflection;
using MouseControls;
using MyConstNamespace;
using System.Linq;

public enum MyColors
{
    eColorNone,
    eColorError,
    eColorLongCorr,
    eColorLatCorr,
    eColorDiagCorr,
    eColorMaze,
    eColorMotion,
    eColorMotionTurn,
    eColorMisc,
    eColorMax
}

public enum Orient
{
    eNorth,
    eNorthEast,
    eEast,
    eSouthEast,
    eSouth,
    eSouthWest,
    eWest,
    eNorthWest,
    eOrientMax
}

//
// This structure is used to capture the wall marking information
//
public struct WallStruct
{
    public byte ucWalls;
};

//
// This structure is used to capture the cell coordinate, the mouse's
// direction whether we marked the wall in this cell and if so, index to the
// wall in the WallList
//
public struct CellStruct
{
    public int iCell;
    public Orient CurOrient;
    public bool bMarkedWalls;
    public int iWallListIndex;
};

//
// This structure is used to keep info. on which cells are in a run.
//
public struct RunStruct
{
    public int iRunNum;
    public double dFwdAccel;
    public double dLatAccel;
    public double dOrthoMaxVel;
    public double dDiagMaxVel;
    public string sRunMode;
    public uint ulRunStartTime;
    public uint ulRunTime;
    public bool bRunSuccess;
    public int iFirstCellIndex;
    public int iLastCellIndex;
    public List<string> lsRunErrors;
};

//
// This structure is used to map runGridView to Run/Cell/Path lists
//
public struct RunCellWallPathStruct
{
    public int iRunListIndex;
    public int iCellListIndex;
    public int iPathListIndex;
    public int iDGVIndex;
};

//
// This structure is used to map runGridView to Run/Cell lists
//
public struct PathStruct
{
    public byte[] path;
};

//
// This structure is used to store the corrector information
//
public struct CorrectorStruct
{
    public string sOffset;
    public string sOffsetDot;
    public string sAngle;
    public string sAngleDot;
};

//
// This structure is used to debug the parser
//
public struct ParseDebugStruct
{
    public int i;
    public int RowIndex;
    public string LogCmd;
    public byte[] LogData;
};

namespace MouseDashNameSpace
{
    public partial class MouseDash : Form
    {
        MyStrings ms = new MyStrings();

        //
        // If we have a runt file, and we don't have a "LMCmdRunEndFail" or
        // "LMCmdRunEndSuccess", then the data for the last run won't be
        // added and the dashboard code chokes, so, track if a run is started
        // and if it was, make sure to finish it
        //
        bool bHaveRunStartedButNotRunFinished = false;

        // Used to control whether we output debug info or not.
        bool bOutputDebugInfo = false;

        // File to writing out debug info.
        string sFileDebug = @"C:\Depot\Source\ZVII\Debug.csv";

        // File to write out log data
        string sFileLog = @"c:\Depot\Source\ZVII\Log.csv";
        StreamWriter streamWriterLog;

        string logDataFormat;

        // Used to control whether during load, we show the mouse being moved
        // or not
        bool bShowMouseDuringLoad = false;

        // Definitions pertaining to the colors we use in the Comment column.
        Color ColorError = Color.Red;
        Color ColorLongCorr = Color.OliveDrab;
        Color ColorLatCorr = Color.Orange;
        Color ColorDiagCorr = Color.BlueViolet;
        Color ColorMaze = Color.Crimson;
        Color ColorMotion = Color.SeaGreen;
        Color ColorMotionTurn = Color.Orange;
        Color ColorMisc = Color.Brown;

        Color[] myColors = new Color[(int)MyColors.eColorMax];

        //
        // This is the structure we use for extracting log data.
        //
        // Use a list because it can be expanded. This will be put
        // into strings for display in the DataGridView and into
        // FilteredPointList for use in graphs.
        //
        struct MotionProfileDataStruct
        {
            public double dmm;
            public double dLFP;
            public double dLDP;
            public double dRDP;
            public double dRFP;
            public double dLFD;
            public double dLDD;
            public double dRDD;
            public double dRFD;
            public double dFVel;
            public double dFErr;
            public double dRVel;
            public double dRPos;
            public double dRErr;
            public double dVBat;
        };

        List<MotionProfileDataStruct> MPDList = new List<MotionProfileDataStruct>();

        // This variation of the variables are used for the DataGridView.
        struct DGVStringStruct
        {
            public string smm;
            public string sLFP;
            public string sLDP;
            public string sRDP;
            public string sRFP;
            public string sLFD;
            public string sLDD;
            public string sRDD;
            public string sRFD;
            public string sFVel;
            public string sFErr;
            public string sRVel;
            public string sRPos;
            public string sRErr;

            public string sLat;
            public string sLong;

            public string sComment;
            public MyColors eCommentColor;

            // iCorrectorIndex is used to update the Correction data
            public int iCorrectorIndex;

            // iDataIndex is used to update the graphs
            public int iDataIndex;

            //
            // iRunGridViewListIndex is used to update the runGridView and
            // the path in the maze display.
            //
            public int iRunGridViewListIndex;
        };

        // DataGridView info. in a string list
        List<DGVStringStruct> DGVStringList = new List<DGVStringStruct>();

        // DataGridView info. in a string array
        DGVStringStruct[] DGVStringArray;

        //
        // This variable is used to inform the controls whether there is data
        // in the d??? and s??? variables for them to use.
        //
        bool bDataInitialized;

        //
        // This variable is used to track how many characters we can display
        // in the Comment column.
        //
        int iNumCharCommentColumn;

        //
        // This constant is used to track how many columns the runDataGridView
        // has.
        //
        const int iNumRunDataGridViewColumns = 5;

        List<RunStruct> RunsList = new List<RunStruct>();
        List<CellStruct> CellList = new List<CellStruct>();

        //
        // The RunGridViewList maintains a mapping between
        // runGridView entries and CellList.
        //
        List<RunCellWallPathStruct> RunGridViewList = new List<RunCellWallPathStruct>();
        List<WallStruct> WallList = new List <WallStruct>();
        List<PathStruct> PathList = new List <PathStruct>();

        //
        // The CorrectorList stores corrector data that is displayed in the
        // corrector text boxes.
        //
        List<CorrectorStruct> CorrectorList = new List<CorrectorStruct>();

        //
        // These lists are used for the graphs
        //
        FilteredPointList dLFPList, dRFPList;
        FilteredPointList dLFDList, dRFDList;
        FilteredPointList dLDPList, dRDPList;
        FilteredPointList dLDDList, dRDDList;

        FilteredPointList dFVelList, dFErrList;
        FilteredPointList dRVelList, dRErrList;

        int iNumPointsToGraph = 0;

        byte mmPerCell;
        UInt16 countsPerCell;
        UInt16 countsPerUTurn;
        double SensorDistWhenCentered;

        // array of logged data
        byte[] logData;

        double convCountsTomm;
        double convCountsToDeg;

        // current cell location
        byte cellLoc;

        const int NUM_POINTS = 500;

		// these are used for the graphs
        GraphPane GraphPane1, GraphPane2, GraphPane3, GraphPane4;

        LineItem LFP_line, RFP_line, LFD_line, RFD_line;
        LineItem LDP_line, RDP_line, LDD_line, RDD_line;

        LineItem FVel_line, FErr_line, RVel_line, RErr_line;

        // Cursor used in the panes
        LineObj CursorPane1 = new LineObj();
        LineObj CursorPane2 = new LineObj();
        LineObj CursorPane3 = new LineObj();
        LineObj CursorPane4 = new LineObj();

        public MouseDash()
        {
            // InitializeComponent is a C# runtime function created by MouseDash.Designer.cs
            InitializeComponent();

            iNumPointsToGraph = (int)NumPointsToGraph.Value;

            // Initialize the four graph panes
            InitializeGraphs();

            // Initialize the Sensor DataGridView
            sensDataGridView_Setup();

            // Initialize the Run DataGridView
            runGridView_Setup();

            // Initialize the wall stuff
            vInitializeWall();

            // Initialize the corrector
            vInitializeCorrector();

            // Draw a corrected no-wall for the north wall
//            this.mazeControl.UpdateWalls(0x88, WallFlags.MappedNorth | WallFlags.NorthWall);
//            this.mazeControl.UpdateWalls(0x88, WallFlags.MappedNorth);

            // Draw a corrected wall for the south wall
//            this.mazeControl.UpdateWalls(0x88, WallFlags.MappedSouth);
//            this.mazeControl.UpdateWalls(0x88, WallFlags.MappedSouth | WallFlags.SouthWall);

            // Draw a no-wall for the west wall
//            this.mazeControl.UpdateWalls(0x88, WallFlags.MappedWest);

//            this.mazeControl.SetPathFromCoordinateNibble(new byte[] { 0x0, 0xF0, 0xF8, 0x88 });

            return;
        }

        private void InitializeGraphs()
        {
            GraphPane1 = zedGraphControlPane1.GraphPane;
            GraphPane1.Title.Text = "Front Sensors";
            GraphPane1.XAxis.Title.Text = "Time";
            GraphPane1.YAxis.Title.Text = "Power";
            GraphPane1.Y2Axis.Title.Text = "Distance";

            GraphPane2 = zedGraphControlPane2.GraphPane;
            GraphPane2.Title.Text = "Diagonal Sensors";;
            GraphPane2.XAxis.Title.Text = "Time";
            GraphPane2.YAxis.Title.Text = "Power";
            GraphPane2.Y2Axis.Title.Text = "Distance";

            GraphPane3 = zedGraphControlPane3.GraphPane;
            GraphPane3.Title.Text = "Forward Profiler And Servo";
            GraphPane3.XAxis.Title.Text = "Time";
            GraphPane3.YAxis.Title.Text = "Velocity (m/s)";
            GraphPane3.Y2Axis.Title.Text = "Error (mm)";

            GraphPane4 = zedGraphControlPane4.GraphPane;
            GraphPane4.Title.Text = "Rotation Profiler And Servo";;
            GraphPane4.XAxis.Title.Text = "Time";
            GraphPane4.YAxis.Title.Text = "Velocity (deg/ms)";
            GraphPane4.Y2Axis.Title.Text = "Error (deg)";

			// add lines to specific graph panes
            //
			// turn off the opposite tics so the Y tics don't show up on the Y2 axis
			// turn off the opposite tics so the Y2 tics don't show up on the Y1 axis
			GraphPane1.YAxis.MajorTic.IsOpposite = false;
			GraphPane1.YAxis.MinorTic.IsOpposite = false;
			GraphPane1.Y2Axis.MajorTic.IsOpposite = false;
			GraphPane1.Y2Axis.MinorTic.IsOpposite = false;

			GraphPane2.YAxis.MajorTic.IsOpposite = false;
			GraphPane2.YAxis.MinorTic.IsOpposite = false;
			GraphPane2.Y2Axis.MajorTic.IsOpposite = false;
			GraphPane2.Y2Axis.MinorTic.IsOpposite = false;

			GraphPane3.YAxis.MajorTic.IsOpposite = false;
			GraphPane3.YAxis.MinorTic.IsOpposite = false;
			GraphPane3.Y2Axis.MajorTic.IsOpposite = false;
			GraphPane3.Y2Axis.MinorTic.IsOpposite = false;

			GraphPane4.YAxis.MajorTic.IsOpposite = false;
			GraphPane4.YAxis.MinorTic.IsOpposite = false;
			GraphPane4.Y2Axis.MajorTic.IsOpposite = false;
			GraphPane4.Y2Axis.MinorTic.IsOpposite = false;

			// enable the Y2 axis display
			GraphPane1.Y2Axis.IsVisible = true;
			GraphPane2.Y2Axis.IsVisible = true;
			GraphPane3.Y2Axis.IsVisible = true;
			GraphPane4.Y2Axis.IsVisible = true;

            // setup the cursors
            CursorPane1.Location.Width = 0;
            CursorPane1.Location.Height = GraphPane1.YAxis.Scale.Max + GraphPane1.YAxis.Scale.Min;
            CursorPane1.Location.X = (GraphPane1.XAxis.Scale.Min + GraphPane1.XAxis.Scale.Max) / 2;
            CursorPane1.Location.Y = GraphPane1.YAxis.Scale.Min;

            CursorPane2.Location.Width = 0;
            CursorPane2.Location.Height = GraphPane2.YAxis.Scale.Max + GraphPane2.YAxis.Scale.Min;
            CursorPane2.Location.X = (GraphPane2.XAxis.Scale.Min + GraphPane2.XAxis.Scale.Max) / 2;
            CursorPane2.Location.Y = GraphPane2.YAxis.Scale.Min;

            CursorPane3.Location.Width = 0;
            CursorPane3.Location.Height = GraphPane3.YAxis.Scale.Max + GraphPane3.YAxis.Scale.Min;
            CursorPane3.Location.X = (GraphPane3.XAxis.Scale.Min + GraphPane3.XAxis.Scale.Max) / 2;
            CursorPane3.Location.Y = GraphPane3.YAxis.Scale.Min;

            CursorPane4.Location.Width = 0;
            CursorPane4.Location.Height = GraphPane4.YAxis.Scale.Max + GraphPane4.YAxis.Scale.Min;
            CursorPane4.Location.X = (GraphPane4.XAxis.Scale.Min + GraphPane4.XAxis.Scale.Max) / 2;
            CursorPane4.Location.Y = GraphPane4.YAxis.Scale.Min;

            CursorPane1.Line.Color = Color.Red;
            CursorPane1.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

            CursorPane2.Line.Color = Color.Red;
            CursorPane2.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

            CursorPane3.Line.Color = Color.Red;
            CursorPane3.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

            CursorPane4.Line.Color = Color.Red;
            CursorPane4.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

            // don't draw the line outside the boundaries of the graph
            CursorPane1.IsClippedToChartRect = true;
            CursorPane2.IsClippedToChartRect = true;
            CursorPane3.IsClippedToChartRect = true;
            CursorPane4.IsClippedToChartRect = true;

            CursorPane1.Tag = "Cursor";
            CursorPane2.Tag = "Cursor";
            CursorPane3.Tag = "Cursor";
            CursorPane4.Tag = "Cursor";

            // show it behind the axis but in front of the curves
            CursorPane1.ZOrder = ZOrder.D_BehindAxis;
            CursorPane2.ZOrder = ZOrder.D_BehindAxis;
            CursorPane3.ZOrder = ZOrder.D_BehindAxis;
            CursorPane4.ZOrder = ZOrder.D_BehindAxis;

            GraphPane1.GraphObjList.Add(CursorPane1);
            GraphPane2.GraphObjList.Add(CursorPane2);
            GraphPane3.GraphObjList.Add(CursorPane3);
            GraphPane4.GraphObjList.Add(CursorPane4);

            return;
        }

        private void AppendStatusText(string text, Color text_color)
        {
            StatusBox.SelectionColor = text_color;
            StatusBox.AppendText(text);
            StatusBox.ScrollToCaret();

            return;
        }

        // modify this to write out interesting data to a file: sFileDebug
        private void vDebugAppendStatusText(string text, Color text_color)
        {
            if (bOutputDebugInfo)
            {
                //                AppendStatusText(text, text_color);

                var outLine = new StringBuilder();
                if (text.StartsWith("LFE:")
                    || text.StartsWith("RFE:")
                    || text.StartsWith("C:")
                    || text.StartsWith("LC:"))
                {
                    outLine.Append(text + " ");
                }

                if (text.StartsWith("NPAA:"))
                {
                    outLine.AppendLine(text);
                }

                File.AppendAllText(sFileDebug, outLine.ToString());
            }

            return;
        }

        // On the General tab show Mouse State
        private void displayMouseState(byte CurXY, byte Orient, string sMode)
        {
            MouseStateCurXY.Text = CurXY.ToString("X");

			if (Orient < (byte)LMPylOrient.LMPylOrientEnd)
			{
				MouseStateOrient.Text = ms.sOrient[Orient];
			}
			else
			{
				MouseStateOrient.Text = "Unknown";
			}

            MouseStateMode.Text = sMode;

            if (sMode == "Learn")
            {
                MouseStateMode.BackColor = System.Drawing.Color.PowderBlue;
                mazeControl.PathColor = System.Drawing.Color.PowderBlue;
            }
            else if (sMode == "Start to Center")
            {
                MouseStateMode.BackColor = System.Drawing.Color.LightGreen;
                mazeControl.PathColor = System.Drawing.Color.LightGreen;
            }
            else if (sMode == "Center to Start")
            {
                MouseStateMode.BackColor = System.Drawing.Color.Khaki;
                mazeControl.PathColor = System.Drawing.Color.Khaki;
            }
			else
            {
                MouseStateMode.BackColor = System.Drawing.Color.Red;
                mazeControl.PathColor = System.Drawing.Color.Red;
			}

            return;
        }

        // On the Mouse State section, show Motion State
        private void displayMotionState(byte MotionState, byte DiagState, UInt32 SysFlags)
        {
            textBoxMotionState.Text = MotionState.ToString("X2");
            textBoxDiagState.Text = DiagState.ToString("X2");
            textBoxSysFlags.Text = SysFlags.ToString("X8");

            return;
        }

        // On the General tab, Mouse state section show data received from the device
        private void displayCorrection(string sOffset, string sOffsetDot, string sAngle, string sAngleDot)
        {
			textBoxOffset.Text = sOffset;
            textBoxOffsetDot.Text = sOffsetDot;

            textBoxAngle.Text = sAngle;
            textBoxAngleDot.Text = sAngleDot;

            return;
        }

        // On the General tab, Mouse state section show data received from the device
        private void displayVbat(UInt16 uwVbat)
		{
			textBoxVbat.Text = uwVbat.ToString("D");

            return;
        }

        private void displayProgress(int i)
        {
            progressTextBox.Text = i.ToString("D");

            return;
        }

        // Show Index
        private void displayIndex(int Index)
		{
            textBoxIndex.Text = Index.ToString("D");

            return;
        }

        private void displayGridIndex(int Index)
        {
            textBoxGridIndex.Text = Index.ToString("D");

            return;
        }

        private void updateWallState(byte Cell, byte Walls)
    	{
            this.mazeControl.UpdateWallsWithRefresh(Cell, (MouseControls.WallFlags)Walls);

            return;
		}

        private void updatePath(byte [] path, byte bLength)
    	{
	        this.mazeControl.SetPathFromCoordinateNibble(path, bLength);

			return;
		}

        // Update all graphs
        //
        // if we want to smoothly scroll, then do:
//            Scale xScale34 = GraphPane2.XAxis.Scale;
//            xScale34.Max = ulTime + XAXIS_SCROLL_THRESHOLD;
//            xScale34.Min = xScale34.Max - XAXIS_SCROLL;
//
//      // another way of smooth scrolling is:
//            Scale xScale12 = GraphPane1.XAxis.Scale;
//            xScale12.Max = ulTime ;
//            xScale12.Min = xScale12.Max - XAXIS_SCROLL;
//       if we want to jump at the end and then fill in (easier to read)
//       then do:
//            Scale xScale34 = GraphPane2.XAxis.Scale;
//            if (ulTime > xScale34.Max)
//            {
//                xScale34.Max = ulTime + XAXIS_SCROLL_THRESHOLD;
//                xScale34.Min = xScale34.Max - XAXIS_SCROLL;
//            }

        private void refreshGraphs()
        {
			// Scroll the xaxis for all four panes
			// show the last XAXIS_SCROLL points
#if SCROLL_GRAPHS
            Scale xScale12 = GraphPane1.XAxis.Scale;
            if (ulTime > xScale12.Max)
            {
                xScale12.Max = ulTime + XAXIS_SCROLL_THRESHOLD;
                xScale12.Min = xScale12.Max - XAXIS_SCROLL;
            }

            Scale xScale34 = GraphPane2.XAxis.Scale;
            if (ulTime > xScale34.Max)
            {
                xScale34.Max = ulTime + XAXIS_SCROLL_THRESHOLD;
                xScale34.Min = xScale34.Max - XAXIS_SCROLL;
            }

            zedGraphControlChannel12.AxisChange();
            zedGraphControlChannel12.Invalidate();
            zedGraphControlChannel34.AxisChange();
            zedGraphControlChannel34.Invalidate();
#endif

            return;
        }

        private void clearSTATUSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StatusBox.Text = "";

            return;
        }

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            vDebugAppendStatusText("\r\nfileToolStripMenuItem1_Click", Color.Green);

            return;
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            bOutputDebugInfo = !bOutputDebugInfo;

            AppendStatusText("\r\nDebug Info: " + bOutputDebugInfo.ToString(), Color.Red);

            if (File.Exists(sFileDebug))
            {
                File.Delete(sFileDebug);
            }

            return;
        }

        private void loadLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openLogFileDialog.ShowDialog() == DialogResult.OK)
            {
                string sFile = openLogFileDialog.FileName;

                vLoadFileAndProcess(sFile);
            }

            return;
        }

        private void saveToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void vLoadFileAndProcess(string filename)
        {
            MyStrings ms = new MyStrings();

            try
            {
                // clear out any old data
                clearLogList();

                logData = File.ReadAllBytes(filename);

                vParseLog();

                // now setup the sensor DataGridView table
                sensDataGridView_AddAllData();

#if DUMP_RAW_LOG_DATA
                vDebugAppendStatusText("\r\n", Color.BurlyWood);

                int iLength = logData.Length;
                for (int i = 0; i < iLength; i++)
                {
                    vDebugAppendStatusText(logData[i].ToString("x2") + " ", Color.Black);
                    //vDebugAppendStatusText(String.Format("{0:x2} ", logData[i]), Color.Black);
                }
#endif
            }
            catch (IOException Exception)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + Exception.Message);
            }

            return;
        }
        private void buttonLoadFile_Click(object sender, EventArgs e)
        {
            // for MouseDash development/debug purposes, hardwire a button to load a file
            string sFile = @"C:\Depot\Source\ZVII\Data.bin";
// uncomment for ZVI:    string sFile = @"C:\Depot\Source\ZVI\Step.bin";

            bShowMouseDuringLoad = false;

            vLoadFileAndProcess(sFile);

            return;
        }

        private void vInitializeWall()
        {
            WallStruct tempWall = new WallStruct();

            // In the start square, draw a wall for the east wall and no north wall
            tempWall.ucWalls = (byte) (WallFlags.MappedEast | WallFlags.EastWall | WallFlags.MappedNorth);

            WallList.Add(tempWall);

            mazeControl.UpdateWallsNoRefresh(0x00, (WallFlags)tempWall.ucWalls);

            return;
        }

        private void vInitializeCorrector()
        {
            CorrectorStruct tempCorrector = new CorrectorStruct();

            //
            // When we have no corrector data, we use the first entry
            // of the list.
            //
            tempCorrector.sOffset = "None";
            tempCorrector.sOffsetDot = "None";
            tempCorrector.sAngle = "None";
            tempCorrector.sAngleDot = "None";

            CorrectorList.Add(tempCorrector);

            return;
        }

        private int iUnpackSensor(byte ucSensorData)
        {
            int iTemp;

            if (ucSensorData < 128)
            {
                iTemp = ucSensorData * 16;
            }
            else if (ucSensorData < 192)
            {
                iTemp = (ucSensorData - 128) * 32 + 2048;
            }
            else
            {
                iTemp = (ucSensorData - 192) * 64 + 4096;
            }

            return(iTemp);
        }

        private bool CheckLogDataOk(int cmdIndex, int i, int cmdLength)
        {
            if ((i + cmdLength) > logData.Length)
            {
                AppendStatusText("\r\nvParseLog: short command " + logData[cmdIndex].ToString("x2") + " at " + cmdIndex.ToString("x") + " short by " + ((i + cmdLength) - logData.Length).ToString("d"), ColorError);
                return (false);
            }
            else
            {
                return (true);
            }
        }

    private void vParseLog()
        {
            int iLogLength = logData.Length;

            int logRawDataFormatLength;

            int RowIndex = 0;

            List<ParseDebugStruct> ParseDebugList = new List<ParseDebugStruct>();

            ParseDebugStruct tempParseDebugStruct = new ParseDebugStruct();

            DGVStringStruct tempDGVStringStruct = new DGVStringStruct();

            CorrectorStruct tempCorrector = new CorrectorStruct();

            MotionProfileDataStruct tempMPDStruct = new MotionProfileDataStruct();

            //
            // if there is a row of data with no comments, add the row of data
            // if there is a row with one comment, add the row of data along
            // with the comment.
            // if there is a row with more than one comment, add the row of
            // data along with the comment and then add rows of comments.
            //
            bool bPopulatedData = false;
            bool bPopulatedComment = false;

            bool bOverrideColor = false;

            // show parsing progress
            displayProgress(0);

            // TODO: We are getting partial packets and that is making it hard to debug.
            // TODO: So, for now, hack in something that guarantees we don't get runt packets.
            //
            // For looking at APEC2014 data, use a "-3" below
            //
            for (int i = 0; i < (iLogLength - 0); i++)
            {
                const byte VersionSupported = (byte)LMVersion.LMCurVersion;

                byte ucLogCmd = logData[i];
                int cmdIndex = i;
                int payloadStartIndex = i + 1;
                
                int iLength;

                short swError;

                int iTemp;

                double dTemp, dTemp2;

                string sTemp;

                //
                // Use this to debug by setting the RowIndex comparison to
                // the desired row and then setting a breakpoint on
                // iTemp = i;
                //
                if (RowIndex == 327)
                {
                    iTemp = i;
                }

                tempParseDebugStruct.i = i;
                tempParseDebugStruct.RowIndex = RowIndex;
                tempParseDebugStruct.LogCmd = ms.sCmd[ucLogCmd];


                switch (ucLogCmd)
                {
                    //
                    // handle Print String
                    //
                    case (byte)LMCmd.LMCmdPrintString:
                        // point to the length
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        iLength = logData[i];

                        // point to the string data
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, iLength))
                        {
                            break;
                        }

                        string PrintString = System.Text.Encoding.UTF8.GetString(logData, i, iLength);
                        vDebugAppendStatusText(PrintString, ColorMisc);

                        tempDGVStringStruct.sComment = PrintString;
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        // point past the string data
                        // the "- 1" is needed because the loop has an increment
                        i += iLength - 1;

                        break;

                    //
                    // handle Errors That Were Logged
                    //
                    case (byte)LMCmd.LMCmdError:
                        // point to the payload
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        byte ErrorNum = logData[i];

                        if (ErrorNum < (byte)LMPylError.LMPlyErrorEnd)
                        {
                            vDebugAppendStatusText("\r\nError: " + ms.sError[ErrorNum], ColorError);

                            tempDGVStringStruct.sComment = "Error: " + ms.sError[ErrorNum];
                            tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                            vAddErrorToRunGridView(ms.sError[ErrorNum], DGVStringList.Count);
                        }
                        else
                        {
                            vDebugAppendStatusText("\r\nError: Unknown " + ErrorNum.ToString("x2"), ColorError);

                            tempDGVStringStruct.sComment = "Error: Unknown " + ErrorNum.ToString("x2");
                            tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                            vAddErrorToRunGridView("Error: Unknown " + ErrorNum.ToString("x2"), DGVStringList.Count);
                        }

                        bPopulatedComment = true;

                        break;

                    //
                    // handle Mark Walls
                    //
                    case (byte)LMCmd.LMCmdMarkedWalls:
                        // point to the payload
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        byte ucWalls = logData[i];
                        string sWalls = "";

                        if ((ucWalls & (byte)LMPylMarked.LMPylMarkedNorthWall) == (byte)LMPylMarked.LMPylMarkedNorthWall)
                        {
                            sWalls += "N ";
                        }

                        if ((ucWalls & (byte)LMPylMarked.LMPylMarkedEastWall) == (byte)LMPylMarked.LMPylMarkedEastWall)
                        {
                            sWalls += "E ";
                        }

                        if ((ucWalls & (byte)LMPylMarked.LMPylMarkedSouthWall) == (byte)LMPylMarked.LMPylMarkedSouthWall)
                        {
                            sWalls += "S ";
                        }

                        if ((ucWalls & (byte)LMPylMarked.LMPylMarkedWestWall) == (byte)LMPylMarked.LMPylMarkedWestWall)
                        {
                            sWalls += "W ";
                        }

                        ucWalls |= (byte)(WallFlags.MappedNorth | WallFlags.MappedEast | WallFlags.MappedSouth | WallFlags.MappedWest);

                        updateWallState(cellLoc, ucWalls);

                        vAddToWallsList(ucWalls);

                        vDebugAppendStatusText("\r\nMark Walls: " + cellLoc.ToString("x2") + ", " + sWalls + ucWalls.ToString("x2"), ColorMaze);

                        tempDGVStringStruct.sComment = "Mark Walls: " + cellLoc.ToString("x2") + ", " + sWalls + ucWalls.ToString("x2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMaze;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle Update Cell Location
                    //
                    case (byte)LMCmd.LMCmdCellLoc:
                        // point to the payload
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        cellLoc = logData[i];

                        if (bShowMouseDuringLoad)
                        {
                            this.mazeControl.DrawMouse(cellLoc);
                        }

                        vDebugAppendStatusText("\r\nCell Location: " + cellLoc.ToString("x2"), ColorMotion);

                        tempDGVStringStruct.sComment = "Cell Location: " + cellLoc.ToString("x2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMotion;

                        bPopulatedComment = true;

                        vAddCellToRunGridView(cellLoc, DGVStringList.Count);

                        break;

                    //
                    // handle Mouse Orientation output
                    //
                    case (byte)LMCmd.LMCmdOrient:
                        // point to the payload
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        byte ucOrient = logData[i];

                        if (ucOrient < (byte)LMPylOrient.LMPylOrientEnd)
                        {
                            vDebugAppendStatusText("\r\nOrient: " + ms.sOrient[ucOrient], ColorMotion);

                            tempDGVStringStruct.sComment = "Orient: " + ms.sOrient[ucOrient];
                            tempDGVStringStruct.eCommentColor = MyColors.eColorMotion;
                        }
                        else
                        {
                            vDebugAppendStatusText("\r\nOrient: Unknown " + ucOrient.ToString("x2"), ColorError);

                            tempDGVStringStruct.sComment = "Orient: Unknown " + ucOrient.ToString("x2");
                            tempDGVStringStruct.eCommentColor = MyColors.eColorError;
                        }

                        bPopulatedComment = true;

                        vUpdateOrient((Orient)ucOrient);

                        break;

                    //
                    // handle Abort Path
                    //
                    case (byte)LMCmd.LMCmdAbortPath:
                        vDebugAppendStatusText("\r\nAbort Path: " + cellLoc.ToString("x2"), ColorMaze);

                        tempDGVStringStruct.sComment = "Abort Path: " + cellLoc.ToString("x2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMaze;

                        bPopulatedComment = true;

                        vAddAbortPathToRunGridView("Abort", DGVStringList.Count);

                        break;

                    case (byte)LMCmd.LMCmdAbortPathFrontWall:
                        vDebugAppendStatusText("\r\nAbort Path Front Wall: " + cellLoc.ToString("x2"), ColorMaze);

                        tempDGVStringStruct.sComment = "Abort Path Front Wall: " + cellLoc.ToString("x2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMaze;

                        bPopulatedComment = true;

                        vAddAbortPathToRunGridView("AbrtFW", DGVStringList.Count);

                        break;

                    case (byte)LMCmd.LMCmdAbortPathSideWall:
                        vDebugAppendStatusText("\r\nAbort Path Side Wall: " + cellLoc.ToString("x2"), ColorMaze);

                        tempDGVStringStruct.sComment = "Abort Path Side Wall: " + cellLoc.ToString("x2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMaze;

                        bPopulatedComment = true;

                        vAddAbortPathToRunGridView("AbrtSW", DGVStringList.Count);

                        break;

                    //
                    // handle UTurn Error Logging
                    //
                    case (byte)LMCmd.LMCmdUTurn:
                        // point to the payload
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        {
                            sbyte sbTemp = (sbyte)logData[i];

                            swError = (short)sbTemp;

//                            dTemp = Convert.ToDouble(BitConverter.ToInt16(logData, i)) / 4.0;
                        }

                        vDebugAppendStatusText("\r\nUTurn Correction: " + (Convert.ToDouble(swError) / 4).ToString("F2"), ColorMotion);

                        tempDGVStringStruct.sComment = "UTurn Correction: " + (Convert.ToDouble(swError) / 4).ToString("F2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMotion;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle getting the conversion factors
                    //
                    case (byte)LMCmd.LMCmdConvFactors:
                        // point to the payload
                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        byte version = logData[i];

                        if (version != VersionSupported)
                        {
                            AppendStatusText("\r\nMouseDash supports version: " + VersionSupported + " but Data is version: " + version, ColorError);

                            tempDGVStringStruct.sComment = "MouseDash supports version: " + VersionSupported + " but Data is version: " + version;
                            tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                            bPopulatedComment = true;

                            // set the length to iLogLength so that the loop breaks out
                            i = iLogLength;

                            break;
                        }
                        else
                        {
                            vDebugAppendStatusText("\r\nData and MouseDash are version: " + version, ColorMisc);

                            tempDGVStringStruct.sComment = "Data and MouseDash are version: " + version;
                            tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;
                        }

                        i++;

                        if (!CheckLogDataOk(cmdIndex, i, 6))
                        {
                            break;
                        }

                        mmPerCell = logData[i];
                        vDebugAppendStatusText("\r\nmmPerCell = " + mmPerCell, ColorMisc);

                        tempDGVStringStruct.sComment += "\nmmPerCell = " + mmPerCell;
                        // use previous color in case an error occurred

                        i++;
                        countsPerCell = BitConverter.ToUInt16(logData, i);
                        vDebugAppendStatusText("\r\ncountsPerCell = " + countsPerCell, ColorMisc);

                        tempDGVStringStruct.sComment += "\ncountsPerCell = " + countsPerCell;
                        // use previous color in case an error occurred

                        i += 2;
                        countsPerUTurn = BitConverter.ToUInt16(logData, i);
                        vDebugAppendStatusText("\r\ncountsPerUTurn = " + countsPerUTurn, ColorMisc);

                        tempDGVStringStruct.sComment += "\ncountsPerUTurn = " + countsPerUTurn;
                        // use previous color in case an error occurred

                        i += 2;
                        SensorDistWhenCentered = BitConverter.ToInt16(logData, i) / 256.0;
                        vDebugAppendStatusText("\r\nSensorDistWhenCentered = " + SensorDistWhenCentered, ColorMisc);

                        tempDGVStringStruct.sComment += "\nSensorDistWhenCentered = " + SensorDistWhenCentered;
                        // use previous color in case an error occurred

                        bPopulatedComment = true;

                        convCountsTomm = (double)mmPerCell / (double)countsPerCell;
                        convCountsToDeg = 180.0 / countsPerUTurn;

                        i++;

                        break;

                    //
                    // handle the profiler and servo only data
                    //
                    case (byte)LMCmd.LMCmdProfServoOnly:
                        //
                        // if we have back to back LMCmdProfServo*, add the
                        // data to the list.
                        //
                        if (bPopulatedData)
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct.sLat = "";
                            tempDGVStringStruct.sLong = "";
                            //
                            // we will be populating data below, so leave
                            // bPopulatedData true.
                            //
                        }

                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 7))
                        {
                            break;
                        }

                        // save sLat and sLong across the new
                        {
                            string sLat, sLong;
                            sLat = tempDGVStringStruct.sLat;
                            sLong = tempDGVStringStruct.sLong;

                            tempDGVStringStruct = default(DGVStringStruct);
                            tempMPDStruct = default(MotionProfileDataStruct);

                            tempDGVStringStruct.sLat = sLat;
                            tempDGVStringStruct.sLong = sLong;
                        }

                        tempMPDStruct.dFErr = ((sbyte)logData[i]) / 4.0;
                        tempDGVStringStruct.sFErr = tempMPDStruct.dFErr.ToString("##0.0");

                        i++;

                        tempMPDStruct.dRPos = BitConverter.ToUInt16(logData, i) / 128.0;
                        tempDGVStringStruct.sRPos = tempMPDStruct.dRPos.ToString("##0.0");

                        i += 2;

                        // get FVel's fractional part
                        dTemp = logData[i] / 256.0;

                        i++;

                        // get RErr's fractional part
                        dTemp2 = ((sbyte)logData[i]) / 256.0;

                        i++;

                        // add in FVel's integer part
                        dTemp += logData[i] & 0x7;

                        tempMPDStruct.dFVel = dTemp;
                        tempDGVStringStruct.sFVel = tempMPDStruct.dFVel.ToString("#0.00");

                        // add in RErr's interger part
                        dTemp2 += (sbyte)logData[i] >> 3;

                        tempMPDStruct.dRErr = dTemp2;
                        tempDGVStringStruct.sRErr = tempMPDStruct.dRErr.ToString("##0.0");

                        i++;

                        tempMPDStruct.dVBat = (logData[i] / 32.0);

                        // for this command write a "ms" in the mm column
                        tempDGVStringStruct.smm = "ms";

                        //
                        // For this command, since we don't have any sensor data,
                        // use the last value, if we have some data already.
                        //
                        // For the zero case, we already zeroed these out when
                        // we did the default(MotionProfileDataStruct) statement
                        // above.
                        //
                        if (MPDList.Count != 0)
                        {
                            tempMPDStruct.dmm = MPDList.Last().dmm;
                            tempMPDStruct.dLFP = MPDList.Last().dLFP;
                            tempMPDStruct.dLDP = MPDList.Last().dLDP;
                            tempMPDStruct.dRDP = MPDList.Last().dRDP;
                            tempMPDStruct.dRFP = MPDList.Last().dRFP;
                            tempMPDStruct.dLFD = MPDList.Last().dLFD;
                            tempMPDStruct.dLDD = MPDList.Last().dLDD;
                            tempMPDStruct.dRDD = MPDList.Last().dRDD;
                            tempMPDStruct.dRFD = MPDList.Last().dRFD;
                        }

                        //                        vDebugAppendStatusText("\r\nLMCmdProfServoOnly = " + RowIndex, ColorMisc);

                        MPDList.Add(tempMPDStruct);

                        bPopulatedData = true;

                        tempDGVStringStruct.iDataIndex = RowIndex;

                        RowIndex++;

                        break;

                    //
                    // handle the profiler, servo and sensor data
                    //
                    case (byte)LMCmd.LMCmdProfServoSensors:
                        //
                        // if we have back to back LMCmdProfServo*, add the
                        // data to the list.
                        //
                        if (bPopulatedData)
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct.sLat = "";
                            tempDGVStringStruct.sLong = "";

                            //
                            // we will be populating data below, so leave
                            // bPopulatedData true.
                            //
                        }

                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 17))
                        {
                            break;
                        }

                        // save sLat across the new
                        {
                            string sLat, sLong;
                            sLat = tempDGVStringStruct.sLat;
                            sLong = tempDGVStringStruct.sLong;

                            tempDGVStringStruct = default(DGVStringStruct);
                            tempMPDStruct = default(MotionProfileDataStruct);

                            tempDGVStringStruct.sLat = sLat;
                            tempDGVStringStruct.sLong = sLong;
                        }

                        tempMPDStruct.dFErr = ((sbyte)logData[i]) / 4.0;
                        tempDGVStringStruct.sFErr = tempMPDStruct.dFErr.ToString("##0.0");

                        i++;

                        tempMPDStruct.dRPos = BitConverter.ToUInt16(logData, i) / 128.0;
                        tempDGVStringStruct.sRPos = tempMPDStruct.dRPos.ToString("##0.0");

                        i += 2;

                        // get FVel's fractional part
                        dTemp = logData[i] / 256.0;

                        i++;

                        // get RErr's fractional part
                        dTemp2 = ((sbyte)logData[i]) / 256.0;

                        i++;

                        // add in FVel's integer part
                        dTemp += logData[i] & 0x7;

                        tempMPDStruct.dFVel = dTemp;
                        tempDGVStringStruct.sFVel = tempMPDStruct.dFVel.ToString("#0.00");

                        // add in RErr's interger part
                        dTemp2 += (sbyte)logData[i] >> 3;

                        tempMPDStruct.dRErr = dTemp2;
                        tempDGVStringStruct.sRErr = tempMPDStruct.dRErr.ToString("##0.0");

                        i++;

                        tempMPDStruct.dVBat = (logData[i] / 32.0);

                        i++;
                        tempMPDStruct.dmm = logData[i]; ;
                        tempDGVStringStruct.smm = tempMPDStruct.dmm.ToString("##0");

                        // Get Left Front sensor power data
                        i++;
                        tempMPDStruct.dLFP = iUnpackSensor(logData[i]);
                        tempDGVStringStruct.sLFP = tempMPDStruct.dLFP.ToString("###0");

                        // Get Left Diagonal sensor power data
                        i++;
                        tempMPDStruct.dLDP = iUnpackSensor(logData[i]);
                        tempDGVStringStruct.sLDP = tempMPDStruct.dLDP.ToString("###0");

                        // Get Right Diagonal sensor power data
                        i++;
                        tempMPDStruct.dRDP = iUnpackSensor(logData[i]);
                        tempDGVStringStruct.sRDP = tempMPDStruct.dRDP.ToString("###0");

                        // Get Right Front sensor power data
                        i++;
                        tempMPDStruct.dRFP = iUnpackSensor(logData[i]);
                        tempDGVStringStruct.sRFP = tempMPDStruct.dRFP.ToString("###0");

                        {
                            int iLFD, iLDD, iRDD, iRFD;

                            // Get Left Front sensor distance data
                            i++;
                            iLFD = logData[i] << 2;

                            // Get Left Diagonal sensor distance data
                            i++;
                            iLDD = logData[i] << 2;

                            // Get Right Diagonal sensor distance data
                            i++;
                            iRDD = logData[i] << 2;

                            // Get Right Front sensor distance data
                            i++;
                            iRFD = logData[i] << 2;

                            // Get distance fractions for all the sensors
                            i++;
                            iTemp = logData[i];

                            // add in the fraction distance for every sensor
                            iLFD |= iTemp & 0x03;

                            iLDD |= (iTemp >> 2) & 0x03;

                            iRDD |= (iTemp >> 4) & 0x03;

                            iRFD |= (iTemp >> 6) & 0x03;

                            tempMPDStruct.dLFD = iLFD / 4.0;
                            tempDGVStringStruct.sLFD = tempMPDStruct.dLFD.ToString("##0.0");

                            tempMPDStruct.dLDD = iLDD / 4.0;
                            tempDGVStringStruct.sLDD = tempMPDStruct.dLDD.ToString("##0.0");

                            tempMPDStruct.dRDD = iRDD / 4.0;
                            tempDGVStringStruct.sRDD = tempMPDStruct.dRDD.ToString("##0.0");

                            tempMPDStruct.dRFD = iRFD / 4.0;
                            tempDGVStringStruct.sRFD = tempMPDStruct.dRFD.ToString("##0.0");
                        }
                        //                        vDebugAppendStatusText("\r\nLMCmdProfServoSensors = " + RowIndex, ColorMisc);

                        MPDList.Add(tempMPDStruct);

                        bPopulatedData = true;

                        tempDGVStringStruct.iDataIndex = RowIndex;
                        //                        tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();

                        RowIndex++;

                        break;

                    //
                    // handle the lateral correction data
                    //
                    case (byte)LMCmd.LMCmdCorrector:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 4))
                        {
                            break;
                        }

                        iTemp = BitConverter.ToInt16(logData, i);
                        dTemp = iTemp / 256.0;
                        //                        vDebugAppendStatusText("\r\nCorr Offset: " + iTemp.ToString("d6"), ColorLatCorr);

                        i += 2;

                        //tempCorrector.sOffset = iTemp.ToString();
                        tempCorrector.sOffset = dTemp.ToString("#0.00");

                        // the first entry may not be a number so, don't compute a derivative for it
                        if (CorrectorList.Count != 1)
                        {
                            dTemp -= Convert.ToDouble(CorrectorList.Last().sOffset);
                        }

                        // tempCorrector.sOffsetDot = iTemp.ToString();
                        tempCorrector.sOffsetDot = dTemp.ToString("#0.00");

                        iTemp = BitConverter.ToInt16(logData, i);
                        dTemp = iTemp / 1024.0;
                        // vDebugAppendStatusText("\r\nCorr Angle: " + (double)iTemp * convCountsToDeg, ColorLatCorr);

                        // point at the last byte of the payload because the loop has an increment
                        i += 1;

                        tempCorrector.sAngle = dTemp.ToString("#0.00");

                        // the first entry may not be a number so, don't compute a derivative for it
                        if (CorrectorList.Count != 1)
                        {
                            dTemp -= Convert.ToDouble(CorrectorList.Last().sAngle);
                        }

                        tempCorrector.sAngleDot = dTemp.ToString("#0.00");

                        CorrectorList.Add(tempCorrector);

                        // commented out since I added it for debugging only
                        // tempDGVStringStruct.sLat = "P";
                        // bPopulatedComment = true;

                        break;

                    //
                    // handle the lateral correction data
                    //
                    case (byte)LMCmd.LMCmdCorrector2:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 8))
                        {
                            break;
                        }

                        iTemp = BitConverter.ToInt32(logData, i);
                        dTemp = iTemp / 65536.0;
                        //                        vDebugAppendStatusText("\r\nCorr Offset: " + iTemp.ToString("d6"), ColorLatCorr);

                        i += 4;

                        //tempCorrector.sOffset = iTemp.ToString();
                        tempCorrector.sOffset = dTemp.ToString("#0.00");

                        // the first entry may not be a number so, don't compute a derivative for it
                        if (CorrectorList.Count != 1)
                        {
                            dTemp -= Convert.ToDouble(CorrectorList.Last().sOffset);
                        }

                        // tempCorrector.sOffsetDot = iTemp.ToString();
                        tempCorrector.sOffsetDot = dTemp.ToString("#0.00");

                        iTemp = BitConverter.ToInt32(logData, i);
                        dTemp = iTemp / 65536.0;
                        //                        vDebugAppendStatusText("\r\nCorr Angle: " + (double)iTemp * convCountsToDeg, ColorLatCorr);

                        // point at the last byte of the payload because the loop has an increment
                        i += 3;

                        tempCorrector.sAngle = dTemp.ToString("#0.00");

                        // the first entry may not be a number so, don't compute a derivative for it
                        if (CorrectorList.Count != 1)
                        {
                            dTemp -= Convert.ToDouble(CorrectorList.Last().sAngle);
                        }

                        tempCorrector.sAngleDot = dTemp.ToString("#0.00");

                        CorrectorList.Add(tempCorrector);

                        // commented out since I added it for debugging only
                        // tempDGVStringStruct.sLat = "P";
                        // bPopulatedComment = true;

                        break;

                    //
                    // handle the run start info
                    //
                    case (byte)LMCmd.LMCmdRunStart:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 10))
                        {
                            break;
                        }

                        RunStruct NewRun = new RunStruct();

                        dTemp = NewRun.dFwdAccel = (double)logData[i] / 8.0;

                        vDebugAppendStatusText("\r\nFwd Accel:     " + dTemp.ToString("0.00"), ColorMisc);

                        tempDGVStringStruct.sComment = "Fwd Accel:     " + dTemp.ToString("0.00");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        i++;

                        dTemp = NewRun.dOrthoMaxVel = (double)logData[i] / 32.0;

                        vDebugAppendStatusText("\r\nOrtho Max Vel: " + dTemp.ToString("0.00"), ColorMisc);

                        tempDGVStringStruct.sComment += "\nOrtho Max Vel: " + dTemp.ToString("0.00");
                        // use old color

                        i++;

                        dTemp = NewRun.dDiagMaxVel = (double)logData[i] / 32.0;

                        vDebugAppendStatusText("\r\nDiag Max Vel:  " + dTemp.ToString("0.00"), ColorMisc);

                        tempDGVStringStruct.sComment += "\nDiag Max Vel:  " + dTemp.ToString("0.00");
                        // use old color

                        i++;

                        dTemp = NewRun.dLatAccel = (double)logData[i] / 8.0;

                        vDebugAppendStatusText("\r\nLat Accel:     " + dTemp.ToString("0.00"), ColorMisc);

                        tempDGVStringStruct.sComment += "\nLat Accel:     " + dTemp.ToString("0.00");
                        // use old color

                        i++;

                        iTemp = logData[i];

                        NewRun.iRunNum = iTemp;

                        vDebugAppendStatusText("\r\nRun #:         " + iTemp, ColorMisc);

                        tempDGVStringStruct.sComment += "\nRun #:         " + iTemp;
                        // use old color for the comment

                        i++;

                        string sRunMode;

                        switch (logData[i])
                        {
                            case (byte)LMPylRunMode.LMPylRunModeLearn:
                                sRunMode = "Learn";
                                break;

                            case (byte)LMPylRunMode.LMPylRunStartToCenter:
                                sRunMode = "Start to Center";
                                break;

                            case (byte)LMPylRunMode.LMPylRunCenterToStart:
                                sRunMode = "Center to Start";
                                break;

                            default:
                                sRunMode = "Unknown";
                                break;
                        }

                        if (sRunMode != "Unknown")
                        {
                            vDebugAppendStatusText("\r\nRun Mode: " + sRunMode, ColorMisc);

                            tempDGVStringStruct.sComment += "\nRun Mode:      " + sRunMode;
                            // use old color
                        }
                        else
                        {
                            vDebugAppendStatusText("\r\nRun Mode: " + sRunMode + logData[i].ToString("x2"), ColorError);

                            tempDGVStringStruct.sComment += "\nRun Mode:      " + sRunMode + logData[i].ToString("x2");
                            // use old color
                        }

                        NewRun.sRunMode = sRunMode;

                        i++;

                        NewRun.ulRunStartTime = BitConverter.ToUInt32(logData, i); ;

                        i += 3;

                        tempDGVStringStruct.sComment += "\nRun Start Time(ms):  " + NewRun.ulRunStartTime.ToString("d");
                        // use old color

                        bPopulatedComment = true;

                        vRunStarted(NewRun);

                        break;

                    //
                    // handle the run end success info
                    //
                    case (byte)LMCmd.LMCmdRunEndSuccess:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 4))
                        {
                            break;
                        }

                        iTemp = (int)BitConverter.ToUInt32(logData, i);

                        vDebugAppendStatusText("\r\nRun Success Time(ms): " + iTemp.ToString("d"), ColorMisc);

                        tempDGVStringStruct.sComment = "Run Success Time(ms): " + iTemp.ToString("d");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        // point at the last byte of the payload because the loop has an increment
                        i += 3;

                        vRunFinished(true, (uint)iTemp);

                        break;

                    //
                    // handle the run end fail info
                    //
                    case (byte)LMCmd.LMCmdRunEndFail:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 4))
                        {
                            break;
                        }

                        iTemp = (int)BitConverter.ToUInt32(logData, i);

                        vDebugAppendStatusText("\r\nRun Fail Time(ms): " + iTemp.ToString("d"), ColorMisc);

                        tempDGVStringStruct.sComment = "Run Fail Time(ms): " + iTemp.ToString("d");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        // point at the last byte of the payload because the loop has an increment
                        i += 3;

                        vRunFinished(false, (uint)iTemp);

                        break;

                    //
                    // handle path Logging
                    //
                    case (byte)LMCmd.LMCmdPath:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        iTemp = logData[i];

                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, iTemp))
                        {
                            break;
                        }

                        PathStruct tempPathStruct = new PathStruct();

                        tempPathStruct.path = logData.Skip(i).Take(iTemp).ToArray();

                        if (bShowMouseDuringLoad)
                        {
                            updatePath(tempPathStruct.path, (byte)iTemp);
                        }

                        i += iTemp - 1;

                        vAddToPathList(tempPathStruct);

                        vDebugAppendStatusText("\r\nNew Path length: " + iTemp.ToString("d"), ColorMisc);

                        tempDGVStringStruct.sComment = "New path length:   " + iTemp.ToString("d");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle move command Logging
                    //
                    case (byte)LMCmd.LMCmdMove:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        iTemp = logData[i];

                        // Extract the move name from the enum
                        sTemp = Enum.GetName(typeof(LMPylMove), iTemp);

                        if (sTemp != null)
                        {
                            bOverrideColor = false;
                            sTemp = sTemp.Remove(0, "LMPyl".Length);
                        }
                        else
                        {
                            bOverrideColor = true;
                            sTemp = "0x" + i.ToString("x6") + ": 0x" + iTemp.ToString("x2");
                        }

                        vDebugAppendStatusText("\r\nMove: " + sTemp, ColorMotion);

                        tempDGVStringStruct.sComment = "Move: " + sTemp;

                        if (!bOverrideColor)
                        {
                            if (sTemp.Contains("Fwd"))
                            {
                                tempDGVStringStruct.eCommentColor = MyColors.eColorMotion;
                            }
                            else
                            {
                                tempDGVStringStruct.eCommentColor = MyColors.eColorMotionTurn;
                            }
                        }
                        else
                        {
                            tempDGVStringStruct.eCommentColor = MyColors.eColorError;
                        }

                        bPopulatedComment = true;

                        vAddMoveToRunGridView(sTemp, DGVStringList.Count);

                        break;

                    //
                    // handle Sensor Drive Level Change changed to low Logging
                    //
                    case (byte)LMCmd.LMCmdSensDriveLow:
                        vDebugAppendStatusText("\r\nFwd Sensor Drive Low", ColorMisc);

                        tempDGVStringStruct.sComment = "Fwd Sensor Drive Low";
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle Sensor Drive Level Change changed to high Logging
                    //
                    case (byte)LMCmd.LMCmdSensDriveHigh:
                        vDebugAppendStatusText("\r\nFwd Sensor Drive High", ColorMisc);

                        tempDGVStringStruct.sComment = "Fwd Sensor Drive High";
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle NOP command
                    //
                    case (byte)LMCmd.LMCmdNOP:
                        vDebugAppendStatusText("\r\nNOP", ColorMisc);

                        tempDGVStringStruct.sComment = "NOP";
                        tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle DiagCorr Logging
                    //
                    case (byte)LMCmd.LMCmdDiagCorrInitLeft:
                    case (byte)LMCmd.LMCmdDiagCorrInitRight:
                    case (byte)LMCmd.LMCmdDiagLatCorrHold:
                    case (byte)LMCmd.LMCmdDiagLatCorr2Sample1:
                    case (byte)LMCmd.LMCmdDiagLatCorr2Sample2Do:
                    case (byte)LMCmd.LMCmdDiagMoveUpdate:
                    case (byte)LMCmd.LMCmdDiagLatCorr1Sample1:
                    case (byte)LMCmd.LMCmdDiagLatCorr1Sample2Do:
                    case (byte)LMCmd.LMCmdDiagLatCorrDoLeft:
                    case (byte)LMCmd.LMCmdDiagLatCorrDoRight:
                        vDebugAppendStatusText("\r\nDiagCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft], ColorDiagCorr);

                        tempDGVStringStruct.sComment = "DiagCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorDiagCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdDiagLongCorrDo:
                            vDebugAppendStatusText("\r\nDiagLongCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft], ColorDiagCorr);

                            tempDGVStringStruct.sComment = "DiagLongCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft];
                            tempDGVStringStruct.eCommentColor = MyColors.eColorDiagCorr;

                            bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdDiagLatCorrDoLeftOffset:
                    case (byte)LMCmd.LMCmdDiagLatCorrDoRightOffset:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        dTemp = logData[i] / 4.0;

                        vDebugAppendStatusText("\r\nDiagLatCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft] + " " + dTemp.ToString("##0.0"), ColorDiagCorr);

                        tempDGVStringStruct.sComment = "DiagLatCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft] + " " + dTemp.ToString("##0.0");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorDiagCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdDiagLongCorrCorrection:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 2))
                        {
                            break;
                        }

                        dTemp = Convert.ToDouble(BitConverter.ToInt16(logData, i)) / 4.0;

                        vDebugAppendStatusText("\r\nDiagLongCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft] + " " + dTemp.ToString("F2"), ColorDiagCorr);

                        tempDGVStringStruct.sComment = "DiagLongCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft] + " " + dTemp.ToString("F2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorDiagCorr;

                        bPopulatedComment = true;

                        // point at the last byte of the payload because the loop has an increment
                        i++;

                        break;

                    case (byte)LMCmd.LMCmdDiagCorrSkip:
                        vDebugAppendStatusText("\r\nDiagCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft], ColorLatCorr);

                        tempDGVStringStruct.sComment = "DiagCorr: " + ms.sDiagCorr[ucLogCmd - (byte)LMCmd.LMCmdDiagCorrInitLeft];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;


                    //
                    // handle LongCorr Logging
                    //
                    case (byte)LMCmd.LMCmdLongCorrEnable:
                    case (byte)LMCmd.LMCmdLongCorrSkip:
                    case (byte)LMCmd.LMCmdLongCorrLeftNoEdges:
                    case (byte)LMCmd.LMCmdLongCorrLeftFallingEdge:
                    case (byte)LMCmd.LMCmdLongCorrLeftRisingEdge:
                    case (byte)LMCmd.LMCmdLongCorrLeftBothEdges:
                    case (byte)LMCmd.LMCmdLongCorrRightNoEdges:
                    case (byte)LMCmd.LMCmdLongCorrRightFallingEdge:
                    case (byte)LMCmd.LMCmdLongCorrRightRisingEdge:
                    case (byte)LMCmd.LMCmdLongCorrRightBothEdges:
                    case (byte)LMCmd.LMCmdLongCorrAdd10mm:
                    case (byte)LMCmd.LMCmdLongCorrHold:
                    case (byte)LMCmd.LMCmdLongCorrDisable:
                        vDebugAppendStatusText("\r\nLongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable], ColorLongCorr);

                        tempDGVStringStruct.sComment = "LongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLongCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLongCorrDataGather:
                        vDebugAppendStatusText("\r\nLongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable], ColorLongCorr);

                        tempDGVStringStruct.sLong = "DG";

                        //                        tempDGVStringStruct.sComment = "LongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable];
                        //                        tempDGVStringStruct.eCommentColor = MyColors.eColorLongCorr;

                        //                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLongCorrCorrection:
                        //
                        // if we have back to back commands, add the data to the list.
                        //
                        if ((tempDGVStringStruct.sLong != "") && (tempDGVStringStruct.sLong != null))
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct = default(DGVStringStruct);
                        }

                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 2))
                        {
                            break;
                        }

                        dTemp = Convert.ToDouble(BitConverter.ToInt16(logData, i)) / 4.0;

                        vDebugAppendStatusText("\r\nLongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable] + " " + dTemp.ToString("F2"), ColorLongCorr);

                        tempDGVStringStruct.sLong = dTemp.ToString("F2");

                        tempDGVStringStruct.sComment = "LongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable] + " " + dTemp.ToString("F2");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLongCorr;

                        bPopulatedComment = true;

                        // point at the last byte of the payload because the loop has an increment
                        i++;

                        break;

                    case (byte)LMCmd.LMCmdLongCorrError:
                        vDebugAppendStatusText("\r\nLongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable], ColorError);

                        tempDGVStringStruct.sComment = "LongCorr: " + ms.sLongCorr[ucLogCmd - (byte)LMCmd.LMCmdLongCorrEnable];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle LatCorr Logging
                    //
                    case (byte)LMCmd.LMCmdLatCorrInitReadWalls1:
                    case (byte)LMCmd.LMCmdLatCorrDoReadWalls2:
                    case (byte)LMCmd.LMCmdLatCorrDoReadWalls3NM_NoBT:
                    case (byte)LMCmd.LMCmdLatCorrDoReadWalls3NM_WithBT:
                        //
                        // if we have back to back commands, add the data to the list.
                        //
                        if ((tempDGVStringStruct.sLat != "") && (tempDGVStringStruct.sLat != null))
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct = default(DGVStringStruct);
                        }

                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1], ColorLatCorr);

                        tempDGVStringStruct.sLat = ms.sLatCorrAbbr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1];

                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;


                    case (byte)LMCmd.LMCmdLatCorrBothWalls:
                    case (byte)LMCmd.LMCmdLatCorrLeftWallOnly:
                    case (byte)LMCmd.LMCmdLatCorrRightWallOnly:
                    case (byte)LMCmd.LMCmdLatCorrPegs:
                    case (byte)LMCmd.LMCmdLatCorrFrontWallDontUsePegs:
                    case (byte)LMCmd.LMCmdLatCorrDo:
                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1], ColorLatCorr);

                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLatCorrDone:
                        //
                        // if we have back to back commands, add the data to the list.
                        //
                        if ((tempDGVStringStruct.sLat != "") && (tempDGVStringStruct.sLat != null))
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct = default(DGVStringStruct);
                        }

                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1], ColorLatCorr);

                        tempDGVStringStruct.sLat = "Done";

                        //                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1];
                        //                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLatCorrCorrection:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        {
                            sbyte sbTemp = (sbyte)logData[i];
                            iTemp = (int)sbTemp;
                        }

                        dTemp = iTemp / 4.0;

                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1] + " " + dTemp.ToString("##0.0"), ColorLatCorr);

                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1] + " " + dTemp.ToString("##0.0");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLatCorrCorrection2:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 2))
                        {
                            break;
                        }

                        {
                            sbyte sbTemp = (sbyte)logData[i];
                            iTemp = (int)sbTemp;
                        }

                        dTemp = iTemp / 4.0;

                        // point to the wall info
                        i++;

                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorr2Walls[logData[i]] + " " + dTemp.ToString("##0.0"), ColorLatCorr);

                        tempDGVStringStruct.sLat = ms.sLatCorr2WallsAbbr[logData[i]] + dTemp.ToString("##0.0");

                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorr2Walls[logData[i]];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle the lateral correction when the mouse is within the deadband
                    //
                    case (byte)LMCmd.LMCmdLatCorrNoCorrDeadBand:
                        //
                        // if we have back to back commands, add the data to the list.
                        //
                        if ((tempDGVStringStruct.sLat != "") && (tempDGVStringStruct.sLat != null))
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct = default(DGVStringStruct);
                        }

                        vDebugAppendStatusText("\r\nLatCorr: Deadband", ColorLatCorr);

                        tempDGVStringStruct.sLat = "DB";

                        //                        tempDGVStringStruct.sComment = "LatCorr: Deadband";
                        //                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLatCorrNotUsed14:
                    case (byte)LMCmd.LMCmdLatCorrError:
                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1], ColorError);

                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrInitReadWalls1];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle new Correction Logging
                    //
                    case (byte)LMCmd.LMCmdLatCorrEnable:
                    case (byte)LMCmd.LMCmdLatCorrDisable:
                    case (byte)LMCmd.LMCmdLatCorrHold:
                    case (byte)LMCmd.LMCmdLatCorrStart:
                    case (byte)LMCmd.LMCmdLatCorrAlreadyEnabled:
                        //
                        // if we have back to back commands, add the data to the list.
                        //
                        if ((tempDGVStringStruct.sLat != "") && (tempDGVStringStruct.sLat != null))
                        {
                            if (tempDGVStringStruct.iDataIndex == 0)
                            {
                                tempDGVStringStruct.iDataIndex = RowIndex;
                            }

                            tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();
                            DGVStringList.Add(tempDGVStringStruct);

                            tempDGVStringStruct = default(DGVStringStruct);
                        }

                        vDebugAppendStatusText("\r\nLatCorr: " + ms.sLatCorrNew[ucLogCmd - (byte)LMCmd.LMCmdLatCorrEnable], ColorLatCorr);

                        tempDGVStringStruct.sLat = ms.sLatCorrNewAbbr[ucLogCmd - (byte)LMCmd.LMCmdLatCorrEnable];

                        //                        tempDGVStringStruct.sComment = "LatCorr: " + ms.sLatCorrNew[ucLogCmd - (byte)LMCmd.LMCmdLatCorrEnable];
                        //                        tempDGVStringStruct.eCommentColor = MyColors.eColorLatCorr;

                        bPopulatedComment = true;

                        break;

                    case (byte)LMCmd.LMCmdLongCorrAlreadyEnabled:
                    case (byte)LMCmd.LMCmdLongLeftDiagSensorPeakIndexIsZero:
                    case (byte)LMCmd.LMCmdLongRightDiagSensorPeakIndexIsZero:
                    case (byte)LMCmd.LMCmdLongCorrDataReset:
                        vDebugAppendStatusText("\r\nLongCorr: " + ms.sLongCorrNew[ucLogCmd - (byte)LMCmd.LMCmdLongCorrAlreadyEnabled], ColorLongCorr);

                        tempDGVStringStruct.sComment = "LongCorr: " + ms.sLongCorrNew[ucLogCmd - (byte)LMCmd.LMCmdLongCorrAlreadyEnabled];
                        tempDGVStringStruct.eCommentColor = MyColors.eColorLongCorr;

                        bPopulatedComment = true;

                        break;

                    //
                    // handle LogData
                    //
                    case (byte)LMCmd.LMCmdLogDataFormat:
                        // get the length of the formatted data
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 2))
                        {
                            break;
                        }
                        
                        logRawDataFormatLength = logData[i];

                        // get the length of the raw data format string
                        i++;

                        iLength = logData[i];

                        // point to the log data format
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, iLength))
                        {
                            break;
                        }

                        logDataFormat = System.Text.Encoding.UTF8.GetString(logData, i, iLength);
                        vDebugAppendStatusText(logDataFormat, ColorMisc);

                        tempDGVStringStruct.sComment = logDataFormat;
                        tempDGVStringStruct.eCommentColor = MyColors.eColorMisc;

                        bPopulatedComment = true;

                        // point past the string data
                        // the "- 1" is needed because the loop has an increment
                        i += iLength - 1;

                        streamWriterLog = File.CreateText(sFileLog);

                        break;

                    case (byte)LMCmd.LMCmdLogData:
                        // point to the data
                        i++;

                        string outString = "";

                        for (int item = 0; item < logDataFormat.Length; item++)
                        {
                            if (outString != "")
                            {
                                outString += ", ";
                            }

                            switch (logDataFormat[item])
                            {
                                case 'B':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 1))
                                    {
                                        break;
                                    }

                                    outString += logData[i].ToString("d3");
                                    i++;
                                    break;

                                case 'b':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 1))
                                    {
                                        break;
                                    }

                                    sbyte temp = (sbyte)logData[i];
                                    outString += temp.ToString("d3");
                                    i++;
                                    break;

                                case 'W':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 2))
                                    {
                                        break;
                                    }

                                    outString += BitConverter.ToUInt16(logData, i);
                                    i += 2;

                                    break;

                                case 'w':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 2))
                                    {
                                        break;
                                    }
                                    outString += BitConverter.ToInt16(logData, i);
                                    i += 2;

                                    break;

                                case 'L':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 4))
                                    {
                                        break;
                                    }

                                    outString += BitConverter.ToUInt32(logData, i);
                                    i += 4;

                                    break;

                                case 'l':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 4))
                                    {
                                        break;
                                    }

                                    outString += BitConverter.ToInt32(logData, i);
                                    i += 4;

                                    break;

                                case 'f':
                                case 'F':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 4))
                                    {
                                        break;
                                    }

                                    float value = BitConverter.ToSingle(logData, i);
                                    outString += value.ToString();
                                    i += 4;

                                    break;

                                case 'd':
                                case 'D':
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 4))
                                    {
                                        break;
                                    }

                                    outString += BitConverter.ToDouble(logData, i);
                                    i += 4;

                                    break;

                                default:
                                    // if there is a runt command at the end, skip past it
                                    if (!CheckLogDataOk(cmdIndex, i, 1))
                                    {
                                        break;
                                    }

                                    AppendStatusText("\r\nLMCmdLogData: Unknown format " + logData[i], ColorError);
                                    break;
                            }
                        }

                        streamWriterLog.WriteLine(outString);

                        // the "- 1" is needed because the loop has an increment
                        i--;

                        break;

                    //
                    // handle the skip portions of flash command
                    //
                    case (byte)LMCmd.LMCmdNextAddr:
                        // point to the payload
                        i++;

                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 4))
                        {
                            break;
                        }

                        iTemp = (int)BitConverter.ToUInt32(logData, i);

                        vDebugAppendStatusText("\r\nSkip to address: " + iTemp.ToString("x"), ColorError);

                        tempDGVStringStruct.sComment = "Skip to address: " + iTemp.ToString("x");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                        bPopulatedComment = true;

                        // point to the address to skip to -1 because we increment i at the bottom of the loop
                        i = iTemp - 1;

                        // since we are skipping a bunch of the data, don't copy the skipped part into the debug list
                        payloadStartIndex = i + 1;

                        break;

                    //
                    // handle Any Unknown Commands
                    //
                    default:
                        // if there is a runt command at the end, skip past it
                        if (!CheckLogDataOk(cmdIndex, i, 1))
                        {
                            break;
                        }

                        AppendStatusText("\r\nvParseLog: Unknown command " + logData[i].ToString("x2") + " at " + i.ToString("x"), ColorError);

                        tempDGVStringStruct.sComment = "vParseLog: Unknown command " + logData[i].ToString("x2") + " at " + i.ToString("x");
                        tempDGVStringStruct.eCommentColor = MyColors.eColorError;

                        vAddErrorToRunGridView("Error: Unknown " + logData[i].ToString("x2") + " at " + i.ToString("x"), DGVStringList.Count);

                        bPopulatedComment = true;

                        break;
                }

                if (bPopulatedComment)
                {
                    int tempRowIndex;

                    //
                    // The first rows are comments and so RowIndex will be zero
                    // so that will make tempRowIndex negative. Skip that case.
                    //
                    if (RowIndex > 0)
                    {
                        tempRowIndex = RowIndex - 1;
                    }
                    else
                    {
                        tempRowIndex = 0;
                    }

                    tempDGVStringStruct.iDataIndex = tempRowIndex;

                    //
                    // Wrap comment information
                    //
                    if ((tempDGVStringStruct.sComment != null) && (tempDGVStringStruct.sComment.Length != 0))
                    {
                        List<string> sWrappedComment = WordWrap(tempDGVStringStruct.sComment, iNumCharCommentColumn);

                        // only need to add grid entries if we have more than one comment line
                        if (sWrappedComment.Count > 1)
                        {
                            //
                            // Don't include the last comment because we want the normal logic
                            // to handle it.
                            //
                            MyColors tempColor = tempDGVStringStruct.eCommentColor;

                            for (int iComment = 0; iComment < sWrappedComment.Count - 1; iComment++)
                            {
                                tempDGVStringStruct.sComment = sWrappedComment[iComment];

                                tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();

                                if (CorrectorList.Count == 0)
                                {
                                    tempDGVStringStruct.iCorrectorIndex = 0;
                                }
                                else
                                {
                                    tempDGVStringStruct.iCorrectorIndex = CorrectorList.Count - 1;
                                }

                                DGVStringList.Add(tempDGVStringStruct);

                                tempDGVStringStruct = default(DGVStringStruct);

                                tempDGVStringStruct.iDataIndex = tempRowIndex;

                                //
                                // The color is initialized above. We re-initialize
                                // it here since we just cleared out tempDGVStringStruct
                                //
                                tempDGVStringStruct.eCommentColor = tempColor;
                            }

                            //
                            // Add the comment for the grid entry to be handled by the
                            // normal logic.
                            //
                            tempDGVStringStruct.sComment = sWrappedComment[sWrappedComment.Count - 1];
                        }
                    }

                    tempDGVStringStruct.iRunGridViewListIndex = iGetrunGridViewListCount();

                    if (CorrectorList.Count == 0)
                    {
                        tempDGVStringStruct.iCorrectorIndex = 0;
                    }
                    else
                    {
                        tempDGVStringStruct.iCorrectorIndex = CorrectorList.Count - 1;
                    }

                    DGVStringList.Add(tempDGVStringStruct);

                    //
                    // since we just wrote out the data and comment, clear the
                    // flags and the data string structure
                    //
                    bPopulatedData = false;
                    bPopulatedComment = false;

                    tempDGVStringStruct = default(DGVStringStruct);
                }

                // copy this message's payload into the debug list
                int payloadLength = i - payloadStartIndex + 1;

                if (payloadLength != 0)
                {
                    // if there is a runt command at the end, skip past it
                    if (!CheckLogDataOk(cmdIndex, i, payloadLength))
                    {
                        break;
                    }

                    tempParseDebugStruct.LogData = new byte[payloadLength];

                    Buffer.BlockCopy(logData, payloadStartIndex, tempParseDebugStruct.LogData, 0, payloadLength);

                    ParseDebugList.Add(tempParseDebugStruct);

                }

                // update the progress bar
                displayProgress(((i * 100) / iLogLength) + 1);
            }

            if (streamWriterLog != null)
            {
                streamWriterLog.Close();
            }

            // if we have a runt file, and we don't have a "LMCmdRunEndFail" or
            // "LMCmdRunEndSuccess", then the data for the last run won't be
            // added and the dashboard code chokes, so, go ahead and fake it
            //
            if (bHaveRunStartedButNotRunFinished)
            {
                vRunFinished(false, 0);
            }

            return;
        }

        private void clearLogList()
        {
            MPDList.Clear();

            MPDList.Capacity = 0;

            return;
        }

        private void convertLogDataToGraphData()
        {
            double[] dIndex = new double[MPDList.Count];

            for (int i = 0; i < dIndex.Length; i++)
            {
                dIndex[i] = i;
            }

            dLFPList = new FilteredPointList(dIndex, MPDList.Select(item => item.dLFP).ToArray());
            dRFPList = new FilteredPointList(dIndex, MPDList.Select(item => item.dRFP).ToArray());

            dLFDList = new FilteredPointList(dIndex, MPDList.Select(item => item.dLFD).ToArray());
            dRFDList = new FilteredPointList(dIndex, MPDList.Select(item => item.dRFD).ToArray());

            dLDPList = new FilteredPointList(dIndex, MPDList.Select(item => item.dLDP).ToArray());
            dRDPList = new FilteredPointList(dIndex, MPDList.Select(item => item.dRDP).ToArray());

            dLDDList = new FilteredPointList(dIndex, MPDList.Select(item => item.dLDD).ToArray());
            dRDDList = new FilteredPointList(dIndex, MPDList.Select(item => item.dRDD).ToArray());

            dFVelList = new FilteredPointList(dIndex, MPDList.Select(item => item.dFVel).ToArray());
            dFErrList = new FilteredPointList(dIndex, MPDList.Select(item => item.dFErr).ToArray());

            dRVelList = new FilteredPointList(dIndex, MPDList.Select(item => item.dRVel).ToArray());
            dRErrList = new FilteredPointList(dIndex, MPDList.Select(item => item.dRErr).ToArray());

            LFP_line = GraphPane1.AddCurve("LFP", dLFPList, Color.Blue, SymbolType.None);
            RFP_line = GraphPane1.AddCurve("RFP", dRFPList, Color.Red, SymbolType.None);
            LFD_line = GraphPane1.AddCurve("LFD", dLFDList, Color.Blue, SymbolType.Plus);
            RFD_line = GraphPane1.AddCurve("RFD", dRFDList, Color.Red, SymbolType.Plus);

            LDP_line = GraphPane2.AddCurve("LDP", dLDPList, Color.Blue, SymbolType.None);
            RDP_line = GraphPane2.AddCurve("RDP", dRDPList, Color.Red, SymbolType.None);
            LDD_line = GraphPane2.AddCurve("LDD", dLDDList, Color.Blue, SymbolType.Plus);
            RDD_line = GraphPane2.AddCurve("RDD", dRDDList, Color.Red, SymbolType.Plus);

            FVel_line = GraphPane3.AddCurve("FVel", dFVelList, Color.Blue, SymbolType.None);
            FErr_line = GraphPane3.AddCurve("FErr", dFErrList, Color.Red, SymbolType.Plus);

            RVel_line = GraphPane4.AddCurve("RVel", dRVelList, Color.Blue, SymbolType.None);
            RErr_line = GraphPane4.AddCurve("RErr", dRErrList, Color.Red, SymbolType.Plus);

            FVel_line.IsVisible = true;
            FVel_line.Label.IsVisible = true;

            FErr_line.IsVisible = true;
            FErr_line.Label.IsVisible = true;

            RVel_line.IsVisible = true;
            RVel_line.Label.IsVisible = true;

            RErr_line.IsVisible = true;
            RErr_line.Label.IsVisible = true;

            // put the distance on Y2 axis and power on Y1 axis
            LFD_line.IsY2Axis = true;
            RFD_line.IsY2Axis = true;

            LDD_line.IsY2Axis = true;
            RDD_line.IsY2Axis = true;

            FErr_line.IsY2Axis = true;

            RErr_line.IsY2Axis = true;

            //
            // From the sample code @ C:\Temp\ZedGraph\Wiki\zedgraph.org\wiki\indexe3e9.html
            //
            // The IsApplyHighLowLogic property does not work properly.  This option has been removed as
            // of ZedGraph version 5.1.4.  In the meantime, you should disable it as follows
            //
            // This errors out now, so commented out for now...
            // dLFPList.IsApplyHighLowLogic = false;

            // To change how much is displayed modify this
            dLFPList.SetBounds(0, dIndex.Length - 1, 1000);
            dRFPList.SetBounds(0, dIndex.Length - 1, 1000);
            dLFDList.SetBounds(0, dIndex.Length - 1, 1000);
            dRFDList.SetBounds(0, dIndex.Length - 1, 1000);

            dLDPList.SetBounds(0, dIndex.Length - 1, 1000);
            dRDPList.SetBounds(0, dIndex.Length - 1, 1000);
            dLDDList.SetBounds(0, dIndex.Length - 1, 1000);
            dRDDList.SetBounds(0, dIndex.Length - 1, 1000);

            dFVelList.SetBounds(0, dIndex.Length - 1, 1000);
            dFErrList.SetBounds(0, dIndex.Length - 1, 1000);

            dRVelList.SetBounds(0, dIndex.Length - 1, 1000);
            dRErrList.SetBounds(0, dIndex.Length - 1, 1000);

            // do this to speed up rendering. If this causes visual artifacts, delete the lines
            LFP_line.Line.IsOptimizedDraw = true;
            RFP_line.Line.IsOptimizedDraw = true;
            LFD_line.Line.IsOptimizedDraw = true;
            RFD_line.Line.IsOptimizedDraw = true;

            LDP_line.Line.IsOptimizedDraw = true;
            RDP_line.Line.IsOptimizedDraw = true;
            LDD_line.Line.IsOptimizedDraw = true;
            RDD_line.Line.IsOptimizedDraw = true;

            FVel_line.Line.IsOptimizedDraw = true;
            FErr_line.Line.IsOptimizedDraw = true;

            RVel_line.Line.IsOptimizedDraw = true;
            RErr_line.Line.IsOptimizedDraw = true;

            //
            // draw in lat/long corr boundary lines
            // logic copied from sensDataGridViw_AllAllData()
            //
            int iNumGridEntries = DGVStringList.Count;

            for (int i = 1; i < iNumGridEntries; i++)
            {
                var iDataIndex = DGVStringArray[i].iDataIndex;

                if (iDataIndex >= MPDList.Count)
                {
                    break;
                }

                // start of lateral correction & end of longitudinal correction
                if (DGVStringArray[i].sLat == "Start")
                {
                    var line = new LineObj(ColorLatCorr, iDataIndex, 0, iDataIndex, 0.2);

                    line.Location.CoordinateFrame = CoordType.XScaleYChartFraction;
                    line.IsClippedToChartRect = true;

                    line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
                    line.Line.Width = 1f;

                    GraphPane1.GraphObjList.Add(line);
                    GraphPane2.GraphObjList.Add(line);
                    GraphPane3.GraphObjList.Add(line);
                    GraphPane4.GraphObjList.Add(line);
                }

                // end of lateral correction & start of longitudinal correction
                if (DGVStringArray[i].sLat == "Dis")
                {
                    var line = new LineObj(ColorLongCorr, iDataIndex, 0, iDataIndex, 0.2);

                    line.Location.CoordinateFrame = CoordType.XScaleYChartFraction;
                    line.IsClippedToChartRect = true;

                    line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
                    line.Line.Width = 1f;

                    GraphPane1.GraphObjList.Add(line);
                    GraphPane2.GraphObjList.Add(line);
                    GraphPane3.GraphObjList.Add(line);
                    GraphPane4.GraphObjList.Add(line);
                }
            }

            return;
        }

        private void vUpdateGraphs(int iDataGridViewIndex)
        {
            // TODO: Add code to not redraw graphs if iDataGridViewIndex has not changed

            Scale xScalePane1 = zedGraphControlPane1.GraphPane.XAxis.Scale;
            Scale xScalePane2 = zedGraphControlPane2.GraphPane.XAxis.Scale;
            Scale xScalePane3 = zedGraphControlPane3.GraphPane.XAxis.Scale;
            Scale xScalePane4 = zedGraphControlPane4.GraphPane.XAxis.Scale;

            int iDataIndex = DGVStringArray[iDataGridViewIndex].iDataIndex;

            // This scheme has a wierd effect at the begining and end of the data set
            int dMinX = Math.Max(0, iDataIndex - (iNumPointsToGraph / 2));
            int dMaxX = Math.Min(MPDList.Count - 1, iDataIndex + (iNumPointsToGraph / 2));

            if (checkBoxLeftPower.CheckState == CheckState.Checked)
            {
                dLFPList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
                dLDPList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
            }

            if (checkBoxLeftDistance.CheckState == CheckState.Checked)
            {
                dLFDList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
                dLDDList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
            }

            if (checkBoxRightPower.CheckState == CheckState.Checked)
            {
                dRFPList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
                dRDPList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
            }

            if (checkBoxRightDistance.CheckState == CheckState.Checked)
            {
                dRFDList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
                dRDDList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
            }

            dFVelList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
            dFErrList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);

            dRVelList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);
            dRErrList.SetBounds(dMinX, dMaxX, iNumPointsToGraph);

            // setup the x-axis
            xScalePane1.Min = dMinX;
            xScalePane1.Max = dMaxX;

            xScalePane2.Min = dMinX;
            xScalePane2.Max = dMaxX;

            xScalePane3.Min = dMinX;
            xScalePane3.Max = dMaxX;

            xScalePane4.Min = dMinX;
            xScalePane4.Max = dMaxX;

            // Draw the cursor
            CursorPane1.Location.Height = GraphPane1.YAxis.Scale.Max - GraphPane1.YAxis.Scale.Min;
            CursorPane1.Location.X = iDataIndex;
            CursorPane1.Location.Y = GraphPane1.YAxis.Scale.Min;

            CursorPane2.Location.Height = GraphPane2.YAxis.Scale.Max - GraphPane2.YAxis.Scale.Min;
            CursorPane2.Location.X = iDataIndex;
            CursorPane2.Location.Y = GraphPane2.YAxis.Scale.Min;

            CursorPane3.Location.Height = GraphPane3.YAxis.Scale.Max - GraphPane3.YAxis.Scale.Min;
            CursorPane3.Location.X = iDataIndex;
            CursorPane3.Location.Y = GraphPane3.YAxis.Scale.Min;

            CursorPane4.Location.Height = GraphPane4.YAxis.Scale.Max - GraphPane4.YAxis.Scale.Min;
            CursorPane4.Location.X = iDataIndex;
            CursorPane4.Location.Y = GraphPane4.YAxis.Scale.Min;

            zedGraphControlPane1.AxisChange();
            zedGraphControlPane1.Invalidate();

            zedGraphControlPane2.AxisChange();
            zedGraphControlPane2.Invalidate();

            zedGraphControlPane3.AxisChange();
            zedGraphControlPane3.Invalidate();

            zedGraphControlPane4.AxisChange();
            zedGraphControlPane4.Invalidate();

            return;
        }

        private void sensDataGridView_Setup()
        {
            bDataInitialized = false;

            sensDataGridView.VirtualMode = true;

            //
            // Having a hard time doing wrapped comments, so simplifying things by
            // using a fixed width font and then wrapping comments by creating
            // separate grid entries.
            //
            sensDataGridView.Font = new Font("Consolas", 8);

            sensDataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            sensDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.SkyBlue;
            sensDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            sensDataGridView.ColumnHeadersDefaultCellStyle.Font =
                new Font(sensDataGridView.Font, FontStyle.Bold);

            sensDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            sensDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            sensDataGridView.GridColor = Color.Black;
            sensDataGridView.RowHeadersVisible = false;

            sensDataGridView.BorderStyle = BorderStyle.None;
            sensDataGridView.EnableHeadersVisualStyles = false;
            sensDataGridView.ShowCellToolTips = false;

            sensDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            sensDataGridView.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            //
            // Setup the rows to not autoresize. Once we load the data, we'll
            // resize the height once and be done.
            //
            sensDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            sensDataGridView.RowHeadersWidthSizeMode =
                DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            sensDataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            sensDataGridView.ReadOnly = true;
            sensDataGridView.AllowUserToDeleteRows = false;

            sensDataGridView.AllowUserToResizeColumns = false;
            sensDataGridView.AllowUserToResizeRows = false;

            //
            // show the values on the right hand side for all columns except "mm" and "Comment"
            // the override for "mm" and "Comment" is below
            //
            sensDataGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // set the height since we are using a fixed font and doing wrapping ourselves
            sensDataGridView.RowTemplate.Height = 14;

            sensDataGridView.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            sensDataGridView.MultiSelect = false;

            sensDataGridView.ColumnCount = 17;
            sensDataGridView.Columns[0].Name = "mm";

            sensDataGridView.Columns[1].Name = "LFP";
            sensDataGridView.Columns[2].Name = "LDP";
            sensDataGridView.Columns[3].Name = "RDP";
            sensDataGridView.Columns[4].Name = "RFP";

            sensDataGridView.Columns[5].Name = "LFD";
            sensDataGridView.Columns[6].Name = "LDD";
            sensDataGridView.Columns[7].Name = "RDD";
            sensDataGridView.Columns[8].Name = "RFD";

            sensDataGridView.Columns[9].Name = "FVel";
            sensDataGridView.Columns[10].Name = "FErr";
            sensDataGridView.Columns[11].Name = "RVel";
            sensDataGridView.Columns[12].Name = "RPos";
            sensDataGridView.Columns[13].Name = "RErr";

            sensDataGridView.Columns[14].Name = "Lat";
            sensDataGridView.Columns[15].Name = "Long";

            sensDataGridView.Columns[16].Name = "Comment";

            // For the sensor power and distance and Diagonal and
            // front wall sensor data change the header color
            sensDataGridView.Columns["mm"].DefaultCellStyle.BackColor = Color.GreenYellow;

            sensDataGridView.Columns["LFP"].DefaultCellStyle.BackColor = Color.Salmon;
            sensDataGridView.Columns["LDP"].DefaultCellStyle.BackColor = Color.Salmon;
            sensDataGridView.Columns["RDP"].DefaultCellStyle.BackColor = Color.Salmon;
            sensDataGridView.Columns["RFP"].DefaultCellStyle.BackColor = Color.Salmon;

            sensDataGridView.Columns["LFD"].DefaultCellStyle.BackColor = Color.Pink;
            sensDataGridView.Columns["LDD"].DefaultCellStyle.BackColor = Color.Pink;
            sensDataGridView.Columns["RDD"].DefaultCellStyle.BackColor = Color.Pink;
            sensDataGridView.Columns["RFD"].DefaultCellStyle.BackColor = Color.Pink;

            sensDataGridView.Columns["FVel"].DefaultCellStyle.BackColor = Color.GreenYellow;
            sensDataGridView.Columns["FErr"].DefaultCellStyle.BackColor = Color.MediumSpringGreen;
            sensDataGridView.Columns["RVel"].DefaultCellStyle.BackColor = Color.GreenYellow;
            sensDataGridView.Columns["RPos"].DefaultCellStyle.BackColor = Color.GreenYellow;
            sensDataGridView.Columns["RErr"].DefaultCellStyle.BackColor = Color.MediumSpringGreen;

            sensDataGridView.Columns["Lat"].DefaultCellStyle.BackColor = Color.AliceBlue;
            sensDataGridView.Columns["Long"].DefaultCellStyle.BackColor = Color.AliceBlue;

            sensDataGridView.Columns["Comment"].DefaultCellStyle.BackColor = SystemColors.Control;

            // for the mm and correction column, show the info. in the middle of the cell
            sensDataGridView.Columns["mm"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            sensDataGridView.Columns["Lat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            sensDataGridView.Columns["Long"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // for the Comment column, show the info. on the left hand side of the cell
            sensDataGridView.Columns["Comment"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // don't wrap text in Comment column
            //sensDataGridView.Columns["Comment"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Set the background color for all rows and for alternating rows.
            // The value for alternating rows overrides the value for all rows.
            //        sensDataGridView.RowsDefaultCellStyle.BackColor = Color.White;
            //        sensDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            sensDataGridView.Columns["mm"].Width = 30;

            sensDataGridView.Columns["LFP"].Width = 35;
            sensDataGridView.Columns["LDP"].Width = 35;
            sensDataGridView.Columns["RDP"].Width = 35;
            sensDataGridView.Columns["RFP"].Width = 35;

            sensDataGridView.Columns["LFD"].Width = 40;
            sensDataGridView.Columns["LDD"].Width = 40;
            sensDataGridView.Columns["RDD"].Width = 40;
            sensDataGridView.Columns["RFD"].Width = 40;

            sensDataGridView.Columns["FVel"].Width = 40;
            sensDataGridView.Columns["FErr"].Width = 35;
            sensDataGridView.Columns["RVel"].Width = 40;
            sensDataGridView.Columns["RPos"].Width = 40;
            sensDataGridView.Columns["RErr"].Width = 35;
            sensDataGridView.Columns["Lat"].Width = 50;
            sensDataGridView.Columns["Long"].Width = 50;
            sensDataGridView.Columns["Comment"].Width = 176;

            //
            // Consolas is a fixed width font, so use a space to determine the number of characters
            // that will fit in the Comment column.
            //
            // When the comment is printed, the width will be the number of characters plus the padding.
            // So, to compute iNumCharCommentColumn, we have to account for both the padding and the
            // characters.
            //
            // The difference between iTemp2Spaces and iTemp1Space is the width of a character and the
            // difference between iTemp1Space and the width of the character is the padding.
            //
            int iTemp2Spaces = TextRenderer.MeasureText("  ", sensDataGridView.Font).Width;
            int iTemp1Space = TextRenderer.MeasureText(" ", sensDataGridView.Font).Width;

            iNumCharCommentColumn = (sensDataGridView.Columns["Comment"].Width - iTemp1Space)
                / (iTemp2Spaces - iTemp1Space) + 1;

            // definitions pertaining to the colors we use in the Comment column
            myColors[(int)MyColors.eColorNone] = ColorError;             // show in error color because this isn't supposed to happen
            myColors[(int)MyColors.eColorError] = ColorError;
            myColors[(int)MyColors.eColorLongCorr] = ColorLongCorr;
            myColors[(int)MyColors.eColorLatCorr] = ColorLatCorr;
            myColors[(int)MyColors.eColorMaze] = ColorMaze;
            myColors[(int)MyColors.eColorMotion] = ColorMotion;
            myColors[(int)MyColors.eColorMotionTurn] = ColorMotionTurn;
            myColors[(int)MyColors.eColorMisc] = ColorMisc;

            sensDataGridView.DoubleBuffered(true);
        }

        void sensDataGridView_AddAllData()
        {
            int iNumGridEntries = DGVStringList.Count;

            int iNumEntries = MPDList.Count;

            // process data if we have some...
            if ((iNumGridEntries == 0) || (iNumEntries == 0))
            {
                return;
            }

            DGVStringArray = new DGVStringStruct[iNumGridEntries];

            DGVStringArray = DGVStringList.ToArray();

            MotionProfileDataStruct tempMPD;

            tempMPD = MPDList[0];

            tempMPD.dRVel = 0.0;

            MPDList[0] = tempMPD;

            int iDataIndex = 1;

            for (int i = 1; (i < iNumGridEntries) && (iDataIndex < iNumEntries); i++)
            {
                // only populate sRVel if sRPos has a value
                if (DGVStringArray[i].sRPos != null)
                {
                    double dTemp = MPDList[iDataIndex].dRPos - MPDList[iDataIndex - 1].dRPos;

                    //
                    // dRPos has a range of 0 to 359.x but dRVel has a range of +/-Y.
                    // So, convert from one range domain to the other.
                    //
                    if (dTemp > 180)
                    {
                        dTemp -= 360.0;
                    }
                    else if (dTemp < -180)
                    {
                        dTemp += 360;
                    }

                    tempMPD = MPDList[iDataIndex];

                    // when we start/stop a run, we get a very high rotational velocity because we zero out rotational position
                    // so, limit what it can be so that we don't screw up the graph
                    if (Math.Abs(dTemp) > 3.0)
                    {
                        dTemp = 0.0;
                    }

                    tempMPD.dRVel = dTemp;

                    MPDList[iDataIndex] = tempMPD;

                    DGVStringArray[i].sRVel = tempMPD.dRVel.ToString("#0.00");

                    iDataIndex++;
                }
            }

            // convert the log data to data format the graphs can use
            convertLogDataToGraphData();

            bDataInitialized = true;

            //
            // enable addition of rows because we need to change the number of
            // rows
            //
            sensDataGridView.AllowUserToAddRows = true;

            sensDataGridView.Rows.AddCopies(0, iNumGridEntries);

            sensDataGridView.AllowUserToAddRows = false;

            // refresh the runGridView so that the colors can be updated
            runGridView.Refresh();

            return;
        }

        private void sensDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (!bDataInitialized)
            {
                return;
            }

            int row = e.RowIndex;

            switch (e.ColumnIndex)
            {
                case 0:
                    e.Value = DGVStringArray[row].smm;
                    break;

                case 1:
                    e.Value = DGVStringArray[row].sLFP;
                    break;

                case 2:
                    e.Value = DGVStringArray[row].sLDP;
                    break;

                case 3:
                    e.Value = DGVStringArray[row].sRDP;
                    break;

                case 4:
                    e.Value = DGVStringArray[row].sRFP;
                    break;

                case 5:
                    e.Value = DGVStringArray[row].sLFD;
                    break;

                case 6:
                    e.Value = DGVStringArray[row].sLDD;
                    break;

                case 7:
                    e.Value = DGVStringArray[row].sRDD;
                    break;

                case 8:
                    e.Value = DGVStringArray[row].sRFD;
                    break;

                case 9:
                    e.Value = DGVStringArray[row].sFVel;
                    break;

                case 10:
                    e.Value = DGVStringArray[row].sFErr;
                    break;

                case 11:
                    e.Value = DGVStringArray[row].sRVel;
                    break;

                case 12:
                    e.Value = DGVStringArray[row].sRPos;
                    break;

                case 13:
                    e.Value = DGVStringArray[row].sRErr;
                    break;

                case 14:
                    e.Value = DGVStringArray[row].sLat;
                    break;

                case 15:
                    e.Value = DGVStringArray[row].sLong;
                    break;

                case 16:
                    e.Value = DGVStringArray[row].sComment;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return;
        }

        private void sensDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (!bDataInitialized || (CellList.Count == 0))
            {
                return;
            }

            int iDataGridViewIndex = sensDataGridView.CurrentCell.RowIndex;

            int iDataIndex = DGVStringArray[iDataGridViewIndex].iDataIndex;

            displayIndex(iDataIndex);

            displayGridIndex(iDataGridViewIndex);

            textBoxVbat.Text = MPDList[iDataIndex].dVBat.ToString("0.00");

            displayCorrection(
                CorrectorList[DGVStringArray[iDataGridViewIndex].iCorrectorIndex].sOffset,
                CorrectorList[DGVStringArray[iDataGridViewIndex].iCorrectorIndex].sOffsetDot,
                CorrectorList[DGVStringArray[iDataGridViewIndex].iCorrectorIndex].sAngle,
                CorrectorList[DGVStringArray[iDataGridViewIndex].iCorrectorIndex].sAngleDot);

            mouseAngleGauge.Value = (float)MPDList[iDataIndex].dRPos;

            int iTempRunGridViewListIndex = DGVStringArray[iDataGridViewIndex].iRunGridViewListIndex;

            if (!bSkiprunGridViewShowCellCall)
            {
                vrunGridViewShowCell(iTempRunGridViewListIndex);
            }

            vShowWalls(iTempRunGridViewListIndex);

            vShowPath(iTempRunGridViewListIndex);

            this.mazeControl.DrawMouse((byte)CellList[RunGridViewList[iTempRunGridViewListIndex].iCellListIndex].iCell);

            displayMouseState(
                (byte)CellList[RunGridViewList[iTempRunGridViewListIndex].iCellListIndex].iCell,
                (byte)CellList[RunGridViewList[iTempRunGridViewListIndex].iCellListIndex].CurOrient,
                RunsList[RunGridViewList[iTempRunGridViewListIndex].iRunListIndex].sRunMode);

            vUpdateGraphs(iDataGridViewIndex);

            return;
        }

        private void sensDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!bDataInitialized)
            {
                return;
            }

            // format the Comment column based on the content
            if (e.ColumnIndex == 16)
            {
                e.CellStyle.ForeColor = myColors[(int)DGVStringArray[e.RowIndex].eCommentColor];
            }

            return;
        }

        private void sensDataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            if (sensDataGridView.CurrentCell.RowIndex < sensDataGridView.FirstDisplayedCell.RowIndex)
            {
                sensDataGridView.CurrentCell = sensDataGridView.FirstDisplayedCell;
            }
            else if (sensDataGridView.CurrentCell.RowIndex > (sensDataGridView.FirstDisplayedCell.RowIndex + sensDataGridView.DisplayedRowCount(false) - 1))
            {
                sensDataGridView.CurrentCell = sensDataGridView.Rows[sensDataGridView.FirstDisplayedCell.RowIndex + sensDataGridView.DisplayedRowCount(false) - 1].Cells[0];
            }

            return;
        }

        // TODO: This is currently not used. It is being kept around in case
        // TODO: I need this code/call for doing something...
        // to enable this, go to MouseDash.cs [Design], select the sensDataGridView and enable the CellPainting event
        private void sensDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // ignore the column header and row header cells
           if (e.RowIndex != -1 && e.ColumnIndex != -1)
           {
              e.PaintBackground(e.ClipBounds, true);
              e.Graphics.DrawString(Convert.ToString(e.FormattedValue), e.CellStyle.Font, new SolidBrush(e.CellStyle.ForeColor), e.CellBounds.X, e.CellBounds.Y, StringFormat.GenericDefault);
              e.Handled = true;
           }

           return;
        }

        bool bSkiprunGridViewShowCellCall = false;

        private void vsensDataGridViewShowRow(int iDGVIndex)
        {
            if (!bDataInitialized)
            {
                return;
            }

            DataGridViewCell tempDataGridViewCell = sensDataGridView.CurrentCell;

            if (tempDataGridViewCell.RowIndex != iDGVIndex)
            {
                //
                // If we click on a cell in runGridView, it calls this function
                // And then in sensDataGridView_SelectionChanged, we call
                // vrunGridViewSelectCell which results in a circular loop - I believe.
                //
                // So, I have pre-emptively added this condition check to avoid
                // the loop.
                //
                bSkiprunGridViewShowCellCall = true;

                sensDataGridView.CurrentCell = sensDataGridView.Rows[iDGVIndex].Cells[0];
            }

            bSkiprunGridViewShowCellCall = false;

            return;
        }

        // routine picked up from: https://github.com/anderssonjohan/snippets/blob/master/wordwrap/WordWrapTests.cs
        public static List<string> WordWrap(string text, int maxLineLength)
        {
            var list = new List<string>();

            int currentIndex;
            var lastWrap = 0;
            var whitespace = new[] { ' ', '\r', '\n', '\t' };
            var CrLf = new[] { '\r', '\n' };

            do
            {
                // add the +1 so that we include the CrLf character and then trim it away when we add the substring
                int iCrLfIndex = text.IndexOfAny(CrLf, lastWrap, Math.Min(text.Length - 1 - lastWrap, maxLineLength)) + 1;

                currentIndex = lastWrap + maxLineLength > text.Length ? text.Length : (text.LastIndexOfAny(new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' }, Math.Min(text.Length - 1, lastWrap + maxLineLength)) + 1);

                currentIndex = Math.Min(currentIndex, iCrLfIndex);

                if (currentIndex <= lastWrap)
                    currentIndex = Math.Min(lastWrap + maxLineLength, text.Length);
                list.Add(text.Substring(lastWrap, currentIndex - lastWrap).Trim(whitespace));
                lastWrap = currentIndex;
            } while (currentIndex < text.Length);

            return list;
        }

        private void runGridView_Setup()
        {
            //
            // Use a fixed width font so that entries line up.
            //
            runGridView.Font = new Font("Consolas", 8);

            runGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            runGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.SkyBlue;
            runGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            runGridView.ColumnHeadersDefaultCellStyle.Font =
                new Font(runGridView.Font, FontStyle.Bold);

            runGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            runGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            runGridView.GridColor = Color.Black;
            runGridView.RowHeadersVisible = false;

            runGridView.BorderStyle = BorderStyle.None;
            runGridView.EnableHeadersVisualStyles = false;
            runGridView.ShowCellToolTips = false;

            runGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            runGridView.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            //
            // Setup the rows to not autoresize. Once we load the data, we'll
            // resize the height once and be done.
            //
            runGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            runGridView.RowHeadersWidthSizeMode =
                DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            runGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            runGridView.ReadOnly = true;
            runGridView.AllowUserToDeleteRows = false;

            runGridView.AllowUserToResizeColumns = false;
            runGridView.AllowUserToResizeRows = false;

            //
            // show the values on the right hand side for all columns except "mm" and "Comment"
            // the override for "mm" and "Comment" is below
            //
            runGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // set the height since we are using a fixed font and doing wrapping ourselves
            runGridView.RowTemplate.Height = 14;

            runGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
            runGridView.MultiSelect = false;

            runGridView.ColumnCount = iNumRunDataGridViewColumns;
            runGridView.Columns[0].Name = "Run";

            runGridView.Columns[1].Name = "Cell";
            runGridView.Columns[2].Name = "Cell";
            runGridView.Columns[3].Name = "Cell";
            runGridView.Columns[4].Name = "Cell";

            // For the sensor power and distance and Diagonal and
            // front wall sensor data change the header color
            runGridView.Columns["Run"].DefaultCellStyle.BackColor = Color.GreenYellow;

            runGridView.Columns[1].DefaultCellStyle.BackColor = SystemColors.Control;
            runGridView.Columns[2].DefaultCellStyle.BackColor = SystemColors.Control;
            runGridView.Columns[3].DefaultCellStyle.BackColor = SystemColors.Control;
            runGridView.Columns[4].DefaultCellStyle.BackColor = SystemColors.Control;

            // for the Run column, show the info. in the middle of the cell
            runGridView.Columns["Run"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // for the Cell column, show the info. on the left hand side of the cell
            runGridView.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            runGridView.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            runGridView.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            runGridView.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // don't wrap text in Cell column
            runGridView.Columns[1].DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            runGridView.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            runGridView.Columns[3].DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            runGridView.Columns[4].DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            //
            // Consolas is a fixed width font, so use a space to determine the width of a column.
            //
            // When the comment is printed, the width will be the number of characters plus the padding.
            //
            // The difference between iTemp2Spaces and iTemp1Space is the width of a character and the
            // difference between iTemp1Space and the width of the character is the padding.
            //
            int iTemp2Spaces = TextRenderer.MeasureText("  ", sensDataGridView.Font).Width;
            int iTemp1Space = TextRenderer.MeasureText(" ", sensDataGridView.Font).Width;

            runGridView.Columns["Run"].Width = iTemp2Spaces;

            // the - 5 is a fudge factor to make the columns wide enough to print the turn moves
            int iTempColumnWidth = iTemp2Spaces + 6 * (iTemp2Spaces - iTemp1Space) - 5;
            runGridView.Columns[1].Width = iTempColumnWidth;
            runGridView.Columns[2].Width = iTempColumnWidth;
            runGridView.Columns[3].Width = iTempColumnWidth;
            runGridView.Columns[4].Width = iTempColumnWidth;

            runGridView.DoubleBuffered(true);

            return;
        }


        private void runGridView_Scroll(object sender, ScrollEventArgs e)
        {
            return;
        }

        private int iGetRunGridViewListIndex(int iRow, int iColumn)
        {
            //
            // The first column in the grid doesn't have a RunGridViewList entry associated with it.
            // So, use the first columns info.
            //
            if (iColumn == 0)
            {
                iColumn = 1;
            }

            //
            // The first column in the grid doesn't have a RunGridViewList entry associated with it.
            // So, we need to subtract off one from iNumRunDataGridViewColumns.
            //
            // This means the first column with data is 1. This means we have to subtract one from iColumnToUse.
            //
            int iListIndex = iRow * (iNumRunDataGridViewColumns - 1) + iColumn - 1;

            if (iListIndex >= RunGridViewList.Count)
            {
                iListIndex = RunGridViewList.Count - 1;
            }

            return(iListIndex);
        }

        private void runGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (!bDataInitialized)
            {
                return;
            }

            if (!bSkipsensDataGridViewShowRunCall)
            {
                int iListIndex = iGetRunGridViewListIndex(runGridView.CurrentCell.RowIndex,
                                                            runGridView.CurrentCell.ColumnIndex);

                vsensDataGridViewShowRow(RunGridViewList[iListIndex].iDGVIndex);
            }

            return;
        }

        private void runGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!bDataInitialized)
            {
                return;
            }

            // format the Comment column based on the content
            // an alternate way of doing this: if (sensDataGridView.Columns[e.ColumnIndex].Name == "Comment")
            if (e.ColumnIndex != 0)
            {
                int iListIndex = iGetRunGridViewListIndex(e.RowIndex, e.ColumnIndex);

                int iDGVIndex = RunGridViewList[iListIndex].iDGVIndex;

                e.CellStyle.ForeColor = myColors[(int)DGVStringArray[iDGVIndex].eCommentColor];
            }

            return;
        }

        private void runGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
#if DRAW_N_PAINT
            if (e.ColumnIndex == 2 && e.RowIndex == 0)
            {
                Rectangle newRect = new Rectangle(e.CellBounds.X + 1,
                    e.CellBounds.Y + 1, e.CellBounds.Width - 4,
                    e.CellBounds.Height - 4);

                using (
                    Brush gridBrush = new SolidBrush(this.runGridView.GridColor),
                    backColorBrush = new SolidBrush(e.CellStyle.BackColor))
                {
                    using (Pen gridLinePen = new Pen(gridBrush))
                    {
                        // Erase the cell.
                        e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

                        // Draw the grid lines (only the right and bottom lines;
                        // DataGridView takes care of the others).
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
                            e.CellBounds.Bottom - 1, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1,
                            e.CellBounds.Top, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom);

                        // Draw the inset highlight box.
                        e.Graphics.DrawRectangle(Pens.Blue, newRect);

                        // Draw the text content of the cell, ignoring alignment.
//                        if (e.Value != null)
                        {
//                            e.Graphics.DrawString((String)e.Value, e.CellStyle.Font,
                            e.Graphics.DrawString("Hello", e.CellStyle.Font,
                                Brushes.Crimson, e.CellBounds.X + 2,
                                e.CellBounds.Y + 2, StringFormat.GenericDefault);
                        }
                        e.Handled = true;
                    }
                }
            }
#endif
            return;
        }

        RunStruct CurrentRun;

        Orient CurrentOrientation = Orient.eNorth;

        bool bHaveCell = false;

        CellStruct tempCell;

        RunCellWallPathStruct tempRunGridViewListEntry;

        int iCurrentRunGridRow = 0;
        int iCurrentRunGridColumn = 0;

        bool bSkipsensDataGridViewShowRunCall = false;

        public void vrunGridViewShowCell(int iRunGridViewListIndex)
        {
            if (!bDataInitialized)
            {
                return;
            }

            int iRow = iRunGridViewListIndex / (iNumRunDataGridViewColumns - 1);
            int iColumn = iRunGridViewListIndex % (iNumRunDataGridViewColumns - 1);

            DataGridViewCell tempDataGridViewCell = runGridView.CurrentCell;

            // The column has a + 1 to skip over the Run column
            iColumn++;

            if ((tempDataGridViewCell.RowIndex != iRow) || (tempDataGridViewCell.ColumnIndex != iColumn))
            {
                //
                // If we click on a cell in sensDataGridView, it calls this
                // function and then in runGridView_SelectionChanged, we call
                // vsensDataGridViewShowRow which results in a circular loop
                //
                bSkipsensDataGridViewShowRunCall = true;

                runGridView.CurrentCell = runGridView.Rows[iRow].Cells[iColumn];
            }

            bSkipsensDataGridViewShowRunCall = false;

            return;
        }

        public int iGetrunGridViewListCount()
        {
            if (bHaveCell)
            {
                return(RunGridViewList.Count);
            }
            else
            {
                if (RunGridViewList.Count != 0)
                {
                    return(RunGridViewList.Count - 1);
                }
                else
                {
                    return(0);
                }
            }
        }

        private void vAddRowToRunGridView()
        {
            string[] sTempRow = new string[iNumRunDataGridViewColumns];

            runGridView.Rows.Add(sTempRow);

            return;
        }

        // use a value that will standout in the RunGridView
        private int iCurrentRunNumber = 0;

        private void vAddEntryToRunGridView(string sEntry)
        {
            //
            // Always display the Run number in the first column
            //
            // NOTE: There is no RunGridViewList entry for these cells.
            //
            if (iCurrentRunGridColumn == 0)
            {
                vAddRowToRunGridView();

                runGridView.Rows[iCurrentRunGridRow].Cells[iCurrentRunGridColumn].Value = iCurrentRunNumber.ToString();

                iCurrentRunGridColumn++;
            }

            runGridView.Rows[iCurrentRunGridRow].Cells[iCurrentRunGridColumn].Value = sEntry;
            iCurrentRunGridColumn++;

            // Wrap around to the next row
            if (iCurrentRunGridColumn >= iNumRunDataGridViewColumns)
            {
                iCurrentRunGridColumn = 0;

                iCurrentRunGridRow++;
            }

            return;
        }

        public void vRunStarted(RunStruct NewRun)
        {
            CurrentRun = NewRun;

            CurrentRun.lsRunErrors = new List<string>();

            bHaveCell = false;

            mouseAngleGauge.Value = 0.0F;

//            CurrentOrientation = Orient.eNorth;

            // TODO: We should really use the run number from the mouse
            // TODO: but for now, we are using the local count because
            // TODO: the mouse increments the run number when it
            // TODO: leaves the start square but we (in this program) were
            // TODO: counting Start to Center as one run and Center to
            // TODO: Start as another...
//            iCurrentRunNumber = CurrentRun.iRunNum;

            if (RunsList.Count() >= 1)
            {
                iCurrentRunNumber = RunsList.Count - 1;
            }
            else
            {
                iCurrentRunNumber = 0;
            }

            CurrentRun.iFirstCellIndex = CellList.Count;

            tempRunGridViewListEntry.iRunListIndex = iCurrentRunNumber;

            bHaveRunStartedButNotRunFinished = true;

            return;
        }

        public void vRunFinished(bool bSuccess, uint ulRunTime)
        {
            CurrentRun.bRunSuccess = bSuccess;

            CurrentRun.ulRunTime = ulRunTime;

            // write out any cell that we had in progress
            vFlushCell();

            // TODO: FIX this screws up the mapping between
            // TODO: sensDataGridView and RunGridView
            //
            // Pad the RunGridView row with empty cells so that the next
            // run starts on a new row.
            //
//            while (iCurrentRunGridColumn != 0)
//            {
//                vAddEntryToRunGridView("");
//            }

            CurrentRun.iLastCellIndex = CellList.Count - 1;

            if ((CurrentRun.lsRunErrors != null) && (CurrentRun.lsRunErrors.Count == 0))
            {
                CurrentRun.lsRunErrors.Add("None");
            }

            RunsList.Add(CurrentRun);

            bHaveRunStartedButNotRunFinished = false;

            return;
        }

        private void vFlushCell()
        {
            if (bHaveCell)
            {
                tempCell.CurOrient = CurrentOrientation;

                //
                // The Count is incremented after adding, so this is the index
                // of the Cell we are going to add below.
                //
                tempRunGridViewListEntry.iCellListIndex = CellList.Count;

                CellList.Add(tempCell);

                //
                // since we've added the cell, clear the flags so that this
                // cell's state doesn't bleed into the next entries.
                //
                tempCell.bMarkedWalls = false;

                RunGridViewList.Add(tempRunGridViewListEntry);

                vAddEntryToRunGridView(tempCell.iCell.ToString("x2"));

                // indicate we don't have any cell data
                bHaveCell = false;
            }

            return;
        }

        public void vAddCellToRunGridView(int iCell, int iDGVIndex)
        {
             //
             // The start square walls are a known config. Mark them once.
             //
            if ((tempCell.iCell == 0x00) && (CellList.Count == 0))
            {
                tempCell.bMarkedWalls = true;
            }

            //
            // The cell coordinate is sent before the orientation is updated
            // so, we have to delay adding the cell to the cell list until
            // we add the next cell. vFlushCell() takes care of this by
            // using the bHaveCell flag.
            //
            vFlushCell();

            tempCell.iCell = iCell;

            tempRunGridViewListEntry.iDGVIndex = iDGVIndex;

            //
            // Set the wall index to that of the first cell - we need it to be
            // something.
            //
            tempCell.iWallListIndex = 0;

            bHaveCell = true;

            return;
        }


        public void vUpdateOrient(Orient NewOrientation)
        {
            CurrentOrientation = NewOrientation;

            return;
        }

        public void vAddToWallsList(byte ucWalls)
        {
            tempCell.bMarkedWalls = true;

            //
            // The Count is incremented after adding, so this is the index
            // of the wall we are going to add below.
            //
            tempCell.iWallListIndex = WallList.Count;

            WallStruct tempWall = new WallStruct();

            tempWall.ucWalls = ucWalls;

            WallList.Add(tempWall);

            return;
        }

        public void vAddErrorToRunGridView(string sError, int iDGVIndex)
        {
            vFlushCell();

            vAddEntryToRunGridView(sError);

            tempRunGridViewListEntry.iDGVIndex = iDGVIndex;

            RunGridViewList.Add(tempRunGridViewListEntry);

            if (CurrentRun.lsRunErrors != null)
            {
                CurrentRun.lsRunErrors.Add(sError);
            }

            return;
        }

        private void buttonSerial_Click(object sender, EventArgs e)
        {
            Form formSerial = new SerialUI();
            formSerial.ShowDialog();
        }

        public void vAddMoveToRunGridView(string sMove, int iDGVIndex)
        {
            vFlushCell();

            vAddEntryToRunGridView(sMove);

            tempRunGridViewListEntry.iDGVIndex = iDGVIndex;

            RunGridViewList.Add(tempRunGridViewListEntry);

            return;
        }

        public void vAddToPathList(PathStruct newPath)
        {
            //
            // The Count is incremented after adding, so this is the index
            // of the path we are going to add below.
            //
            tempRunGridViewListEntry.iPathListIndex = PathList.Count;

            PathList.Add(newPath);

            return;
        }

        public void vAddAbortPathToRunGridView(string sMove, int iDGVIndex)
        {
            vFlushCell();

            vAddEntryToRunGridView(sMove);

            tempRunGridViewListEntry.iDGVIndex = iDGVIndex;

            RunGridViewList.Add(tempRunGridViewListEntry);

            return;
        }

        public void vShowWalls(int iRunGridViewListIndex)
        {
            int iCellListIndex = RunGridViewList[iRunGridViewListIndex].iCellListIndex;

            if (iCellListIndex == 0)
            {
                return;
            }

            mazeControl.ResetWallsNoRefresh();

            for (int i = 0; i <= iCellListIndex; i++)
            {
                if (CellList[i].bMarkedWalls)
                {
                    mazeControl.UpdateWallsNoRefresh((byte)CellList[i].iCell, (WallFlags)WallList[CellList[i].iWallListIndex].ucWalls);
                }
            }

            mazeControl.Refresh();

            return;
        }

        int iPathListIndexLast = 0;

        public void vShowPath(int iRunGridViewListIndex)
        {
            int iPathListIndex = RunGridViewList[iRunGridViewListIndex].iPathListIndex;

            if (iPathListIndexLast != iPathListIndex)
            {
                iPathListIndexLast = iPathListIndex;

                updatePath(PathList[iPathListIndex].path, (byte)PathList[iPathListIndex].path.Length);
            }

            return;
        }

        private void NumPointsToGraph_ValueChanged(object sender, EventArgs e)
        {
            iNumPointsToGraph = (int)NumPointsToGraph.Value;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxFwdSensor_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxLeftPower.CheckState == CheckState.Checked);

            LFP_line.IsVisible = bVisible;
            LFP_line.Label.IsVisible = bVisible;

            RFP_line.IsVisible = bVisible;
            RFP_line.Label.IsVisible = bVisible;

            LFD_line.IsVisible = bVisible;
            LFD_line.Label.IsVisible = bVisible;

            RFD_line.IsVisible = bVisible;
            RFD_line.Label.IsVisible = bVisible;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxForward_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxLeftDistance.CheckState == CheckState.Checked);


            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxDiagSensor_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxRightPower.CheckState == CheckState.Checked);

            LDP_line.IsVisible = bVisible;
            LDP_line.Label.IsVisible = bVisible;

            RDP_line.IsVisible = bVisible;
            RDP_line.Label.IsVisible = bVisible;

            LDD_line.IsVisible = bVisible;
            LDD_line.Label.IsVisible = bVisible;

            RDD_line.IsVisible = bVisible;
            RDD_line.Label.IsVisible = bVisible;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxRotation_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxRightDistance.CheckState == CheckState.Checked);


            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxLeftPower_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxLeftPower.CheckState == CheckState.Checked);

            LFP_line.IsVisible = bVisible;
            LFP_line.Label.IsVisible = bVisible;

            LDP_line.IsVisible = bVisible;
            LDP_line.Label.IsVisible = bVisible;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxLeftDistance_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxLeftDistance.CheckState == CheckState.Checked);

            LFD_line.IsVisible = bVisible;
            LFD_line.Label.IsVisible = bVisible;

            LDD_line.IsVisible = bVisible;
            LDD_line.Label.IsVisible = bVisible;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxRightPower_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxRightPower.CheckState == CheckState.Checked);

            RFP_line.IsVisible = bVisible;
            RFP_line.Label.IsVisible = bVisible;

            RDP_line.IsVisible = bVisible;
            RDP_line.Label.IsVisible = bVisible;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void checkBoxRightDistance_CheckedChanged(object sender, EventArgs e)
        {
            bool bVisible = (checkBoxRightDistance.CheckState == CheckState.Checked);

            RFD_line.IsVisible = bVisible;
            RFD_line.Label.IsVisible = bVisible;

            RDD_line.IsVisible = bVisible;
            RDD_line.Label.IsVisible = bVisible;

            if (bDataInitialized)
            {
                vUpdateGraphs(sensDataGridView.CurrentCell.RowIndex);
            }

            return;
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            //
            // For MouseDash development/debug purposes, hardwire a button to
            // write data into a .csv file
            //
            string sFileMM = @"C:\Depot\Source\ZVII\StepMM.txt";
            string sFilems = @"C:\Depot\Source\ZVII\Stepms.txt";

            vWriteMMFile(sFileMM);
//            vWritemsFile(sFilems);
        }

        private void vWriteMMFile(String sFileMM)
        {
            if (File.Exists(sFileMM))
            {
                File.Delete(sFileMM);
            }

            var csv = new StringBuilder();

            var newLine = string.Format("mm, LFD, LFP, RFD, RFP, LDD, LDP, RDD, RDP");

            csv.AppendLine(newLine);

            foreach (var item in DGVStringList)
            {
                double num;

                if (double.TryParse(item.smm, out num))
                {
                    newLine = item.smm + ", "
                        + item.sLFD + ", "
                        + item.sLFP + ", "
                        + item.sRFD + ", "
                        + item.sRFP + ", "
                        + item.sLDD + ", "
                        + item.sLDP + ", "
                        + item.sRDD + ", "
                        + item.sRDP;

                    csv.AppendLine(newLine);
                }
            }

            File.WriteAllText(sFileMM, csv.ToString());

            return;
        }

        private void vWritemsFile(String sFilems)
        {
            MessageBox.Show("Error: Write code for vWritemsFile()");

            return;
        }


    }
}

public static class ExtensionMethods
{
    public static void DoubleBuffered(this DataGridView dgv, bool setting)
    {
        Type dgvType = dgv.GetType();
        PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(dgv, setting, null);

        return;
    }
}

