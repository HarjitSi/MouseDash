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

namespace MouseDashNameSpace
{
    partial class MouseDash
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zedGraphControlPane1 = new ZedGraph.ZedGraphControl();
            this.zedGraphControlPane2 = new ZedGraph.ZedGraphControl();
            this.StatusBox = new System.Windows.Forms.RichTextBox();
            this.openLogFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.sensDataGridView = new System.Windows.Forms.DataGridView();
            this.buttonLoadFile = new System.Windows.Forms.Button();
            this.NumPointsToGraph = new System.Windows.Forms.NumericUpDown();
            this.checkBoxRightDistance = new System.Windows.Forms.CheckBox();
            this.checkBoxRightPower = new System.Windows.Forms.CheckBox();
            this.checkBoxLeftDistance = new System.Windows.Forms.CheckBox();
            this.checkBoxLeftPower = new System.Windows.Forms.CheckBox();
            this.buttonDebug = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonSerial = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.textBoxAngleDot = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.textBoxGridIndex = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIndex = new System.Windows.Forms.TextBox();
            this.labelIndex = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.textBoxOffsetDot = new System.Windows.Forms.TextBox();
            this.textBoxOffset = new System.Windows.Forms.TextBox();
            this.MouseStateToCenter = new System.Windows.Forms.CheckBox();
            this.MouseStateTouched = new System.Windows.Forms.CheckBox();
            this.textBoxAngle = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.textBoxSysFlags = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.textBoxDiagState = new System.Windows.Forms.TextBox();
            this.MouseStateCurXY = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.MouseStateOrient = new System.Windows.Forms.TextBox();
            this.textBoxMotionState = new System.Windows.Forms.TextBox();
            this.MouseStateMode = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.textBoxVbat = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.progressTextBox = new System.Windows.Forms.TextBox();
            this.runGridView = new System.Windows.Forms.DataGridView();
            this.mazeControl = new MouseControls.MazeControl();
            this.zedGraphControlPane3 = new ZedGraph.ZedGraphControl();
            this.zedGraphControlPane4 = new ZedGraph.ZedGraphControl();
            this.mouseAngleGauge = new MouseDashNameSpace.AGauge();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sensDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumPointsToGraph)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.runGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1877, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadLogToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadLogToolStripMenuItem
            // 
            this.loadLogToolStripMenuItem.Name = "loadLogToolStripMenuItem";
            this.loadLogToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.loadLogToolStripMenuItem.Text = "Load Log";
            this.loadLogToolStripMenuItem.Click += new System.EventHandler(this.loadLogToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.saveToolStripMenuItem.Text = "Save Configuration";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // zedGraphControlPane1
            // 
            this.zedGraphControlPane1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zedGraphControlPane1.Location = new System.Drawing.Point(238, 28);
            this.zedGraphControlPane1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zedGraphControlPane1.Name = "zedGraphControlPane1";
            this.zedGraphControlPane1.ScrollGrace = 0D;
            this.zedGraphControlPane1.ScrollMaxX = 0D;
            this.zedGraphControlPane1.ScrollMaxY = 0D;
            this.zedGraphControlPane1.ScrollMaxY2 = 0D;
            this.zedGraphControlPane1.ScrollMinX = 0D;
            this.zedGraphControlPane1.ScrollMinY = 0D;
            this.zedGraphControlPane1.ScrollMinY2 = 0D;
            this.zedGraphControlPane1.Size = new System.Drawing.Size(820, 215);
            this.zedGraphControlPane1.TabIndex = 4;
            // 
            // zedGraphControlPane2
            // 
            this.zedGraphControlPane2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zedGraphControlPane2.Location = new System.Drawing.Point(238, 243);
            this.zedGraphControlPane2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zedGraphControlPane2.Name = "zedGraphControlPane2";
            this.zedGraphControlPane2.ScrollGrace = 0D;
            this.zedGraphControlPane2.ScrollMaxX = 0D;
            this.zedGraphControlPane2.ScrollMaxY = 0D;
            this.zedGraphControlPane2.ScrollMaxY2 = 0D;
            this.zedGraphControlPane2.ScrollMinX = 0D;
            this.zedGraphControlPane2.ScrollMinY = 0D;
            this.zedGraphControlPane2.ScrollMinY2 = 0D;
            this.zedGraphControlPane2.Size = new System.Drawing.Size(820, 215);
            this.zedGraphControlPane2.TabIndex = 10;
            // 
            // StatusBox
            // 
            this.StatusBox.Location = new System.Drawing.Point(1563, 27);
            this.StatusBox.Name = "StatusBox";
            this.StatusBox.ReadOnly = true;
            this.StatusBox.Size = new System.Drawing.Size(310, 500);
            this.StatusBox.TabIndex = 12;
            this.StatusBox.Text = "";
            // 
            // openLogFileDialog
            // 
            this.openLogFileDialog.DefaultExt = "bin";
            this.openLogFileDialog.Filter = "Log Files (*.bin)|*.bin|All files (*.*)|*.*";
            this.openLogFileDialog.Title = "Load Micromouse Log Files";
            // 
            // sensDataGridView
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.sensDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.sensDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.sensDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.sensDataGridView.Location = new System.Drawing.Point(1058, 532);
            this.sensDataGridView.Name = "sensDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.sensDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.sensDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sensDataGridView.Size = new System.Drawing.Size(815, 356);
            this.sensDataGridView.TabIndex = 53;
            this.sensDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.sensDataGridView_CellFormatting);
            this.sensDataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.sensDataGridView_CellValueNeeded);
            this.sensDataGridView.CurrentCellChanged += new System.EventHandler(this.sensDataGridView_CurrentCellChanged);
            this.sensDataGridView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sensDataGridView_Scroll);
            // 
            // buttonLoadFile
            // 
            this.buttonLoadFile.Location = new System.Drawing.Point(6, 27);
            this.buttonLoadFile.Name = "buttonLoadFile";
            this.buttonLoadFile.Size = new System.Drawing.Size(58, 23);
            this.buttonLoadFile.TabIndex = 5;
            this.buttonLoadFile.Text = "Load";
            this.buttonLoadFile.UseVisualStyleBackColor = true;
            this.buttonLoadFile.Click += new System.EventHandler(this.buttonLoadFile_Click);
            // 
            // NumPointsToGraph
            // 
            this.NumPointsToGraph.Increment = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.NumPointsToGraph.Location = new System.Drawing.Point(49, 306);
            this.NumPointsToGraph.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumPointsToGraph.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.NumPointsToGraph.Name = "NumPointsToGraph";
            this.NumPointsToGraph.Size = new System.Drawing.Size(63, 20);
            this.NumPointsToGraph.TabIndex = 0;
            this.NumPointsToGraph.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumPointsToGraph.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.NumPointsToGraph.ValueChanged += new System.EventHandler(this.NumPointsToGraph_ValueChanged);
            // 
            // checkBoxRightDistance
            // 
            this.checkBoxRightDistance.AutoSize = true;
            this.checkBoxRightDistance.Checked = true;
            this.checkBoxRightDistance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRightDistance.Location = new System.Drawing.Point(117, 125);
            this.checkBoxRightDistance.Name = "checkBoxRightDistance";
            this.checkBoxRightDistance.Size = new System.Drawing.Size(96, 17);
            this.checkBoxRightDistance.TabIndex = 0;
            this.checkBoxRightDistance.Text = "Right Distance";
            this.checkBoxRightDistance.UseVisualStyleBackColor = true;
            this.checkBoxRightDistance.CheckedChanged += new System.EventHandler(this.checkBoxRightDistance_CheckedChanged);
            // 
            // checkBoxRightPower
            // 
            this.checkBoxRightPower.AutoSize = true;
            this.checkBoxRightPower.Checked = true;
            this.checkBoxRightPower.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRightPower.Location = new System.Drawing.Point(117, 102);
            this.checkBoxRightPower.Name = "checkBoxRightPower";
            this.checkBoxRightPower.Size = new System.Drawing.Size(84, 17);
            this.checkBoxRightPower.TabIndex = 0;
            this.checkBoxRightPower.Text = "Right Power";
            this.checkBoxRightPower.UseVisualStyleBackColor = true;
            this.checkBoxRightPower.CheckedChanged += new System.EventHandler(this.checkBoxRightPower_CheckedChanged);
            // 
            // checkBoxLeftDistance
            // 
            this.checkBoxLeftDistance.AutoSize = true;
            this.checkBoxLeftDistance.Checked = true;
            this.checkBoxLeftDistance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLeftDistance.Location = new System.Drawing.Point(117, 79);
            this.checkBoxLeftDistance.Name = "checkBoxLeftDistance";
            this.checkBoxLeftDistance.Size = new System.Drawing.Size(89, 17);
            this.checkBoxLeftDistance.TabIndex = 0;
            this.checkBoxLeftDistance.Text = "Left Distance";
            this.checkBoxLeftDistance.UseVisualStyleBackColor = true;
            this.checkBoxLeftDistance.CheckedChanged += new System.EventHandler(this.checkBoxLeftDistance_CheckedChanged);
            // 
            // checkBoxLeftPower
            // 
            this.checkBoxLeftPower.AutoSize = true;
            this.checkBoxLeftPower.Checked = true;
            this.checkBoxLeftPower.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLeftPower.Location = new System.Drawing.Point(117, 56);
            this.checkBoxLeftPower.Name = "checkBoxLeftPower";
            this.checkBoxLeftPower.Size = new System.Drawing.Size(77, 17);
            this.checkBoxLeftPower.TabIndex = 0;
            this.checkBoxLeftPower.Text = "Left Power";
            this.checkBoxLeftPower.UseVisualStyleBackColor = true;
            this.checkBoxLeftPower.CheckedChanged += new System.EventHandler(this.checkBoxLeftPower_CheckedChanged);
            // 
            // buttonDebug
            // 
            this.buttonDebug.Location = new System.Drawing.Point(70, 27);
            this.buttonDebug.Name = "buttonDebug";
            this.buttonDebug.Size = new System.Drawing.Size(58, 23);
            this.buttonDebug.TabIndex = 3;
            this.buttonDebug.Text = "Debug";
            this.buttonDebug.UseVisualStyleBackColor = true;
            this.buttonDebug.Click += new System.EventHandler(this.buttonDebug_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonSerial);
            this.groupBox3.Controls.Add(this.buttonExport);
            this.groupBox3.Controls.Add(this.textBoxAngleDot);
            this.groupBox3.Controls.Add(this.label29);
            this.groupBox3.Controls.Add(this.textBoxGridIndex);
            this.groupBox3.Controls.Add(this.NumPointsToGraph);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.buttonLoadFile);
            this.groupBox3.Controls.Add(this.textBoxIndex);
            this.groupBox3.Controls.Add(this.labelIndex);
            this.groupBox3.Controls.Add(this.buttonDebug);
            this.groupBox3.Controls.Add(this.label34);
            this.groupBox3.Controls.Add(this.textBoxOffsetDot);
            this.groupBox3.Controls.Add(this.textBoxOffset);
            this.groupBox3.Controls.Add(this.MouseStateToCenter);
            this.groupBox3.Controls.Add(this.MouseStateTouched);
            this.groupBox3.Controls.Add(this.textBoxAngle);
            this.groupBox3.Controls.Add(this.label35);
            this.groupBox3.Controls.Add(this.label36);
            this.groupBox3.Controls.Add(this.label37);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.textBoxSysFlags);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label33);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.textBoxDiagState);
            this.groupBox3.Controls.Add(this.MouseStateCurXY);
            this.groupBox3.Controls.Add(this.label32);
            this.groupBox3.Controls.Add(this.MouseStateOrient);
            this.groupBox3.Controls.Add(this.textBoxMotionState);
            this.groupBox3.Controls.Add(this.MouseStateMode);
            this.groupBox3.Controls.Add(this.label31);
            this.groupBox3.Controls.Add(this.textBoxVbat);
            this.groupBox3.Controls.Add(this.label30);
            this.groupBox3.Controls.Add(this.checkBoxRightDistance);
            this.groupBox3.Controls.Add(this.checkBoxLeftPower);
            this.groupBox3.Controls.Add(this.checkBoxRightPower);
            this.groupBox3.Controls.Add(this.checkBoxLeftDistance);
            this.groupBox3.Location = new System.Drawing.Point(12, 27);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(222, 339);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "MouseState";
            // 
            // buttonSerial
            // 
            this.buttonSerial.Location = new System.Drawing.Point(134, 27);
            this.buttonSerial.Name = "buttonSerial";
            this.buttonSerial.Size = new System.Drawing.Size(58, 23);
            this.buttonSerial.TabIndex = 60;
            this.buttonSerial.Text = "Serial";
            this.buttonSerial.UseVisualStyleBackColor = true;
            this.buttonSerial.Click += new System.EventHandler(this.buttonSerial_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(6, 56);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(58, 23);
            this.buttonExport.TabIndex = 47;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // textBoxAngleDot
            // 
            this.textBoxAngleDot.Location = new System.Drawing.Point(118, 216);
            this.textBoxAngleDot.Name = "textBoxAngleDot";
            this.textBoxAngleDot.ReadOnly = true;
            this.textBoxAngleDot.Size = new System.Drawing.Size(65, 20);
            this.textBoxAngleDot.TabIndex = 46;
            this.textBoxAngleDot.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 287);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(106, 13);
            this.label29.TabIndex = 14;
            this.label29.Text = "# of Points To Graph";
            this.label29.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxGridIndex
            // 
            this.textBoxGridIndex.Location = new System.Drawing.Point(180, 316);
            this.textBoxGridIndex.Name = "textBoxGridIndex";
            this.textBoxGridIndex.ReadOnly = true;
            this.textBoxGridIndex.Size = new System.Drawing.Size(42, 20);
            this.textBoxGridIndex.TabIndex = 59;
            this.textBoxGridIndex.Text = "0";
            this.textBoxGridIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(129, 319);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 58;
            this.label1.Text = "Grid Index";
            // 
            // textBoxIndex
            // 
            this.textBoxIndex.Location = new System.Drawing.Point(180, 242);
            this.textBoxIndex.Name = "textBoxIndex";
            this.textBoxIndex.ReadOnly = true;
            this.textBoxIndex.Size = new System.Drawing.Size(42, 20);
            this.textBoxIndex.TabIndex = 57;
            this.textBoxIndex.Text = "0";
            this.textBoxIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelIndex
            // 
            this.labelIndex.AutoSize = true;
            this.labelIndex.Location = new System.Drawing.Point(129, 245);
            this.labelIndex.Name = "labelIndex";
            this.labelIndex.Size = new System.Drawing.Size(33, 13);
            this.labelIndex.TabIndex = 56;
            this.labelIndex.Text = "Index";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(6, 219);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(34, 13);
            this.label34.TabIndex = 45;
            this.label34.Text = "Angle";
            // 
            // textBoxOffsetDot
            // 
            this.textBoxOffsetDot.Location = new System.Drawing.Point(118, 193);
            this.textBoxOffsetDot.Name = "textBoxOffsetDot";
            this.textBoxOffsetDot.ReadOnly = true;
            this.textBoxOffsetDot.Size = new System.Drawing.Size(65, 20);
            this.textBoxOffsetDot.TabIndex = 44;
            this.textBoxOffsetDot.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxOffset
            // 
            this.textBoxOffset.Location = new System.Drawing.Point(52, 193);
            this.textBoxOffset.Name = "textBoxOffset";
            this.textBoxOffset.ReadOnly = true;
            this.textBoxOffset.Size = new System.Drawing.Size(65, 20);
            this.textBoxOffset.TabIndex = 43;
            this.textBoxOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MouseStateToCenter
            // 
            this.MouseStateToCenter.AutoCheck = false;
            this.MouseStateToCenter.AutoSize = true;
            this.MouseStateToCenter.Location = new System.Drawing.Point(117, 148);
            this.MouseStateToCenter.Name = "MouseStateToCenter";
            this.MouseStateToCenter.Size = new System.Drawing.Size(70, 17);
            this.MouseStateToCenter.TabIndex = 21;
            this.MouseStateToCenter.Text = "ToCenter";
            this.MouseStateToCenter.UseVisualStyleBackColor = true;
            // 
            // MouseStateTouched
            // 
            this.MouseStateTouched.AutoCheck = false;
            this.MouseStateTouched.AutoSize = true;
            this.MouseStateTouched.Location = new System.Drawing.Point(117, 171);
            this.MouseStateTouched.Name = "MouseStateTouched";
            this.MouseStateTouched.Size = new System.Drawing.Size(69, 17);
            this.MouseStateTouched.TabIndex = 22;
            this.MouseStateTouched.Text = "Touched";
            this.MouseStateTouched.UseVisualStyleBackColor = true;
            // 
            // textBoxAngle
            // 
            this.textBoxAngle.Location = new System.Drawing.Point(52, 216);
            this.textBoxAngle.Name = "textBoxAngle";
            this.textBoxAngle.ReadOnly = true;
            this.textBoxAngle.Size = new System.Drawing.Size(65, 20);
            this.textBoxAngle.TabIndex = 42;
            this.textBoxAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(53, 177);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(35, 13);
            this.label35.TabIndex = 39;
            this.label35.Text = "Offset";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(93, 177);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(24, 13);
            this.label36.TabIndex = 41;
            this.label36.Text = "Dot";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(6, 196);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(44, 13);
            this.label37.TabIndex = 40;
            this.label37.Text = "Position";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(129, 263);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(37, 13);
            this.label18.TabIndex = 13;
            this.label18.Text = "CurXY";
            // 
            // textBoxSysFlags
            // 
            this.textBoxSysFlags.Location = new System.Drawing.Point(52, 151);
            this.textBoxSysFlags.Name = "textBoxSysFlags";
            this.textBoxSysFlags.ReadOnly = true;
            this.textBoxSysFlags.Size = new System.Drawing.Size(42, 20);
            this.textBoxSysFlags.TabIndex = 38;
            this.textBoxSysFlags.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(129, 282);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(35, 13);
            this.label17.TabIndex = 14;
            this.label17.Text = "Orient";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(1, 154);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(49, 13);
            this.label33.TabIndex = 37;
            this.label33.Text = "SysFlags";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(129, 299);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(34, 13);
            this.label16.TabIndex = 12;
            this.label16.Text = "Mode";
            // 
            // textBoxDiagState
            // 
            this.textBoxDiagState.Location = new System.Drawing.Point(52, 125);
            this.textBoxDiagState.Name = "textBoxDiagState";
            this.textBoxDiagState.ReadOnly = true;
            this.textBoxDiagState.Size = new System.Drawing.Size(36, 20);
            this.textBoxDiagState.TabIndex = 36;
            this.textBoxDiagState.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MouseStateCurXY
            // 
            this.MouseStateCurXY.Location = new System.Drawing.Point(180, 260);
            this.MouseStateCurXY.Name = "MouseStateCurXY";
            this.MouseStateCurXY.ReadOnly = true;
            this.MouseStateCurXY.Size = new System.Drawing.Size(42, 20);
            this.MouseStateCurXY.TabIndex = 15;
            this.MouseStateCurXY.Text = "00";
            this.MouseStateCurXY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 128);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(39, 13);
            this.label32.TabIndex = 35;
            this.label32.Text = "DiagSt";
            // 
            // MouseStateOrient
            // 
            this.MouseStateOrient.Location = new System.Drawing.Point(180, 279);
            this.MouseStateOrient.Name = "MouseStateOrient";
            this.MouseStateOrient.ReadOnly = true;
            this.MouseStateOrient.Size = new System.Drawing.Size(42, 20);
            this.MouseStateOrient.TabIndex = 16;
            this.MouseStateOrient.Text = "N";
            this.MouseStateOrient.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxMotionState
            // 
            this.textBoxMotionState.Location = new System.Drawing.Point(52, 99);
            this.textBoxMotionState.Name = "textBoxMotionState";
            this.textBoxMotionState.ReadOnly = true;
            this.textBoxMotionState.Size = new System.Drawing.Size(36, 20);
            this.textBoxMotionState.TabIndex = 34;
            this.textBoxMotionState.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MouseStateMode
            // 
            this.MouseStateMode.Location = new System.Drawing.Point(180, 296);
            this.MouseStateMode.Name = "MouseStateMode";
            this.MouseStateMode.ReadOnly = true;
            this.MouseStateMode.Size = new System.Drawing.Size(42, 20);
            this.MouseStateMode.TabIndex = 17;
            this.MouseStateMode.Text = "Learn";
            this.MouseStateMode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(6, 102);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(49, 13);
            this.label31.TabIndex = 33;
            this.label31.Text = "MotionSt";
            // 
            // textBoxVbat
            // 
            this.textBoxVbat.Location = new System.Drawing.Point(52, 264);
            this.textBoxVbat.Name = "textBoxVbat";
            this.textBoxVbat.ReadOnly = true;
            this.textBoxVbat.Size = new System.Drawing.Size(42, 20);
            this.textBoxVbat.TabIndex = 32;
            this.textBoxVbat.Text = "0.00";
            this.textBoxVbat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(12, 267);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(30, 13);
            this.label30.TabIndex = 31;
            this.label30.Text = "VBat";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(680, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Parse Progress";
            // 
            // progressTextBox
            // 
            this.progressTextBox.Location = new System.Drawing.Point(760, 7);
            this.progressTextBox.Name = "progressTextBox";
            this.progressTextBox.ReadOnly = true;
            this.progressTextBox.Size = new System.Drawing.Size(29, 20);
            this.progressTextBox.TabIndex = 47;
            this.progressTextBox.Text = "0";
            this.progressTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // runGridView
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.runGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.runGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.runGridView.DefaultCellStyle = dataGridViewCellStyle5;
            this.runGridView.Location = new System.Drawing.Point(0, 584);
            this.runGridView.Name = "runGridView";
            this.runGridView.Size = new System.Drawing.Size(237, 304);
            this.runGridView.TabIndex = 60;
            this.runGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.runGridView_CellFormatting);
            this.runGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.runGridView_CellPainting);
            this.runGridView.CurrentCellChanged += new System.EventHandler(this.runGridView_CurrentCellChanged);
            this.runGridView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.runGridView_Scroll);
            // 
            // mazeControl
            // 
            this.mazeControl.BackColor = System.Drawing.Color.Black;
            this.mazeControl.CorrectedNoWallColor = System.Drawing.Color.Green;
            this.mazeControl.CorrectedNoWallFilled = false;
            this.mazeControl.CorrectedWallColor = System.Drawing.Color.Green;
            this.mazeControl.CorrectedWallFilled = true;
            this.mazeControl.FontColor = System.Drawing.Color.White;
            this.mazeControl.Location = new System.Drawing.Point(1058, 28);
            this.mazeControl.MouseColor = System.Drawing.Color.Yellow;
            this.mazeControl.Name = "mazeControl";
            this.mazeControl.NotMappedColor = System.Drawing.Color.IndianRed;
            this.mazeControl.NotMappedFilled = false;
            this.mazeControl.NotMappedNoWallColor = System.Drawing.Color.Red;
            this.mazeControl.NotMappedNoWallFilled = false;
            this.mazeControl.NotMappedWallColor = System.Drawing.Color.Red;
            this.mazeControl.NotMappedWallFilled = true;
            this.mazeControl.NoWallColor = System.Drawing.Color.Black;
            this.mazeControl.NoWallFilled = false;
            this.mazeControl.NumberOfColumns = 16;
            this.mazeControl.NumberOfRows = 16;
            this.mazeControl.Path = new System.Drawing.Point[0];
            this.mazeControl.PathColor = System.Drawing.Color.Yellow;
            this.mazeControl.PointEndColor = System.Drawing.Color.Green;
            this.mazeControl.PointExpandColor = System.Drawing.Color.Yellow;
            this.mazeControl.PegColor = System.Drawing.Color.DarkRed;
            this.mazeControl.Size = new System.Drawing.Size(500, 500);
            this.mazeControl.TabIndex = 14;
            this.mazeControl.WallColor = System.Drawing.Color.Red;
            this.mazeControl.WallFilled = true;
            // 
            // zedGraphControlPane3
            // 
            this.zedGraphControlPane3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zedGraphControlPane3.Location = new System.Drawing.Point(238, 458);
            this.zedGraphControlPane3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zedGraphControlPane3.Name = "zedGraphControlPane3";
            this.zedGraphControlPane3.ScrollGrace = 0D;
            this.zedGraphControlPane3.ScrollMaxX = 0D;
            this.zedGraphControlPane3.ScrollMaxY = 0D;
            this.zedGraphControlPane3.ScrollMaxY2 = 0D;
            this.zedGraphControlPane3.ScrollMinX = 0D;
            this.zedGraphControlPane3.ScrollMinY = 0D;
            this.zedGraphControlPane3.ScrollMinY2 = 0D;
            this.zedGraphControlPane3.Size = new System.Drawing.Size(820, 215);
            this.zedGraphControlPane3.TabIndex = 61;
            // 
            // zedGraphControlPane4
            // 
            this.zedGraphControlPane4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zedGraphControlPane4.Location = new System.Drawing.Point(238, 673);
            this.zedGraphControlPane4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zedGraphControlPane4.Name = "zedGraphControlPane4";
            this.zedGraphControlPane4.ScrollGrace = 0D;
            this.zedGraphControlPane4.ScrollMaxX = 0D;
            this.zedGraphControlPane4.ScrollMaxY = 0D;
            this.zedGraphControlPane4.ScrollMaxY2 = 0D;
            this.zedGraphControlPane4.ScrollMinX = 0D;
            this.zedGraphControlPane4.ScrollMinY = 0D;
            this.zedGraphControlPane4.ScrollMinY2 = 0D;
            this.zedGraphControlPane4.Size = new System.Drawing.Size(820, 215);
            this.zedGraphControlPane4.TabIndex = 62;
            // 
            // mouseAngleGauge
            // 
            this.mouseAngleGauge.BaseArcColor = System.Drawing.Color.Gray;
            this.mouseAngleGauge.BaseArcRadius = 70;
            this.mouseAngleGauge.BaseArcStart = 270;
            this.mouseAngleGauge.BaseArcSweep = 360;
            this.mouseAngleGauge.BaseArcWidth = 2;
            this.mouseAngleGauge.Cap_Idx = ((byte)(0));
            this.mouseAngleGauge.CapColors = new System.Drawing.Color[] {
        System.Drawing.Color.Black,
        System.Drawing.Color.Black,
        System.Drawing.Color.Black,
        System.Drawing.Color.Black,
        System.Drawing.Color.Black};
            this.mouseAngleGauge.CapPosition = new System.Drawing.Point(10, 190);
            this.mouseAngleGauge.CapsPosition = new System.Drawing.Point[] {
        new System.Drawing.Point(10, 190),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10)};
            this.mouseAngleGauge.CapsText = new string[] {
        "Mouse Angle",
        "",
        "",
        "",
        ""};
            this.mouseAngleGauge.CapText = "Mouse Angle";
            this.mouseAngleGauge.Center = new System.Drawing.Point(111, 100);
            this.mouseAngleGauge.Location = new System.Drawing.Point(12, 372);
            this.mouseAngleGauge.MaxValue = 360F;
            this.mouseAngleGauge.MinValue = 0F;
            this.mouseAngleGauge.Name = "mouseAngleGauge";
            this.mouseAngleGauge.NeedleColor1 = MouseDashNameSpace.AGauge.NeedleColorEnum.Gray;
            this.mouseAngleGauge.NeedleColor2 = System.Drawing.Color.DimGray;
            this.mouseAngleGauge.NeedleRadius = 80;
            this.mouseAngleGauge.NeedleType = 0;
            this.mouseAngleGauge.NeedleWidth = 2;
            this.mouseAngleGauge.Range_Idx = ((byte)(0));
            this.mouseAngleGauge.RangeColor = System.Drawing.Color.LightGreen;
            this.mouseAngleGauge.RangeEnabled = false;
            this.mouseAngleGauge.RangeEndValue = 300F;
            this.mouseAngleGauge.RangeInnerRadius = 70;
            this.mouseAngleGauge.RangeOuterRadius = 80;
            this.mouseAngleGauge.RangesColor = new System.Drawing.Color[] {
        System.Drawing.Color.LightGreen,
        System.Drawing.Color.Red,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.mouseAngleGauge.RangesEnabled = new bool[] {
        false,
        false,
        false,
        false,
        false};
            this.mouseAngleGauge.RangesEndValue = new float[] {
        300F,
        400F,
        0F,
        0F,
        0F};
            this.mouseAngleGauge.RangesInnerRadius = new int[] {
        70,
        70,
        70,
        70,
        70};
            this.mouseAngleGauge.RangesOuterRadius = new int[] {
        80,
        80,
        80,
        80,
        80};
            this.mouseAngleGauge.RangesStartValue = new float[] {
        0F,
        300F,
        0F,
        0F,
        0F};
            this.mouseAngleGauge.RangeStartValue = 0F;
            this.mouseAngleGauge.ScaleLinesInterColor = System.Drawing.Color.Black;
            this.mouseAngleGauge.ScaleLinesInterInnerRadius = 73;
            this.mouseAngleGauge.ScaleLinesInterOuterRadius = 80;
            this.mouseAngleGauge.ScaleLinesInterWidth = 1;
            this.mouseAngleGauge.ScaleLinesMajorColor = System.Drawing.Color.Black;
            this.mouseAngleGauge.ScaleLinesMajorInnerRadius = 70;
            this.mouseAngleGauge.ScaleLinesMajorOuterRadius = 80;
            this.mouseAngleGauge.ScaleLinesMajorStepValue = 45F;
            this.mouseAngleGauge.ScaleLinesMajorWidth = 2;
            this.mouseAngleGauge.ScaleLinesMinorColor = System.Drawing.Color.Gray;
            this.mouseAngleGauge.ScaleLinesMinorInnerRadius = 75;
            this.mouseAngleGauge.ScaleLinesMinorNumOf = 9;
            this.mouseAngleGauge.ScaleLinesMinorOuterRadius = 80;
            this.mouseAngleGauge.ScaleLinesMinorWidth = 1;
            this.mouseAngleGauge.ScaleNumbersColor = System.Drawing.Color.Black;
            this.mouseAngleGauge.ScaleNumbersFormat = null;
            this.mouseAngleGauge.ScaleNumbersRadius = 90;
            this.mouseAngleGauge.ScaleNumbersRotation = 0;
            this.mouseAngleGauge.ScaleNumbersStartScaleLine = 2;
            this.mouseAngleGauge.ScaleNumbersStepScaleLines = 1;
            this.mouseAngleGauge.Size = new System.Drawing.Size(222, 206);
            this.mouseAngleGauge.TabIndex = 54;
            this.mouseAngleGauge.Text = "mouseAngle";
            this.mouseAngleGauge.Value = 0F;
            // 
            // MouseDash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1877, 891);
            this.Controls.Add(this.zedGraphControlPane4);
            this.Controls.Add(this.zedGraphControlPane3);
            this.Controls.Add(this.runGridView);
            this.Controls.Add(this.progressTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.sensDataGridView);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.mazeControl);
            this.Controls.Add(this.StatusBox);
            this.Controls.Add(this.zedGraphControlPane2);
            this.Controls.Add(this.zedGraphControlPane1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.mouseAngleGauge);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MouseDash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MouseDash v0.6";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sensDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumPointsToGraph)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.runGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private ZedGraph.ZedGraphControl zedGraphControlPane1;
        private ZedGraph.ZedGraphControl zedGraphControlPane2;
        private System.Windows.Forms.RichTextBox StatusBox;
        private System.Windows.Forms.ToolStripMenuItem loadLogToolStripMenuItem;
        private MouseControls.MazeControl mazeControl;
        private System.Windows.Forms.OpenFileDialog openLogFileDialog;
        private System.Windows.Forms.DataGridView sensDataGridView;
        private System.Windows.Forms.Button buttonLoadFile;
        private System.Windows.Forms.NumericUpDown NumPointsToGraph;
        private System.Windows.Forms.CheckBox checkBoxRightDistance;
        private System.Windows.Forms.CheckBox checkBoxRightPower;
        private System.Windows.Forms.CheckBox checkBoxLeftDistance;
        private System.Windows.Forms.CheckBox checkBoxLeftPower;
        private System.Windows.Forms.Button buttonDebug;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBoxAngleDot;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox textBoxOffsetDot;
        private System.Windows.Forms.TextBox textBoxOffset;
        private System.Windows.Forms.TextBox textBoxAngle;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.TextBox textBoxSysFlags;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox textBoxDiagState;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox textBoxMotionState;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox textBoxVbat;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.CheckBox MouseStateTouched;
        private System.Windows.Forms.CheckBox MouseStateToCenter;
        private System.Windows.Forms.TextBox MouseStateMode;
        private System.Windows.Forms.TextBox MouseStateOrient;
        private System.Windows.Forms.TextBox MouseStateCurXY;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label29;
        private AGauge mouseAngleGauge;
        private System.Windows.Forms.TextBox textBoxIndex;
        private System.Windows.Forms.Label labelIndex;
        private System.Windows.Forms.TextBox textBoxGridIndex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox progressTextBox;
        private System.Windows.Forms.DataGridView runGridView;
        private System.Windows.Forms.Button buttonExport;
        private ZedGraph.ZedGraphControl zedGraphControlPane3;
        private ZedGraph.ZedGraphControl zedGraphControlPane4;
        private System.Windows.Forms.Button buttonSerial;
    }
}

