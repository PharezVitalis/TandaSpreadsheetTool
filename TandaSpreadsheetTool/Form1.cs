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

       

        public Form1()
        {
            InitializeComponent();
            
            networker = new Networker();
            
        }

        public void NetStatusChanged(NetworkStatus newStatus)
        {
            Console.WriteLine("Here is what is happening: " + newStatus.ToString());

            if(newStatus == NetworkStatus.ERROR)
            {
                Console.Write(networker.LastErrMsg);
            }
        }


        private void btnLogIn_Click(object sender, EventArgs e)
        {
            lblLoad.Text = "Loading";

            networker.Subscribe(this);
            networker.GetToken(txtBxUName.Text, txtBxPwd.Text);

            txtBxPwd.Text = "";
            btnLogIn.Enabled = false;


            


        }

        
    }
}
