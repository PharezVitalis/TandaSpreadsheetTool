using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

namespace TandaSpreadsheetTool
{
    public partial class Form1 : Form
    {
        Networker networker;
        RosterBuilder builder;
        SpreadSheetBuilder sheetBuilder;
        Thread bgThread;
       FormattedRoster roster;

      

        public Form1()
        {
            InitializeComponent();

            
            networker = new Networker();

            Directory.CreateDirectory(RosterBuilder.Path);
            
            sheetBuilder = new SpreadSheetBuilder();
            networker.LoadUsername();

            if(networker.LastUser != "")
            {
                txtBxUName.Text = networker.LastUser;
            }


            dtPFrom.Value = DateTime.Now.AddDays(-7);
            dtPTo.Value = DateTime.Now;

            AcceptButton = btnLogIn;
           
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

        void EnableLogInPnl()
        {
            pnlLogIn.Enabled = true;
            txtBxPwd.Text = "";
        }
        void LoggedIn()

        {
            pnlLogIn.Visible = false;
            txtBxPwd.Text = "";
            pnlMain.Visible = true;
            AcceptButton = btnSaveJSON;
        }
        void SetUserNameToOld()
        {
            txtBxUName.Text = networker.LastUser;
            txtBxPwd.Text = "";
        }

        async  void LogIn()
        {
            
            if (networker.LastUser != "")
            {
                if (object.Equals(txtBxUName.Text, networker.LastUser))
                {
                    if (networker.SignIn(txtBxPwd.Text))
                    {
                        Invoke(new MethodInvoker(LoggedIn));
                        builder = new RosterBuilder(networker, this);
                      
                    }
                    else
                    {
                        MessageBox.Show("Failed to Authenticate", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        Invoke(new MethodInvoker(EnableLogInPnl));
                    }

                }
                else
                {
                    var dResult = MessageBox.Show("The username is different from the stored username, only one user may be signed in. Sign previous user out?"
                        , "Sign out Old User?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (dResult == DialogResult.Yes)
                    {

                     
                        networker.ClearFileData();
                        

                      var gotToken = await  networker.GetToken(txtBxUName.Text, txtBxPwd.Text);

                        if (!gotToken)
                        {
                            Invoke(new MethodInvoker(EnableLogInPnl));
                            MessageBox.Show("Log In failed: " + networker.LastNetErrMsg, "Log In Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            builder = new RosterBuilder(networker, this);
                            Invoke(new MethodInvoker(LoggedIn));
                        }
                    }
                    else
                    {
                        Invoke(new MethodInvoker(EnableLogInPnl));
                        Invoke(new MethodInvoker(SetUserNameToOld));

                    }
                }
            }
            else
            {
                var gotToken = await networker.GetToken(txtBxUName.Text, txtBxPwd.Text);

                if (!gotToken)
                {
                    Invoke(new MethodInvoker(EnableLogInPnl));
                    MessageBox.Show("Log In failed: " + networker.LastNetErrMsg, "Log In Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    builder = new RosterBuilder(networker, this);
                    Invoke(new MethodInvoker(LoggedIn));
                }


            }

          

           
        }

       async void MakeRoster()
        {
            var dateFrom = dtPFrom.Value;
            var dateTo = dtPTo.Value;

            var newRoster = new FormattedRoster();

            try
            {
                 newRoster = await builder.BuildRoster(dateFrom, dateTo);
            }
            catch (ArgumentOutOfRangeException aoe)
            {
                Console.WriteLine(aoe.ActualValue);
                Console.WriteLine(aoe.Message);

            }


            roster = newRoster;
          
         Invoke( new  MethodInvoker(EnableJsonBtn));
        }

        void EnableJsonBtn()
        {
            btnSaveJSON.Enabled = true;
            btnUpdateStaff.Enabled = true;
            if (roster != null)
            {
                btnOpenExcel.Enabled = true;
            }
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
         
            bgThread = new Thread(() => LogIn());
            pnlLogIn.Enabled = false;
            bgThread.Start();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (builder==null)
            {
                builder = new RosterBuilder(networker, this);
               
                
            }

            btnSaveJSON.Enabled = false;
            btnUpdateStaff.Enabled = false;
            if (!bgThread.IsAlive)
            {
                bgThread = new Thread(new ThreadStart(MakeRoster));
                bgThread.Start();
            }
            else
            {
                MessageBox.Show("Task Aborted: Background processes are still running");
            }

        }

        private void btnOpenExcel_Click(object sender, EventArgs e)
        {
            sheetBuilder.AddRoster(roster);
            sheetBuilder.CreateDocument();
        }

        async private void RefreshStaffList()
        {
            await builder.GetStaff(true);
            await builder.GetTeams(true);
            Invoke(new MethodInvoker(EnableJsonBtn));
        }


        private void btnUpdateStaff_Click(object sender, EventArgs e)
        {
            if (builder == null)
            {
                builder = new RosterBuilder(networker, this);

            }

            btnSaveJSON.Enabled = false;
            btnUpdateStaff.Enabled = false;

            if (!bgThread.IsAlive)
            {
                bgThread = new Thread(new ThreadStart(RefreshStaffList));
                bgThread.Start();
            }
            else
            {
                MessageBox.Show("Task Aborted: Background processes are still running");
            }
        }
    }
}
