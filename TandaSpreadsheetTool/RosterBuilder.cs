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

                   

                    
                    if (currentSchedule.user_id != null)
                    {
                        var currentStaffId = Convert.ToInt32(currentSchedule.user_id);

                        if (!lstStaffIds.Contains(currentStaffId))
                        {
                            lstStaffIds.Add(currentStaffId);
                        }
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
                var outText = JsonConvert.SerializeObject(formRoster).ToString();
                

                File.WriteAllText(path + "/Roster " + DateTime.Now.ToString("dd MM yy - hh mm") + ".json", outText);

                form.FormattingComplete();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Save file: " +e.Message+" path:"+path);
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
                    var newSchedule = GenerateSchedule(currentDay.schedules[j]);
                    formRoster.schedules.Add(newSchedule);

                    
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
                if (unformSchedule.user_id != null)
                {
                    var currentUserId = Convert.ToInt32(unformSchedule.user_id);

                    if (currentUserId == staffObjs[i].id)
                    {
                        outSchedule.staff = staffObjs[i].name;
                        break;
                    }
                }
               
            }

            var startUnix = Convert.ToInt64(unformSchedule.start);
            Console.WriteLine(startUnix);

            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            startTime = startTime.AddSeconds(startUnix);

         

            outSchedule.startDate = startTime.ToString("dd/MM/yyyy");
            outSchedule.startTime = startTime.ToString("HH:mm");
            if(unformSchedule.finish != null)
            {
                var endTime = new DateTime(1970,1,1,0,0,0,0);
               endTime= endTime.AddSeconds(Convert.ToInt64(unformSchedule.finish));
                

                outSchedule.endTime = endTime.ToString("HH:mm");
                
            }
            

            for (int i = 0; i < teamObjs.Count; i++)
            {
                if(unformSchedule.department_id== null)
                {
                    break;
                }

                if (Convert.ToInt32(unformSchedule.department_id) == teamObjs[i].id)
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
