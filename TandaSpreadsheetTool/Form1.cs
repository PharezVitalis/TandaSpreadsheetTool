using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TandaSpreadsheetTool
{
    public partial class Form1 : Form
    {
        Networker networker;

        int timeoutRemaining;

        public Form1()
        {
            InitializeComponent();
            
            networker = new Networker();
           
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            lblLoad.Text = "Loading";
            networker.Connect(txtBxUName.Text, txtBxPwd.Text);
            txtBxPwd.Text = "";
            btnLogIn.Enabled = false;


            tNetPoller.Tick += new EventHandler(CheckConnection);
            timeoutRemaining = 15000;
            tNetPoller.Start();


        }

        private void CheckConnection(object sender, EventArgs e)
        {
            

            if (networker.Connected)
            {
                tNetPoller.Stop();

                pnlLogIn.Enabled = false;
                pnlLogIn.Visible = false;

                pnlMain.Enabled = true;
                pnlMain.Visible = true;
            }

            timeoutRemaining -= tNetPoller.Interval;

            if (timeoutRemaining <= 0)
            {
                MessageBox.Show("Connection Timed out", "Timed Out", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
