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
        RosterManager builder;
        SpreadSheetBuilder sheetBuilder;
        Thread bgThread;
        FormattedRoster[] rosters;
        
      

        public Form1()
        {
            InitializeComponent();

            
            networker = new Networker();
            sheetBuilder = new SpreadSheetBuilder();
            builder = new RosterManager(networker);
            Directory.CreateDirectory(RosterManager.Path);
            Directory.CreateDirectory(RosterManager.Path + "Rosters");
            builder.LoadRosters();

            

          
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
            AcceptButton = btnGetJSON;
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
                if (Equals(txtBxUName.Text, networker.LastUser))
                {
                    if (networker.LoadToken(txtBxPwd.Text))
                    {
                        Invoke(new MethodInvoker(LoggedIn));
                         
                      
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
                            builder.LoadRosters();

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
                 newRoster = await builder.BuildRoster(dateFrom, dateTo, ckBxSaveJson.Checked);
            }
            catch (ArgumentOutOfRangeException aoe)
            {
                Console.WriteLine(aoe.ActualValue);
                Console.WriteLine(aoe.Message);

            }



            Invoke(new MethodInvoker(UpdateRosterList));


         Invoke( new  MethodInvoker(EnableJsonBtn));
            
        }

        void EnableJsonBtn()
        {
            btnGetJSON.Enabled = true;
            btnUpdateStaff.Enabled = true;
            if (rosters.Length>0)
            {
                btnOpenExcel.Enabled = true;
            }
        }

        void UpdateRosterList()
        {
            rosters = builder.GetAllRosters();
            lstBxRosters.Items.Clear();

            for (int i = 0; i < rosters.Length; i++)
            {
                var roster = rosters[i];

                var itemStr = roster.start.ToShortDateString() + " - " + roster.finish.ToShortDateString();
                lstBxRosters.Items.Add(itemStr);
            }
            lstBxRosters.SelectedIndex = lstBxRosters.Items.Count - 1;
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
                 
               
                
            }

            btnGetJSON.Enabled = false;
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
            //should be done on a seperate thread 
            sheetBuilder.CreateWorkbook(rosters[lstBxRosters.SelectedIndex], SpreadSheetStyle.Default());
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
                 

            }

            btnGetJSON.Enabled = false;
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
