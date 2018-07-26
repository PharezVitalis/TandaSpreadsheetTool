using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

namespace TandaSpreadsheetTool
{
    public partial class MainForm : Form, INotifiable
    {
        Networker networker;
        RosterManager builder;
        SpreadSheetBuilder sheetBuilder;
        Thread bgThread;
        FormattedRoster[] rosters;
        StylerForm stylerForm;
        SpreadSheetStyle style;

        int notifierCount = 0;
        
        string spreadSheetPath = null;

        public MainForm()
        {
            InitializeComponent();
            //maybe add functionality to autosave styles
            style = SpreadSheetStyle.Default();
          
            networker = new Networker(this);
            sheetBuilder = new SpreadSheetBuilder(this);
            builder = new RosterManager(networker,this);
            
            Directory.CreateDirectory(RosterManager.Path + "Rosters");
          

            stylerForm = new StylerForm(style);
            stylerForm.Hide();


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
            builder.LoadRosters();
            
            Invoke(new MethodInvoker(UpdateRosterList));
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
            if (rosters != null)
            {
                if (rosters.Length > 0)
                {
                    btnMakeExcel.Enabled = true;
                }
            }
            
        }

        void UpdateRosterList()
        {
            rosters = builder.GetAllRosters();
            if (rosters == null)
            {
                return;
            }
            lstBxRosters.Items.Clear();

            for (int i = 0; i < rosters.Length; i++)
            {
                var roster = rosters[i];

                var itemStr = roster.start.ToShortDateString() + " to " + roster.finish.ToShortDateString();
                lstBxRosters.Items.Add(itemStr);
            }
            if (rosters.Length > 0)
            {
                btnMakeExcel.Enabled = true;
                btnRemove.Enabled = true;
               
            }
            else
            {
                btnMakeExcel.Enabled = false;
                btnRemove.Enabled = false;
                return;
            }
            
            lstBxRosters.SelectedIndex = lstBxRosters.Items.Count - 1;

        }

        void BuildExcelSheet(int selectedIndex, bool open = false)
        {
            if (!sheetBuilder.TeamsSet)
            {
                if (builder.Teams.Length < 1)
                {
                    return;
                }
                sheetBuilder.SetTeams(builder.Teams);
            }
         

            if (sheetBuilder.CreateWorkbook(rosters[selectedIndex], style, spreadSheetPath))
            {
                // sucessful

            if (open)
                {
                    System.Diagnostics.Process.Start(@spreadSheetPath);
                }
            }
            else
            {
                // failed
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
            if (!bgThread.IsAlive)
            {
                var dResult = excelSaveDialog.ShowDialog();
                
                if (dResult == DialogResult.OK || dResult == DialogResult.Yes)
                {
                    spreadSheetPath = excelSaveDialog.FileName;

                }
                else
                {
                    return;
                }
                var open = ckBxOpenSpreadsheet.Checked;
                var selectedindex = lstBxRosters.SelectedIndex;
                bgThread = new Thread(()=> BuildExcelSheet(selectedindex,open));
                bgThread.Start();
            }
            else
            {
                MessageBox.Show("Task Aborted: Background processes are still running");
            }

        }

        async private void RefreshStaffList()
        {
            await builder.GetStaff(true);
            await builder.GetTeams(true);
            Invoke(new MethodInvoker(EnableJsonBtn));
        }

        private void btnUpdateStaff_Click(object sender, EventArgs e)
        {
            
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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var dResult = MessageBox.Show("Delete Selected Roster?"
                       , "Delete Roster", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            if(dResult == DialogResult.Yes && lstBxRosters.Items.Count>0)
            {
                builder.Remove(lstBxRosters.SelectedIndex);
                UpdateRosterList();
            }
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            var dResult = stylerForm.ShowDialog();

            if (dResult == DialogResult.OK)
            {
                style = stylerForm.Style;
            }
            return;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            stylerForm.Close();
            stylerForm.Dispose();
        }

        public void EnableNotifiers()
        {
            notifierCount++;
            if (notifierCount > 0)
            {
                if (SynchronizationContext.Current == null)
                {
                    Invoke(new MethodInvoker(EnableNotifiers));
                    notifierCount--;
                    return;
                }

                pgBarMain.Visible = true;
                pgBarMain.Enabled = true;

                lstBxNotifier.Enabled = true;

            }
            
        }

        public void DisableNotifiers()
        {
            notifierCount--;
            if (notifierCount < 1)
            {

                if (SynchronizationContext.Current == null)
                {
                    Invoke(new MethodInvoker((DisableNotifiers)));
                  
                    return;
                }
                pgBarMain.Enabled = false;
                pgBarMain.Visible = false;
                lstBxNotifier.Enabled = false;
                notifierCount = 0;
            }            
        }

        public void UpdateProgress(string progressUpdate, int progress = -1)
        {
           
            if (SynchronizationContext.Current==null)
            {
                Invoke(new MethodInvoker(() => UpdateProgress(progressUpdate, progress)));
                return;
            }
           
            if (progress > -1 & progress < 101)
            {
                pgBarMain.Value = progress;
            }

            if (lstBxNotifier.Items.Count > 4)
            {
                lstBxNotifier.Items.RemoveAt(0);
            }
            lstBxNotifier.Items.Add(progressUpdate);
        }

       

        public void RaiseMessage(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if (SynchronizationContext.Current == null)
            {
                Invoke(new MethodInvoker(() => RaiseMessage(title, message, icon)));
                return;
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        public int ProcessCount
        {
            get
            {
                return notifierCount;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
