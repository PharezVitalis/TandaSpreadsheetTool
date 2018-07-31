using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TandaSpreadsheetTool
{
    /// <summary>
    /// Main Form
    /// </summary>
    public partial class MainForm : Form, INotifiable
    {
        /// <summary>
        /// networker instance (there should only ever be one)
        /// </summary>
        Networker networker;
        /// <summary>
        /// the Roster object building instance
        /// </summary>
        RosterManager builder;
        /// <summary>
        /// The spreadsheet building instance
        /// </summary>
        SpreadSheetBuilder sheetBuilder;

        /// <summary>
        /// The background worker thread (does all large operations)
        /// </summary>
        Thread bgThread;

        /// <summary>
        /// The currently formatted rosters
        /// </summary>
        FormattedRoster[] rosters;
        /// <summary>
        /// Instance of the form that handles spreadsheet styles
        /// </summary>
        StylerForm stylerForm;
        /// <summary>
        /// The style struct which sets cell style values in the spreadsheet
        /// </summary>
        SpreadSheetStyle style;

        /// <summary>
        /// The current number of notifiers (instances that can update progress to the form)
        /// </summary>
        int notifierCount = 0;
        
        /// <summary>
        /// The path where the spreadsheet will be saved
        /// </summary>
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

            lblLastUpdated.Text = "Last Updated: " + builder.LastStaffUpdate;
            
            dtPFrom.Value = DateTime.Now.AddDays(-7);
            dtPTo.Value = DateTime.Now;

            AcceptButton = btnLogIn;
           
        }
#region Non-Auto Methods

        /// <summary>
        /// shows the main panel
        /// </summary>
        void ShowMainPanel()
        {
            pnlLogIn.Visible = false;
            pnlMain.Visible = true;
        }

        /// <summary>
        /// shows the login panel
        /// </summary>
        void ShowLogInPanel()
        {
            pnlMain.Visible = false;
            pnlLogIn.Visible = true;
        }

        /// <summary>
        /// Enables the login panel
        /// </summary>
        void EnableLogInPnl()
        {
            pnlLogIn.Enabled = true;
            txtBxPwd.Text = "";
        }

        /// <summary>
        /// Actions to be taken once sucessfully logged in
        /// </summary>
        void LoggedIn()
        {
            pnlLogIn.Visible = false;
            txtBxPwd.Text = "";
            pnlMain.Visible = true;
            AcceptButton = btnGetJSON;
            builder.LoadRosters();
            
            Invoke(new MethodInvoker(UpdateRosterList));
        }

        /// <summary>
        /// resets the username value to the one found in the Networker instance
        /// </summary>
        void SetUserNameToOld()
        {
            txtBxUName.Text = networker.LastUser;
            txtBxPwd.Text = "";
        }

        /// <summary>
        /// Request a log in from the netwworker
        /// </summary>
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
                    var dResult = MessageBox.Show("The username is different from the stored username, only one user may be signed in. Sign previous user out? (This will remove all user data including the rosters)"
                        , "Sign out Old User?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (dResult == DialogResult.Yes)
                    {

                     
                        networker.ClearFileData();
                       
                        builder.Remove();
                        Invoke(new MethodInvoker(UpdateRosterList));

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

        /// <summary>
        /// Makes a Roster object inside of the RosterManager instance  
        /// </summary>
       async void MakeRoster()
        {
            var dateFrom = dtPFrom.Value;
            var dateTo = dtPTo.Value;

            var newRoster = new FormattedRoster();

            try
            {
                 newRoster = await builder.BuildRoster(dateFrom, dateTo, ckBxSaveJson.Checked);
                if (newRoster == null)
                {
                    Invoke(new MethodInvoker(EnableJsonBtn));
                    return;
                }
            }
            catch (ArgumentOutOfRangeException aoe)
            {
                Console.WriteLine(aoe.ActualValue);
                Console.WriteLine(aoe.Message);

            }



          
         Invoke(new MethodInvoker(UpdateRosterList));


         Invoke( new  MethodInvoker(EnableJsonBtn));
            
        }

       /// <summary>
       /// Enables the button that gets rosters
       /// </summary>
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

        /// <summary>
        /// Updates the list box that contains rosters
        /// </summary>
        void UpdateRosterList()
        {
            
            rosters = builder.GetAllRosters();
            lstBxRosters.Items.Clear();
            if (rosters == null |rosters.Length<1)
            {              
                return;
            }
           

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

        /// <summary>
        /// Builds a spreadsheet from a roster object
        /// </summary>
        /// <param name="selectedIndex">Index value of Roster to be built from array</param>
        /// <param name="open">Whether the excel sheet will be open on complete</param>
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

        /// <summary>
        /// Refreshes the Staff List
        /// </summary>
        async private void RefreshStaffList()
        {
            await builder.GetStaff(true);
            await builder.GetTeams(true);
            Invoke(new MethodInvoker(EnableJsonBtn));
        }

        /// <summary>
        /// Increments notifier count, enables notifier form elements
        /// </summary>
        public void NewNotifier()
        {
            notifierCount++;
            if (notifierCount > 0)
            {
                if (SynchronizationContext.Current == null)
                {
                    Invoke(new MethodInvoker(NewNotifier));
                    notifierCount--;
                    return;
                }

                pgBarMain.Visible = true;
                pgBarMain.Enabled = true;

                lstBxNotifier.Enabled = true;

            }

        }

        /// <summary>
        /// Decrements the notifier count, disables the form elements if the count is 0
        /// </summary>
        public void RemoveNotifier()
        {
            notifierCount--;
            if (notifierCount < 1)
            {

                if (SynchronizationContext.Current == null)
                {
                    Invoke(new MethodInvoker((RemoveNotifier)));

                    return;
                }
                pgBarMain.Enabled = false;
                pgBarMain.Visible = false;
                lstBxNotifier.Enabled = false;
                notifierCount = 0;
            }
        }

        /// <summary>
        /// Updates the form notifcation listbox and changes a progress  value if it is valid
        /// </summary>
        /// <param name="progressUpdate">Description of what has changed / been processed</param>
        /// <param name="progress">% value for the progressbar</param>
        public void UpdateProgress(string progressUpdate, int progress = -1)
        {

            if (SynchronizationContext.Current == null)
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

        /// <summary>
        /// Opens a new Message Dialogue 
        /// </summary>
        /// <param name="title">The title of the message (Caption at the top)</param>
        /// <param name="message">The contents of the message</param>
        /// <param name="icon">The icon to be used within the message</param>
        public void RaiseMessage(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if (SynchronizationContext.Current == null)
            {
                Invoke(new MethodInvoker(() => RaiseMessage(title, message, icon)));
                return;
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        /// <summary>
        /// Number of Notifiers that currently exist
        /// </summary>
        public int NotifiersCount
        {
            get
            {
                return notifierCount;
            }
        }
        #endregion

#region Event Handlers
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
                bgThread = new Thread(() => BuildExcelSheet(selectedindex, open));
                bgThread.Start();
            }
            else
            {
                MessageBox.Show("Task Aborted: Background processes are still running");
            }

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void dtPFrom_ValueChanged(object sender, EventArgs e)
        {
            dtPTo.MinDate = dtPFrom.Value;
        }

        private void dtPTo_ValueChanged(object sender, EventArgs e)
        {
            dtPFrom.MaxDate = dtPTo.Value;
        }
#endregion
    }
}
