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

            networker.LoadUsername();

            if(networker.LastUser != "")
            {
                txtBxUName.Text = networker.LastUser;
            }
           
        }

        void ShowMainPanel()
        {
            pnlLogIn.Visible = false;
            pnlMain.Visible = true;
        }

        void ShowLogInPanel()
        {
            pnlMain.Visible = false;
            pnlLogIn.Visible = true;
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

                        ShowMainPanel();

                        txtBxUName.Enabled = true;
                        txtBxPwd.Enabled = true;

                        gettingToken = false;
                        networker.Unsubscribe(this);
                        break;

                    case NetworkStatus.ERROR:

                        MessageBox.Show(networker.LastNetErrMsg, "Network Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

                        MessageBox.Show(networker.LastNetErrMsg, "Network Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            

            
            if (networker.LastUser != "")
            {
                
                if (networker.LastUser == txtBxPwd.Text)
                {
                    networker.SignIn(txtBxPwd.Text);
                }
                else
                {
                    var dResult = MessageBox.Show("The username is different from the stored username, only one user may be signed in. Sign previous user out?"
                        , "Sign out Old User?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (dResult == DialogResult.Yes)
                    {

                        txtBxPwd.Text = "";
                        btnLogIn.Enabled = false;
                        txtBxUName.Enabled = false;
                        txtBxPwd.Enabled = false;
                        networker.ClearFileData();
                        networker.Subscribe(this);
                        gettingToken = true;
                        networker.GetToken(txtBxUName.Text, txtBxPwd.Text);
                    }
                    else 
                    {
                        txtBxUName.Text = networker.LastUser;
                    }
                }
            }
            else
            {

                
                btnLogIn.Enabled = false;
                txtBxUName.Enabled = false;
                txtBxPwd.Enabled = false;
                networker.Subscribe(this);
                gettingToken = true;
                networker.GetToken(txtBxUName.Text, txtBxPwd.Text);
                
            }


            txtBxPwd.Text = "";


        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            networker.Subscribe(this);
            networker.GetRooster(txtBxDate.Text);
            btnTest.Enabled = false;
        }
    }
}
