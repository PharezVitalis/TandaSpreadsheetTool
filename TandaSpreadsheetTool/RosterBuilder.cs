using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TandaSpreadsheetTool
{
    class RosterBuilder:INetworkListener
    {
        
        Networker networker;
        Roster rosterObj;
        Form1 form;

        int maxTries;
        int currentTries = 0;

       
        
        List<Team> teamObjs;
        List<User> staffObjs;

        CurrentGet currentGet = CurrentGet.NONE;

      

        int[] staffIds;

        FormattedRoster formRoster;

        public RosterBuilder(Networker networker, Form1 form, int maxTries = 5 )
        {
            this.maxTries = maxTries;

            this.networker = networker;
            this.form = form;

            

        
          
            staffObjs = new List<User>();
            rosterObj = new Roster();
            teamObjs = new List<Team>();

            if (networker.Roster == null)
            {
                return;
            }

           
            rosterObj = JsonConvert.DeserializeObject<Roster>(networker.Roster.ToString());

           
        }

       public void CreateFormattedRoster()
        {
            var lstStaffIds = new List<int>();

            

            for (int i = 0; i < rosterObj.schedules.Count; i++)
            {
                var currentDate = rosterObj.schedules[i];

                for (int j = 0; j < currentDate.schedules.Count; j++)
                {
                    var currentSchedule = currentDate.schedules[j];

                    var currentStaffId = currentSchedule.user_id;

                    if (!lstStaffIds.Contains(currentStaffId))
                    {
                        lstStaffIds.Add(currentStaffId);
                    }
                }
            }

            staffIds = lstStaffIds.ToArray();

            networker.Subscribe(this);
            currentGet = CurrentGet.TEAMS;
            networker.GetDepartments();
        }

        public void SaveRoster()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TandaJson");
            
            Directory.CreateDirectory(path);
            try
            {
                File.WriteAllText(path + "/Roster "+ DateTime.Now.ToString()+".json", JsonConvert.SerializeObject(formRoster).ToString());
                form.FormattingComplete();
            }
            catch
            {
                Console.WriteLine("Failed to Save file");
            }
            
        }

        private void GenerateStaffObjects()
        {
            var staffJArr = networker.Staff;

            if (staffJArr == null)
            {
                return;
            }
            staffObjs.Clear();

            

            for (int i = 0; i < staffJArr.Count; i++)
            {
                staffObjs.Add(JsonConvert.DeserializeObject<User>(staffJArr[i].ToString()));
            }

            

           
        }

       private void BuildRoster()
        {

            
                

                GenerateStaffObjects();



            var teamJArr = networker.Teams;

            for (int i = 0; i <teamJArr.Count ; i++)
            {
                teamObjs.Add(JsonConvert.DeserializeObject<Team>(teamJArr[i].ToString()));
            }

            formRoster = new FormattedRoster();

            formRoster.start = FormatDate(rosterObj.start);
            formRoster.finish = FormatDate(rosterObj.finish);

            for (int i = 0; i < rosterObj.schedules.Count; i++)
            {
                var currentDay = rosterObj.schedules[i];

                for (int j = 0; j < currentDay.schedules.Count; j++)
                {
                    //NRE here
                    formRoster.schedules.Add(GenerateSchedule(currentDay.schedules[j]));

                    
                }
            }

            SaveRoster();
           
        }

        string FormatDate(string date)
        {                  

            return date.Substring(8, 2) + "/" + date.Substring(5, 2) + "/" + date.Substring(0, 4);
        }

        private FormattedSchedule GenerateSchedule(Schedule unformSchedule)
        {
            var outSchedule = new FormattedSchedule();

            for (int i = 0; i < staffObjs.Count; i++)
            {
                if(unformSchedule.user_id == staffObjs[i].id)
                {
                    outSchedule.staff = staffObjs[i].name;
                    break;
                }
            }

            var startTime = new DateTime(unformSchedule.start * 10000000);

            outSchedule.startDate = startTime.Date.ToString("MM dd, yyyy");
            outSchedule.startTime = startTime.ToString("HH:mm");
            if(unformSchedule.finish != null)
            {
                
                outSchedule.endTime = new DateTime((long)unformSchedule.finish * 10000000).ToString("HH:mm");
            }
            

            for (int i = 0; i < teamObjs.Count; i++)
            {
                if (unformSchedule.department_id == teamObjs[i].id)
                {
                    outSchedule.team = teamObjs[i].name;
                    break;
                }
            }

            return outSchedule;
        }

        public void NetStatusChanged(NetworkStatus status)
        {
            switch (currentGet)
            {
                case CurrentGet.TEAMS:
                    
                    if (status == NetworkStatus.IDLE)
                    {
                        if (networker.Teams == null)
                        {
                            currentTries++;
                            if(currentTries >= maxTries)
                            {
                                //failed
                                networker.Unsubscribe(this);
                                currentGet = CurrentGet.NONE;
                            }
                            else
                            {
                                networker.GetDepartments();
                            }
                            
                        }
                        else
                        {
                            currentTries = 0;
                            currentGet = CurrentGet.STAFF;
                            networker.GetStaff();

                        }
                    }
                  else if (status == NetworkStatus.ERROR)
                    {
                        Console.WriteLine("It failes to get the teams: "+networker.LastNetErrMsg);
                    }

                    break;

                case CurrentGet.STAFF:

                    if (status == NetworkStatus.IDLE)
                    {
                        if (networker.Staff == null)
                        {
                            currentTries++;
                            if (currentTries >= maxTries)
                            {
                                //failed
                                networker.Unsubscribe(this);
                                currentGet = CurrentGet.NONE;
                            }
                            else
                            {
                                networker.GetStaff();
                            }
                            
                        }
                        else
                        {
                            networker.Unsubscribe(this);
                            currentTries = 0;
                            BuildRoster();

                        }
                    }
                    else if (status == NetworkStatus.ERROR)
                    {
                        Console.WriteLine("It failes to get the staff ");
                        
                    }
                    break;
            }
        }

       

    }
}
