namespace TandaSpreadsheetTool
{
    partial class Form1
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
            this.lblLoad = new System.Windows.Forms.Label();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.txtBxPwd = new System.Windows.Forms.TextBox();
            this.txtBxUName = new System.Windows.Forms.TextBox();
            this.lblPwd = new System.Windows.Forms.Label();
            this.lblUName = new System.Windows.Forms.Label();
            this.lblToContinue = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.lblDateTo = new System.Windows.Forms.Label();
            this.ckBxOpenFolder = new System.Windows.Forms.CheckBox();
            this.lblInDate = new System.Windows.Forms.Label();
            this.btnSaveJSON = new System.Windows.Forms.Button();
            this.dtPTo = new System.Windows.Forms.DateTimePicker();
            this.dtPFrom = new System.Windows.Forms.DateTimePicker();
            this.pnlLogIn.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLogIn
            // 
            this.pnlLogIn.Controls.Add(this.lblLoad);
            this.pnlLogIn.Controls.Add(this.btnLogIn);
            this.pnlLogIn.Controls.Add(this.txtBxPwd);
            this.pnlLogIn.Controls.Add(this.txtBxUName);
            this.pnlLogIn.Controls.Add(this.lblPwd);
            this.pnlLogIn.Controls.Add(this.lblUName);
            this.pnlLogIn.Controls.Add(this.lblToContinue);
            this.pnlLogIn.Location = new System.Drawing.Point(12, 12);
            this.pnlLogIn.Name = "pnlLogIn";
            this.pnlLogIn.Size = new System.Drawing.Size(538, 142);
            this.pnlLogIn.TabIndex = 0;
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
            this.btnLogIn.Location = new System.Drawing.Point(213, 119);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(75, 23);
            this.btnLogIn.TabIndex = 5;
            this.btnLogIn.Text = "Log In";
            this.btnLogIn.UseVisualStyleBackColor = true;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // txtBxPwd
            // 
            this.txtBxPwd.Location = new System.Drawing.Point(110, 93);
            this.txtBxPwd.Name = "txtBxPwd";
            this.txtBxPwd.Size = new System.Drawing.Size(178, 20);
            this.txtBxPwd.TabIndex = 4;
            this.txtBxPwd.UseSystemPasswordChar = true;
            // 
            // txtBxUName
            // 
            this.txtBxUName.Location = new System.Drawing.Point(110, 59);
            this.txtBxUName.Name = "txtBxUName";
            this.txtBxUName.Size = new System.Drawing.Size(178, 20);
            this.txtBxUName.TabIndex = 3;
            // 
            // lblPwd
            // 
            this.lblPwd.AutoSize = true;
            this.lblPwd.Location = new System.Drawing.Point(51, 96);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(53, 13);
            this.lblPwd.TabIndex = 2;
            this.lblPwd.Text = "Password";
            // 
            // lblUName
            // 
            this.lblUName.AutoSize = true;
            this.lblUName.Location = new System.Drawing.Point(69, 62);
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
            this.pnlMain.Controls.Add(this.dtPFrom);
            this.pnlMain.Controls.Add(this.dtPTo);
            this.pnlMain.Controls.Add(this.lblDateTo);
            this.pnlMain.Controls.Add(this.ckBxOpenFolder);
            this.pnlMain.Controls.Add(this.lblInDate);
            this.pnlMain.Controls.Add(this.btnSaveJSON);
            this.pnlMain.Location = new System.Drawing.Point(12, 12);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(538, 142);
            this.pnlMain.TabIndex = 1;
            this.pnlMain.Visible = false;
            // 
            // lblDateTo
            // 
            this.lblDateTo.AutoSize = true;
            this.lblDateTo.Location = new System.Drawing.Point(14, 73);
            this.lblDateTo.Name = "lblDateTo";
            this.lblDateTo.Size = new System.Drawing.Size(49, 13);
            this.lblDateTo.TabIndex = 6;
            this.lblDateTo.Text = "Date To:";
            // 
            // ckBxOpenFolder
            // 
            this.ckBxOpenFolder.AutoSize = true;
            this.ckBxOpenFolder.Location = new System.Drawing.Point(337, 45);
            this.ckBxOpenFolder.Name = "ckBxOpenFolder";
            this.ckBxOpenFolder.Size = new System.Drawing.Size(169, 17);
            this.ckBxOpenFolder.TabIndex = 3;
            this.ckBxOpenFolder.Text = "Open Folder When Complete?";
            this.ckBxOpenFolder.UseVisualStyleBackColor = true;
            // 
            // lblInDate
            // 
            this.lblInDate.AutoSize = true;
            this.lblInDate.Location = new System.Drawing.Point(7, 23);
            this.lblInDate.Name = "lblInDate";
            this.lblInDate.Size = new System.Drawing.Size(59, 13);
            this.lblInDate.TabIndex = 4;
            this.lblInDate.Text = "From Date:";
            // 
            // btnSaveJSON
            // 
            this.btnSaveJSON.Location = new System.Drawing.Point(378, 96);
            this.btnSaveJSON.Name = "btnSaveJSON";
            this.btnSaveJSON.Size = new System.Drawing.Size(75, 23);
            this.btnSaveJSON.TabIndex = 4;
            this.btnSaveJSON.Text = "Get JSON";
            this.btnSaveJSON.UseVisualStyleBackColor = true;
            this.btnSaveJSON.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // dtPTo
            // 
            this.dtPTo.Location = new System.Drawing.Point(72, 67);
            this.dtPTo.Name = "dtPTo";
            this.dtPTo.Size = new System.Drawing.Size(200, 20);
            this.dtPTo.TabIndex = 2;
            // 
            // dtPFrom
            // 
            this.dtPFrom.Location = new System.Drawing.Point(72, 18);
            this.dtPFrom.Name = "dtPFrom";
            this.dtPFrom.Size = new System.Drawing.Size(200, 20);
            this.dtPFrom.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 173);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlLogIn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Tanda Roster Spreadsheet Tool";
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
        private System.Windows.Forms.Button btnSaveJSON;
        private System.Windows.Forms.Label lblInDate;
        private System.Windows.Forms.CheckBox ckBxOpenFolder;
        private System.Windows.Forms.Label lblDateTo;
        private System.Windows.Forms.DateTimePicker dtPFrom;
        private System.Windows.Forms.DateTimePicker dtPTo;
    }
}

