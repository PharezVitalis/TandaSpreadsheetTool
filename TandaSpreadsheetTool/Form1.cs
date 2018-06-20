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
    public partial class Form1 : Form, INetworkListener
    {
        Networker networker;

        bool gettingToken;
        

        public Form1()
        {
            InitializeComponent();
            
            
            networker = new Networker();
           
        }

        public void NetStatusChanged(NetworkStatus newStatus)
        {
            if (gettingToken)
            {
                switch (newStatus)
                {
                    case NetworkStatus.BUSY:

                        break;

                    case NetworkStatus.IDLE:
                        MessageBox.Show("Connected successfully");

                        pnlLogIn.Enabled = false;
                        pnlLogIn.Visible = false;

                        pnlMain.Visible = true;
                        pnlMain.Enabled = true;

                        txtBxUName.Enabled = true;
                        txtBxPwd.Enabled = true;

                        gettingToken = false;
                        networker.Unsubscribe(this);
                        break;

                    case NetworkStatus.ERROR:

                        MessageBox.Show(networker.LastErrMsg, "Network Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        lblLoad.Text = "";
                        gettingToken = false;

                        btnLogIn.Enabled = true;

                        txtBxUName.Enabled = true;
                        txtBxPwd.Enabled = true;

                        networker.Unsubscribe(this);
                        break;


                }
            }
            else
            {
                switch (newStatus)
                {
                    case NetworkStatus.BUSY:

                        break;

                    case NetworkStatus.IDLE:
                        MessageBox.Show("Successfully saved a file to : "+ AppDomain.CurrentDomain.BaseDirectory);

                        btnTest.Enabled = true;



                        networker.Unsubscribe(this);
                        break;

                    case NetworkStatus.ERROR:

                        MessageBox.Show(networker.LastErrMsg, "Network Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        lblLoad.Text = "";
                        gettingToken = false;

                        btnTest.Enabled = true;
                        btnLogIn.Enabled = true;

                        networker.Unsubscribe(this);
                        break;
                }
            }
        }


        private void btnLogIn_Click(object sender, EventArgs e)
        {
            lblLoad.Text = "Loading";
            gettingToken = true;

            networker.Subscribe(this);
            networker.GetToken(txtBxUName.Text, txtBxPwd.Text);

            txtBxPwd.Text = "";
            btnLogIn.Enabled = false;
            txtBxUName.Enabled = false;
            txtBxPwd.Enabled = false;
            


        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            networker.Subscribe(this);
            networker.GetRooster(txtBxDate.Text);
            btnTest.Enabled = false;
        }
    }
}
