using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

namespace TandaSpreadsheetTool
{
    class RosterManager
    {
        
        Networker networker;
     
        Form1 form;

        int maxTries;
        int retryDelay;



        List<FormattedRoster> builtRosters;

        List<User> staffObjs;

        List<Team> teamObjs;


        bool hasStaff = false;
        bool hasTeams = false;

        DateTime staffLstLastUpdated;
  
        public static string Path
        {
            get
            {
                return Environment.CurrentDirectory + "\\" + "Data"+"\\";
            }
        }
       

        public RosterManager(Networker networker, Form1 form, int maxTries = 5, int retryDelay = 500  )
        {
            this.maxTries = maxTries;

            this.networker = networker;
            this.form = form;
            this.retryDelay = retryDelay;
            
          
            staffObjs = new List<User>();
            
            teamObjs = new List<Team>();

           


           
        }


     public string LastStaffUpdate
        {
            get
            {
                if (File.Exists(Path + "staff.json"))
                {
                    return File.GetLastWriteTime(Path + "staff.json").ToLongDateString();  
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<bool> GetStaff(bool forceUpdate = false)
        {
            var staffJArr = new JArray();
            var readFromFile = false;

            if (!forceUpdate & File.Exists(Path+"staff.json" )) 
            {
                var fileData = File.ReadAllText(Path+"staff.json");
                try
                {
                    staffJArr = JArray.Parse(fileData);
                   staffLstLastUpdated = File.GetLastAccessTime(Path + "staff.json");
                    readFromFile = true;
                }
                catch 
                {
                    Console.WriteLine("ERROR GETTING STAFF FROM FILE");
                    File.Delete(Path + "staff.json");
                    staffJArr = await networker.GetStaff();
                }
                
                
                
            }
            else
            {
                staffJArr = await networker.GetStaff();
            }
             
            try
            {


                for (int i = 0; i < staffJArr.Count; i++)
                {
                    var staff = JsonConvert.DeserializeObject<User>(staffJArr[i].ToString());

                    if (!staffObjs.Contains(staff))
                    {
                        staffObjs.Add(staff);
                    }
                }
            }

            catch 
            {
                Console.WriteLine("error deserializing users");
                hasStaff = false;
                return false;
            }

            if (!readFromFile)
            {
                string[] removedFields = { "date_of_birth", "passcode" , "user_levels", "preferred_hours", "active", "email",
                    "photo", "phone","normalised_phone","time_zone", "created_at", "employment_start_date", "employee_id",
                "department_ids","award_template_id", "award_tag_ids", "report_department_id",  "managed_department_ids",
               "utc_offset",  "part_time_fixed_hours","qualifications","updated_at" };

                staffLstLastUpdated = DateTime.Now;

                for (int i = 0; i < staffJArr.Count; i++)
                {
                    var currentToken = staffJArr[i];
                    RemoveFields(currentToken, removedFields);

                }
                File.WriteAllText(Path + "staff.json", staffJArr.ToString());
            }



            hasStaff = true;
            return true;

        }

        public string LastUpdated
        {
            get
            {
                return staffLstLastUpdated.ToShortDateString();
            }
        }


        private void RemoveFields( JToken token, string[] fields)
        {
            JContainer container = token as JContainer;
            if (container == null) return;

            List<JToken> removeList = new List<JToken>();
            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                {
                    removeList.Add(el);
                }
                RemoveFields( el, fields);
            }

            foreach (JToken el in removeList)
            {
                el.Remove();
            }
        }

        public async Task<bool> GetTeams(bool forceUpdate = false)
        {
            var teamsArr = new JArray();
            bool readFromFile = false;

            if (!forceUpdate & File.Exists(Path + "teams.json"))
            {
                var fileData = File.ReadAllText(Path + "teams.json");
                try
                {
                    teamsArr = JArray.Parse(fileData);
                    readFromFile = true;
                }
                catch
                {
                    Console.WriteLine("ERROR GETTING TEAMS FROM FILE");
                    File.Delete(Path + "teams.json");
                    teamsArr = await networker.GetStaff();
                }



            }
            else
            {
                teamsArr = await networker.GetDepartments();
            }


         

            try
            {
                for (int i = 0; i < teamsArr.Count; i++)
                {
                    var team = JsonConvert.DeserializeObject<Team>(teamsArr[i].ToString());

                    if (!teamObjs.Contains(team))
                    {
                        teamObjs.Add(team);
                    }
                }
            }
            catch 
            {
                Console.WriteLine("Error deserializing teams");
                hasTeams = false;
                return false;
            }

            if (!readFromFile)
            {
                File.WriteAllText(Path + "teams.json", teamsArr.ToString());
            }

            hasTeams = true;
            return true;
        }

        public static DateTime SetToTime(DateTime day, int hours, int minutes)
        {
            
            
            day = day - day.TimeOfDay;
           day= day.AddHours(hours);
            day =day.AddMinutes(minutes);
            return day;

        }

       public async Task<FormattedRoster> BuildRoster(DateTime dateFrom, DateTime dateTo)
        {
            var currentTries = 0;
            
           
            dateFrom = SetToTime(dateFrom,0,0);
            dateTo = SetToTime(dateTo, 23, 59);

            int weeks =(int) (dateTo - dateFrom ).TotalDays / 7;

            var rosters = (dateFrom.DayOfWeek != DayOfWeek.Monday)? new JObject[weeks+1]: new JObject[weeks];

            var currentDate = dateFrom;

            for (int i = 0; i < weeks; i++)
            {

                rosters[i] = await networker.GetRooster(currentDate);

                currentDate = currentDate.AddDays(7);

            }
            if (dateFrom.DayOfWeek!= DayOfWeek.Monday)
            {
                rosters[rosters.Length - 1] = await networker.GetRooster(dateTo);
            }

            if (!hasTeams)
            {
                currentTries = 0;
                while(true)
                {
                    await GetTeams();

                    currentTries++;
                    if (!hasTeams)
                    {
                        if (currentTries < maxTries)
                        {
                            System.Threading.Thread.Sleep(retryDelay);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        

            if (!hasStaff)
            {
                currentTries = 0;
                while (true)
                {
                    await GetStaff();
                    currentTries++;
                    if(!hasStaff)
                    {
                        if (currentTries < maxTries)
                        {
                            System.Threading.Thread.Sleep(retryDelay);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }





            // format rosters - adding all staff to the outroster will mean all staff in ESMS render in the spreadshee

            var rosterObjs = new Roster[rosters.Length];

            for (int i = 0; i < rosterObjs.Length; i++)
            {
              rosterObjs[i] = JsonConvert.DeserializeObject<Roster>(rosters[i].ToString());
            }

            var outRoster = CreateRoster(rosterObjs);
            
            outRoster.finish = dateTo;
            outRoster.start = dateFrom;

            return outRoster;
        }

        public static DateTime BuildDate(string date)
        {
            int days = 0;
            int months = 0;
            int years = 0;

            try
            {
            days = Convert.ToInt32(date.Substring(0, 2));
                months = Convert.ToInt32(date.Substring(3, 2));
                if (date.Length > 8)
                {
                    years = Convert.ToInt32(date.Substring(6, 4));
                }
                else
                {
                    years = 2000 + (Convert.ToInt32(date.Substring(6, 2)));
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return new DateTime(1970, 1, 1);
                
            }

            return new DateTime(years, months, days);
        }

        public static DateTime UnixToDate(int unixValue)
        {
            var date = new DateTime(1970, 1, 1);
            date = date.AddSeconds(unixValue);
            date = date.ToLocalTime();



            return date;
        }

        public static DateTime UnixToDate(long unixValue)
        {
            var date = new DateTime(1970, 1, 1,0,0,0);
            date = date.AddSeconds(unixValue);
            date = date.ToLocalTime();


            return date;
        }

        public static string FormatDate(string date)
        {     
            return date.Substring(6, 4) + "/" + date.Substring(3, 2) + "/" + date.Substring(0, 2);
        }

        public int RetryDelay
        {
            get
            {
                return retryDelay;
            }
            set
            {
                if (value < 1500 && value > 0)
                {
                    retryDelay = value;
                }
                else if (value < 0)
                {
                    retryDelay = 0;
                }
                else
                {
                    retryDelay = 1500;
                }
            }
        }

        private FormattedRoster CreateRoster(Roster[] rosters)
        {

            var roster = new FormattedRoster();

            for (int i = 0; i < rosters.Length; i++)
            {
                var currentRoster = rosters[i];

                for (int j = 0; j < currentRoster.schedules.Count; j++)
                {
                    var currentDay = currentRoster.schedules[j];

                    for (int k = 0; k < currentDay.schedules.Count; k++)
                    {
                        for (int l = 0; l < currentDay.schedules.Count ; l++)
                        {
                            var currentSchedule = currentDay.schedules[i];

                            var staffIdObj = currentSchedule.user_id;

                            int staffId = (staffIdObj != null) ? Convert.ToInt32(staffIdObj) : -1;
                            
                            if (staffId == -1)
                            {
                                continue;
                            }
                            
                                var staffMember = roster.staff.Find(x => x.id ==staffId);

                                if (staffMember == null)
                                {
                                    staffMember = new FormattedStaff();
                                    staffMember.id = staffId;
                                    staffMember.name = staffObjs.Find(x => x.id == staffId).name;
                                    if(staffMember.name == null)
                                    {
                                        continue;
                                    }
                                    roster.staff.Add(staffMember);
                                }
                                

                                var formattedSch = new FormattedSchedule();

                                var start = currentSchedule.start;

                                if (start != null)
                                {
                                    formattedSch.startDate = UnixToDate(Convert.ToInt32(start));
                                }

                               
                                formattedSch.startTime = formattedSch.startDate.ToString("HH:mm");

                                var teamId = currentSchedule.department_id;
                               
                                var team = (teamId !=null) ?teamObjs.Find(x => x.id == Convert.ToInt32(teamId)):null;

                                if (currentSchedule.finish != null)
                                {
                                    formattedSch.endTime = UnixToDate(Convert.ToInt32(currentSchedule.finish)).ToString("HH:mm");
                                }

                                if (team != null)
                                {
                                    formattedSch.team = team.name;
                                    formattedSch.teamColour = team.colour;
                                    formattedSch.teamNameShort = team.export_name;
                                }

                                staffMember.schedules.Add(formattedSch);


                            
                            
                            
                        }
                    }

                }




            }

            builtRosters.Add(roster);

            return roster;
        }


        public FormattedRoster[] GetAllRosters
        {
            get
            {
                return builtRosters.ToArray();
            }
        }
        

      

    }
}
