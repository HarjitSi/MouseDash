namespace MouseDashNameSpace
{
    partial class SerialUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.buttonEraseData = new System.Windows.Forms.Button();
            this.buttonEraseChip = new System.Windows.Forms.Button();
            this.listBoxCOMPort = new System.Windows.Forms.ListBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelError = new System.Windows.Forms.Label();
            this.textBoxError = new System.Windows.Forms.TextBox();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.textBoxReadBufLen = new System.Windows.Forms.TextBox();
            this.textBoxWriteBufLen = new System.Windows.Forms.TextBox();
            this.textBoxRate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "COM Port";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(96, 13);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(100, 23);
            this.buttonConnect.TabIndex = 2;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_click);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Enabled = false;
            this.buttonDownload.Location = new System.Drawing.Point(96, 42);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(100, 23);
            this.buttonDownload.TabIndex = 3;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // buttonEraseData
            // 
            this.buttonEraseData.Enabled = false;
            this.buttonEraseData.Location = new System.Drawing.Point(96, 71);
            this.buttonEraseData.Name = "buttonEraseData";
            this.buttonEraseData.Size = new System.Drawing.Size(100, 23);
            this.buttonEraseData.TabIndex = 4;
            this.buttonEraseData.Text = "Erase Data";
            this.buttonEraseData.UseVisualStyleBackColor = true;
            this.buttonEraseData.Click += new System.EventHandler(this.buttonEraseData_Click);
            // 
            // buttonEraseChip
            // 
            this.buttonEraseChip.Enabled = false;
            this.buttonEraseChip.Location = new System.Drawing.Point(96, 100);
            this.buttonEraseChip.Name = "buttonEraseChip";
            this.buttonEraseChip.Size = new System.Drawing.Size(100, 23);
            this.buttonEraseChip.TabIndex = 5;
            this.buttonEraseChip.Text = "Erase Chip";
            this.buttonEraseChip.UseVisualStyleBackColor = true;
            this.buttonEraseChip.Click += new System.EventHandler(this.buttonEraseChip_Click);
            // 
            // listBoxCOMPort
            // 
            this.listBoxCOMPort.FormattingEnabled = true;
            this.listBoxCOMPort.Location = new System.Drawing.Point(13, 30);
            this.listBoxCOMPort.Name = "listBoxCOMPort";
            this.listBoxCOMPort.Size = new System.Drawing.Size(77, 82);
            this.listBoxCOMPort.TabIndex = 6;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(10, 153);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(37, 13);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "Status";
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Location = new System.Drawing.Point(10, 134);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(29, 13);
            this.labelError.TabIndex = 8;
            this.labelError.Text = "Error";
            // 
            // textBoxError
            // 
            this.textBoxError.Location = new System.Drawing.Point(49, 131);
            this.textBoxError.Name = "textBoxError";
            this.textBoxError.ReadOnly = true;
            this.textBoxError.Size = new System.Drawing.Size(68, 20);
            this.textBoxError.TabIndex = 9;
            this.textBoxError.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(49, 150);
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.Size = new System.Drawing.Size(68, 20);
            this.textBoxStatus.TabIndex = 10;
            this.textBoxStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxReadBufLen
            // 
            this.textBoxReadBufLen.Location = new System.Drawing.Point(123, 150);
            this.textBoxReadBufLen.Name = "textBoxReadBufLen";
            this.textBoxReadBufLen.ReadOnly = true;
            this.textBoxReadBufLen.Size = new System.Drawing.Size(68, 20);
            this.textBoxReadBufLen.TabIndex = 11;
            this.textBoxReadBufLen.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxWriteBufLen
            // 
            this.textBoxWriteBufLen.Location = new System.Drawing.Point(123, 131);
            this.textBoxWriteBufLen.Name = "textBoxWriteBufLen";
            this.textBoxWriteBufLen.ReadOnly = true;
            this.textBoxWriteBufLen.Size = new System.Drawing.Size(68, 20);
            this.textBoxWriteBufLen.TabIndex = 12;
            this.textBoxWriteBufLen.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxRate
            // 
            this.textBoxRate.Location = new System.Drawing.Point(49, 169);
            this.textBoxRate.Name = "textBoxRate";
            this.textBoxRate.ReadOnly = true;
            this.textBoxRate.Size = new System.Drawing.Size(68, 20);
            this.textBoxRate.TabIndex = 14;
            this.textBoxRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 172);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Rate";
            // 
            // SerialUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(210, 197);
            this.Controls.Add(this.textBoxRate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxWriteBufLen);
            this.Controls.Add(this.textBoxReadBufLen);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.textBoxError);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listBoxCOMPort);
            this.Controls.Add(this.buttonEraseChip);
            this.Controls.Add(this.buttonEraseData);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SerialUI";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "SerialUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formSerialUI_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Button buttonEraseData;
        private System.Windows.Forms.Button buttonEraseChip;
        private System.Windows.Forms.ListBox listBoxCOMPort;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelError;
        private System.Windows.Forms.TextBox textBoxError;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.TextBox textBoxReadBufLen;
        private System.Windows.Forms.TextBox textBoxWriteBufLen;
        private System.Windows.Forms.TextBox textBoxRate;
        private System.Windows.Forms.Label label2;
    }
}