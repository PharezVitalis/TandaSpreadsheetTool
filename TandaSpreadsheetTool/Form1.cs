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
        }
    }
}
