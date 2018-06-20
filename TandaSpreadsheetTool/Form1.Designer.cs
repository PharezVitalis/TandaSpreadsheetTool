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
            this.components = new System.ComponentModel.Container();
            this.pnlLogIn = new System.Windows.Forms.Panel();
            this.lblLoad = new System.Windows.Forms.Label();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.txtBxPwd = new System.Windows.Forms.TextBox();
            this.txtBxUName = new System.Windows.Forms.TextBox();
            this.lblPwd = new System.Windows.Forms.Label();
            this.lblUName = new System.Windows.Forms.Label();
            this.lblToContinue = new System.Windows.Forms.Label();
            this.tNetPoller = new System.Windows.Forms.Timer(this.components);
            this.pnlMain = new System.Windows.Forms.Panel();
            this.lblTest = new System.Windows.Forms.Label();
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
            this.pnlLogIn.Location = new System.Drawing.Point(22, 9);
            this.pnlLogIn.Name = "pnlLogIn";
            this.pnlLogIn.Size = new System.Drawing.Size(538, 190);
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
            this.btnLogIn.Location = new System.Drawing.Point(295, 147);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(75, 23);
            this.btnLogIn.TabIndex = 5;
            this.btnLogIn.Text = "Log In";
            this.btnLogIn.UseVisualStyleBackColor = true;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // txtBxPwd
            // 
            this.txtBxPwd.Location = new System.Drawing.Point(192, 121);
            this.txtBxPwd.Name = "txtBxPwd";
            this.txtBxPwd.Size = new System.Drawing.Size(178, 20);
            this.txtBxPwd.TabIndex = 4;
            this.txtBxPwd.UseSystemPasswordChar = true;
            // 
            // txtBxUName
            // 
            this.txtBxUName.Location = new System.Drawing.Point(192, 87);
            this.txtBxUName.Name = "txtBxUName";
            this.txtBxUName.Size = new System.Drawing.Size(178, 20);
            this.txtBxUName.TabIndex = 3;
            // 
            // lblPwd
            // 
            this.lblPwd.AutoSize = true;
            this.lblPwd.Location = new System.Drawing.Point(133, 124);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(53, 13);
            this.lblPwd.TabIndex = 2;
            this.lblPwd.Text = "Password";
            // 
            // lblUName
            // 
            this.lblUName.AutoSize = true;
            this.lblUName.Location = new System.Drawing.Point(151, 90);
            this.lblUName.Name = "lblUName";
            this.lblUName.Size = new System.Drawing.Size(35, 13);
            this.lblUName.TabIndex = 1;
            this.lblUName.Text = "Email:";
            // 
            // lblToContinue
            // 
            this.lblToContinue.AutoSize = true;
            this.lblToContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToContinue.Location = new System.Drawing.Point(78, 34);
            this.lblToContinue.Name = "lblToContinue";
            this.lblToContinue.Size = new System.Drawing.Size(441, 39);
            this.lblToContinue.TabIndex = 0;
            this.lblToContinue.Text = "Log Into Tanda To Continue";
            // 
            // tNetPoller
            // 
            this.tNetPoller.Interval = 300;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lblTest);
            this.pnlMain.Enabled = false;
            this.pnlMain.Location = new System.Drawing.Point(642, 12);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(592, 546);
            this.pnlMain.TabIndex = 1;
            this.pnlMain.Visible = false;
            // 
            // lblTest
            // 
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(175, 238);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(54, 13);
            this.lblTest.TabIndex = 0;
            this.lblTest.Text = "It worked!";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1258, 596);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlLogIn);
            this.Name = "Form1";
            this.Text = "Form1";
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
        private System.Windows.Forms.Timer tNetPoller;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblTest;
    }
}

