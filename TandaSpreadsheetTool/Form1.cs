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
                        lblLoad.Text = "Connected Successfullly";

                        pnlLogIn.Enabled = false;
                        pnlLogIn.Visible = false;
                        
                        pnlMain.Visible = true;
                        pnlMain.Enabled = true;

                        break;

                    case NetworkStatus.ERROR:

                        MessageBox.Show("Failed to Get Authentication", "Failed to Connect", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        lblLoad.Text = "";
                        gettingToken = false;

                        btnLogIn.Enabled = true;

                        networker.Unsubscribe(this);
                        break;

                    
                }
            }
            else
            {

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
            

            


        }

        
    }
}
