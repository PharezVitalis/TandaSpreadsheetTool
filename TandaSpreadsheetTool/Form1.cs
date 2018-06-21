using System;

using System.Windows.Forms;

namespace TandaSpreadsheetTool
{
    public partial class Form1 : Form, INetworkListener
    {
        Networker networker;
        RosterBuilder builder;

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
                        if (networker.Rooster != null)
                        {
                            builder = new RosterBuilder(networker.Rooster);
                        }
                        else
                        {
                            MessageBox.Show("Failed to get File");
                        }

                        btnSaveJSON.Enabled = true;



                        networker.Unsubscribe(this);
                        break;

                    case NetworkStatus.ERROR:

                        MessageBox.Show(networker.LastNetErrMsg, "Network Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        lblLoad.Text = "";
                        gettingToken = false;

                        btnSaveJSON.Enabled = true;
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
                
               

                if (object.Equals(txtBxUName.Text,networker.LastUser))
                {
                  if (networker.SignIn(txtBxPwd.Text))
                    {
                        ShowMainPanel();
                    }
                    else
                    {
                        MessageBox.Show("Failed to Authenticate", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

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


                ShowMainPanel();
                networker.Subscribe(this);
                gettingToken = true;
                networker.GetToken(txtBxUName.Text, txtBxPwd.Text);
                
            }


            txtBxPwd.Text = "";


        }

        private void btnTest_Click(object sender, EventArgs e)
        {
           

            if(txtBxDate.Text.Length!=8 & txtBxDate.Text.Length != 10)
            {
                MessageBox.Show("Invalid date use DD-MM-YY format","Invalid Date",MessageBoxButtons.OK,MessageBoxIcon.Warning);               
            }
            else
            {
                var formattedDate = "";

                if (txtBxDate.Text.Length == 10)
                {
                     formattedDate = txtBxDate.Text.Substring(6, 4);
                    formattedDate += "-";
                    formattedDate += txtBxDate.Text.Substring(3, 2);
                    formattedDate += "-";
                    formattedDate += txtBxDate.Text.Substring(0, 2);
                    Console.WriteLine(formattedDate);
                }
                else
                {
                    try
                    {
                        var testInt = Convert.ToInt32(txtBxDate.Text.Substring(6, 1));
                         formattedDate = "";
                        if(testInt == 9)
                        {
                            formattedDate = "19";
                            
                        }
                        else
                        {
                            formattedDate = "20";
                        }
                        formattedDate += txtBxDate.Text.Substring(6, 2);
                        formattedDate += "-";
                        formattedDate += txtBxDate.Text.Substring(3, 2);
                        formattedDate += "-";
                        formattedDate += txtBxDate.Text.Substring(0, 2);
                        Console.WriteLine(formattedDate);
                    }
                    catch 
                    {
                        MessageBox.Show("Invalid date use DD-MM-YY format", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                 networker.Subscribe(this);
                networker.GetRooster(formattedDate);
                btnSaveJSON.Enabled = false;
            }

            


             
        }

        
    }
}
