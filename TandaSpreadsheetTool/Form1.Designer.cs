namespace TandaSpreadsheetTool
{
    partial class MainForm
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
            this.pnlLogIn = new System.Windows.Forms.Panel();
            this.btnClearData = new System.Windows.Forms.Button();
            this.lblLoad = new System.Windows.Forms.Label();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.txtBxPwd = new System.Windows.Forms.TextBox();
            this.txtBxUName = new System.Windows.Forms.TextBox();
            this.lblPwd = new System.Windows.Forms.Label();
            this.lblUName = new System.Windows.Forms.Label();
            this.lblToContinue = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.ckBxOpenSpreadsheet = new System.Windows.Forms.CheckBox();
            this.btnFormat = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lstBxRosters = new System.Windows.Forms.ListBox();
            this.ckBxSaveJson = new System.Windows.Forms.CheckBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnMakeExcel = new System.Windows.Forms.Button();
            this.dtPFrom = new System.Windows.Forms.DateTimePicker();
            this.dtPTo = new System.Windows.Forms.DateTimePicker();
            this.lblDateTo = new System.Windows.Forms.Label();
            this.lblInDate = new System.Windows.Forms.Label();
            this.btnGetJSON = new System.Windows.Forms.Button();
            this.excelSaveDialog = new System.Windows.Forms.SaveFileDialog();
            this.pgBarMain = new System.Windows.Forms.ProgressBar();
            this.btnExit = new System.Windows.Forms.Button();
            this.lstBxNotifier = new System.Windows.Forms.ListBox();
            this.btnSsreference = new System.Windows.Forms.Button();
            this.ckBxRebuildSheets = new System.Windows.Forms.CheckBox();
            this.pnlLogIn.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLogIn
            // 
            this.pnlLogIn.Controls.Add(this.btnClearData);
            this.pnlLogIn.Controls.Add(this.lblLoad);
            this.pnlLogIn.Controls.Add(this.btnLogIn);
            this.pnlLogIn.Controls.Add(this.txtBxPwd);
            this.pnlLogIn.Controls.Add(this.txtBxUName);
            this.pnlLogIn.Controls.Add(this.lblPwd);
            this.pnlLogIn.Controls.Add(this.lblUName);
            this.pnlLogIn.Controls.Add(this.lblToContinue);
            this.pnlLogIn.Location = new System.Drawing.Point(15, 6);
            this.pnlLogIn.Name = "pnlLogIn";
            this.pnlLogIn.Size = new System.Drawing.Size(733, 142);
            this.pnlLogIn.TabIndex = 0;
            // 
            // btnClearData
            // 
            this.btnClearData.Location = new System.Drawing.Point(431, 111);
            this.btnClearData.Name = "btnClearData";
            this.btnClearData.Size = new System.Drawing.Size(75, 23);
            this.btnClearData.TabIndex = 7;
            this.btnClearData.Text = "Clear Data";
            this.btnClearData.UseVisualStyleBackColor = true;
            this.btnClearData.Click += new System.EventHandler(this.btnClearData_Click);
            // 
            // lblLoad
            // 
            this.lblLoad.AutoSize = true;
            this.lblLoad.Location = new System.Drawing.Point(437, 196);
            this.lblLoad.Name = "lblLoad";
            this.lblLoad.Size = new System.Drawing.Size(0, 13);
            this.lblLoad.TabIndex = 6;
            // 
            // btnLogIn
            // 
            this.btnLogIn.Location = new System.Drawing.Point(315, 111);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(75, 23);
            this.btnLogIn.TabIndex = 5;
            this.btnLogIn.Text = "Log In";
            this.btnLogIn.UseVisualStyleBackColor = true;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // txtBxPwd
            // 
            this.txtBxPwd.Location = new System.Drawing.Point(123, 87);
            this.txtBxPwd.Name = "txtBxPwd";
            this.txtBxPwd.Size = new System.Drawing.Size(178, 20);
            this.txtBxPwd.TabIndex = 4;
            this.txtBxPwd.UseSystemPasswordChar = true;
            // 
            // txtBxUName
            // 
            this.txtBxUName.Location = new System.Drawing.Point(123, 53);
            this.txtBxUName.Name = "txtBxUName";
            this.txtBxUName.Size = new System.Drawing.Size(178, 20);
            this.txtBxUName.TabIndex = 3;
            // 
            // lblPwd
            // 
            this.lblPwd.AutoSize = true;
            this.lblPwd.Location = new System.Drawing.Point(64, 90);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(53, 13);
            this.lblPwd.TabIndex = 2;
            this.lblPwd.Text = "Password";
            // 
            // lblUName
            // 
            this.lblUName.AutoSize = true;
            this.lblUName.Location = new System.Drawing.Point(82, 56);
            this.lblUName.Name = "lblUName";
            this.lblUName.Size = new System.Drawing.Size(35, 13);
            this.lblUName.TabIndex = 1;
            this.lblUName.Text = "Email:";
            // 
            // lblToContinue
            // 
            this.lblToContinue.AutoSize = true;
            this.lblToContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToContinue.Location = new System.Drawing.Point(65, 3);
            this.lblToContinue.Name = "lblToContinue";
            this.lblToContinue.Size = new System.Drawing.Size(441, 39);
            this.lblToContinue.TabIndex = 0;
            this.lblToContinue.Text = "Log Into Tanda To Continue";
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.ckBxRebuildSheets);
            this.pnlMain.Controls.Add(this.btnSsreference);
            this.pnlMain.Controls.Add(this.ckBxOpenSpreadsheet);
            this.pnlMain.Controls.Add(this.btnFormat);
            this.pnlMain.Controls.Add(this.btnRemove);
            this.pnlMain.Controls.Add(this.lstBxRosters);
            this.pnlMain.Controls.Add(this.ckBxSaveJson);
            this.pnlMain.Controls.Add(this.btnRefresh);
            this.pnlMain.Controls.Add(this.lblStatus);
            this.pnlMain.Controls.Add(this.btnMakeExcel);
            this.pnlMain.Controls.Add(this.dtPFrom);
            this.pnlMain.Controls.Add(this.dtPTo);
            this.pnlMain.Controls.Add(this.lblDateTo);
            this.pnlMain.Controls.Add(this.lblInDate);
            this.pnlMain.Controls.Add(this.btnGetJSON);
            this.pnlMain.Location = new System.Drawing.Point(15, 9);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(733, 142);
            this.pnlMain.TabIndex = 1;
            this.pnlMain.Visible = false;
            // 
            // ckBxOpenSpreadsheet
            // 
            this.ckBxOpenSpreadsheet.AutoSize = true;
            this.ckBxOpenSpreadsheet.Checked = true;
            this.ckBxOpenSpreadsheet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckBxOpenSpreadsheet.Location = new System.Drawing.Point(546, 6);
            this.ckBxOpenSpreadsheet.Name = "ckBxOpenSpreadsheet";
            this.ckBxOpenSpreadsheet.Size = new System.Drawing.Size(131, 17);
            this.ckBxOpenSpreadsheet.TabIndex = 17;
            this.ckBxOpenSpreadsheet.Text = "Open When Complete";
            this.ckBxOpenSpreadsheet.UseVisualStyleBackColor = true;
            // 
            // btnFormat
            // 
            this.btnFormat.Location = new System.Drawing.Point(546, 80);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(150, 23);
            this.btnFormat.TabIndex = 16;
            this.btnFormat.Text = "Spreadsheet Formatting";
            this.btnFormat.UseVisualStyleBackColor = true;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(546, 111);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(150, 23);
            this.btnRemove.TabIndex = 15;
            this.btnRemove.Text = "Delete Selected";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lstBxRosters
            // 
            this.lstBxRosters.FormattingEnabled = true;
            this.lstBxRosters.Location = new System.Drawing.Point(376, 13);
            this.lstBxRosters.Name = "lstBxRosters";
            this.lstBxRosters.Size = new System.Drawing.Size(164, 121);
            this.lstBxRosters.TabIndex = 14;
            // 
            // ckBxSaveJson
            // 
            this.ckBxSaveJson.AutoSize = true;
            this.ckBxSaveJson.Checked = true;
            this.ckBxSaveJson.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckBxSaveJson.Location = new System.Drawing.Point(20, 86);
            this.ckBxSaveJson.Name = "ckBxSaveJson";
            this.ckBxSaveJson.Size = new System.Drawing.Size(130, 17);
            this.ckBxSaveJson.TabIndex = 13;
            this.ckBxSaveJson.Text = "Save Roster Data File";
            this.ckBxSaveJson.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(205, 111);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(150, 23);
            this.btnRefresh.TabIndex = 12;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnUpdateStaff_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(355, 24);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 11;
            // 
            // btnMakeExcel
            // 
            this.btnMakeExcel.Enabled = false;
            this.btnMakeExcel.Location = new System.Drawing.Point(546, 26);
            this.btnMakeExcel.Name = "btnMakeExcel";
            this.btnMakeExcel.Size = new System.Drawing.Size(150, 23);
            this.btnMakeExcel.TabIndex = 7;
            this.btnMakeExcel.Text = "Create Spreadsheet";
            this.btnMakeExcel.UseVisualStyleBackColor = true;
            this.btnMakeExcel.Click += new System.EventHandler(this.btnOpenExcel_Click);
            // 
            // dtPFrom
            // 
            this.dtPFrom.Location = new System.Drawing.Point(72, 6);
            this.dtPFrom.Name = "dtPFrom";
            this.dtPFrom.Size = new System.Drawing.Size(118, 20);
            this.dtPFrom.TabIndex = 1;
            this.dtPFrom.ValueChanged += new System.EventHandler(this.dtPFrom_ValueChanged);
            // 
            // dtPTo
            // 
            this.dtPTo.Location = new System.Drawing.Point(72, 34);
            this.dtPTo.Name = "dtPTo";
            this.dtPTo.Size = new System.Drawing.Size(118, 20);
            this.dtPTo.TabIndex = 2;
            this.dtPTo.ValueChanged += new System.EventHandler(this.dtPTo_ValueChanged);
            // 
            // lblDateTo
            // 
            this.lblDateTo.AutoSize = true;
            this.lblDateTo.Location = new System.Drawing.Point(17, 40);
            this.lblDateTo.Name = "lblDateTo";
            this.lblDateTo.Size = new System.Drawing.Size(49, 13);
            this.lblDateTo.TabIndex = 6;
            this.lblDateTo.Text = "Date To:";
            // 
            // lblInDate
            // 
            this.lblInDate.AutoSize = true;
            this.lblInDate.Location = new System.Drawing.Point(14, 13);
            this.lblInDate.Name = "lblInDate";
            this.lblInDate.Size = new System.Drawing.Size(59, 13);
            this.lblInDate.TabIndex = 4;
            this.lblInDate.Text = "From Date:";
            // 
            // btnGetJSON
            // 
            this.btnGetJSON.Location = new System.Drawing.Point(20, 111);
            this.btnGetJSON.Name = "btnGetJSON";
            this.btnGetJSON.Size = new System.Drawing.Size(150, 23);
            this.btnGetJSON.TabIndex = 4;
            this.btnGetJSON.Text = "Get Roster (JSON)";
            this.btnGetJSON.UseVisualStyleBackColor = true;
            this.btnGetJSON.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // excelSaveDialog
            // 
            this.excelSaveDialog.Filter = "Excel| *.xlsx";
            // 
            // pgBarMain
            // 
            this.pgBarMain.Enabled = false;
            this.pgBarMain.Location = new System.Drawing.Point(526, 157);
            this.pgBarMain.Name = "pgBarMain";
            this.pgBarMain.Size = new System.Drawing.Size(219, 23);
            this.pgBarMain.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pgBarMain.TabIndex = 3;
            this.pgBarMain.Visible = false;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(595, 202);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(150, 23);
            this.btnExit.TabIndex = 17;
            this.btnExit.Text = "Quit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lstBxNotifier
            // 
            this.lstBxNotifier.BackColor = System.Drawing.SystemColors.Control;
            this.lstBxNotifier.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstBxNotifier.Enabled = false;
            this.lstBxNotifier.FormattingEnabled = true;
            this.lstBxNotifier.Location = new System.Drawing.Point(3, 157);
            this.lstBxNotifier.Name = "lstBxNotifier";
            this.lstBxNotifier.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstBxNotifier.Size = new System.Drawing.Size(517, 78);
            this.lstBxNotifier.TabIndex = 18;
            this.lstBxNotifier.TabStop = false;
            // 
            // btnSsreference
            // 
            this.btnSsreference.Enabled = false;
            this.btnSsreference.Location = new System.Drawing.Point(546, 51);
            this.btnSsreference.Name = "btnSsreference";
            this.btnSsreference.Size = new System.Drawing.Size(150, 23);
            this.btnSsreference.TabIndex = 18;
            this.btnSsreference.Text = "Update Excel reference";
            this.btnSsreference.UseVisualStyleBackColor = true;
            this.btnSsreference.Click += new System.EventHandler(this.btnSsreference_Click);
            // 
            // ckBxRebuildSheets
            // 
            this.ckBxRebuildSheets.AutoSize = true;
            this.ckBxRebuildSheets.Checked = true;
            this.ckBxRebuildSheets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckBxRebuildSheets.Location = new System.Drawing.Point(205, 88);
            this.ckBxRebuildSheets.Name = "ckBxRebuildSheets";
            this.ckBxRebuildSheets.Size = new System.Drawing.Size(150, 17);
            this.ckBxRebuildSheets.TabIndex = 19;
            this.ckBxRebuildSheets.Text = "Regenerate Spreadsheets";
            this.ckBxRebuildSheets.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 248);
            this.Controls.Add(this.lstBxNotifier);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.pgBarMain);
            this.Controls.Add(this.pnlLogIn);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Tanda Roster Spreadsheet Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.pnlLogIn.ResumeLayout(false);
            this.pnlLogIn.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLogIn;
        private System.Windows.Forms.TextBox txtBxPwd;
        private System.Windows.Forms.TextBox txtBxUName;
        private System.Windows.Forms.Label lblPwd;
        private System.Windows.Forms.Label lblUName;
        private System.Windows.Forms.Label lblToContinue;
        private System.Windows.Forms.Button btnLogIn;
        private System.Windows.Forms.Label lblLoad;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Button btnGetJSON;
        private System.Windows.Forms.Label lblInDate;
        private System.Windows.Forms.Label lblDateTo;
        private System.Windows.Forms.DateTimePicker dtPFrom;
        private System.Windows.Forms.DateTimePicker dtPTo;
        private System.Windows.Forms.Button btnMakeExcel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox ckBxSaveJson;
        private System.Windows.Forms.ListBox lstBxRosters;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnFormat;
        private System.Windows.Forms.SaveFileDialog excelSaveDialog;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ListBox lstBxNotifier;
        private System.Windows.Forms.ProgressBar pgBarMain;
        private System.Windows.Forms.CheckBox ckBxOpenSpreadsheet;
        private System.Windows.Forms.Button btnClearData;
        private System.Windows.Forms.Button btnSsreference;
        private System.Windows.Forms.CheckBox ckBxRebuildSheets;
    }
}

