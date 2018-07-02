using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TandaSpreadsheetTool
{
    class RosterBuilder:INetworkListener
    {
        
        Networker networker;
     
        Form1 form;

        int maxTries;
        int currentTries = 0;

        bool hasTeams = false;
       

        List<User> staffObjs;

        List<Team> teamObjs;

    CurrentGet currentGet = CurrentGet.NONE;

      

  

       

        public RosterBuilder(Networker networker, Form1 form, int maxTries = 5, bool autoGetData = true )
        {
            this.maxTries = maxTries;

            this.networker = networker;
            this.form = form;

            
          
            staffObjs = new List<User>();
            
            teamObjs = new List<Team>();

            if (networker.Roster == null)
            {
                return;
            }

           
          if (autoGetData)
            {
                GetStaff();
                GetTeams();
                hasTeams = true;
            }

           
        }


        public async void GetStaff()
        {
            currentGet = CurrentGet.STAFF;

            var staffJArr = await networker.GetStaff();

            for (int i = 0; i < staffJArr.Count; i++)
            {
                var staff = JsonConvert.DeserializeObject<User>(staffJArr[i].ToString());

                if (!staffObjs.Contains(staff))
                {
                    staffObjs.Add(staff);
                }                
            }

            currentGet = CurrentGet.NONE;
        }

        public async void GetTeams()
        {
            currentGet = CurrentGet.TEAMS;

            var teamsArr = await networker.GetDepartments();

            for (int i = 0; i < teamsArr.Count; i++)
            {
                var team = JsonConvert.DeserializeObject<Team>(teamsArr[i].ToString());

                if (!teamObjs.Contains(team))
                {
                    teamObjs.Add(team);
                }
            }

            currentGet = CurrentGet.NONE;
        }

       public async Task<FormattedRoster> BuildRoster(string dateFrom, string dateTo)
        {
            var from = BuildDate(dateFrom);
            var to = BuildDate(dateTo);


            int weeks =(int) (to - from).TotalDays / 7;

            var rosters = new JObject[weeks];

            if (!hasTeams)
            {
                GetTeams();
                GetStaff();
            }

            currentGet = CurrentGet.ROSTER;


            for (int i = 0; i < weeks; i++)
            {
                string date = from.AddDays(7 * i).ToShortDateString();
                date = FormatDate(date);
                rosters[i] = await networker.GetRooster(date);
                
            }
           

            currentGet = CurrentGet.NONE;


            var outRoster = new FormattedRoster();

            outRoster.start = dateFrom;
          

            // format rosters
            for (int i = 0; i < rosters.Length; i++)
            {
                var currentRoster = JsonConvert.DeserializeObject<Roster>(rosters[i].ToString());

                for (int j = 0; j < currentRoster.schedules.Count; j++)
                {
                    var currentDay = currentRoster.schedules[j];

                    for (int k = 0; k < currentDay.schedules.Count; k++)
                    {
                        var currentSchedule = currentDay.schedules[j];

                        outRoster.schedules.Add(GenerateSchedule(currentSchedule));
                    }
                }

               
            }
            outRoster.finish = outRoster.schedules[outRoster.schedules.Count - 1].startDate;


            return outRoster;

         
        }

        public static DateTime BuildDate(string date)
        {
            var outDate = new DateTime();

           outDate=outDate.AddDays(Convert.ToInt32(date.Substring(0, 2)));
           outDate= outDate.AddMonths(Convert.ToInt32(date.Substring(3, 2)));
            
            if (date.Length < 10)
            {
                int years = Convert.ToInt32(date.Substring(6, 2));
                if (years > DateTime.Now.Year)
                {
                    years += 1900;
                }
                else
                {
                    years += 2000;
                }
                outDate = outDate.AddYears(years);
            }
            else
            {
                outDate = outDate.AddYears(Convert.ToInt32(date.Substring(6, 4)));
            }

            return outDate;
        }

        public static DateTime UnixToDate(int unixValue)
        {
            var date = new DateTime(1970, 1, 1);
            date = date.AddSeconds(unixValue);



            return date;
        }

       public void NetStatusChanged(NetworkStatus status)
        {

        }

       public static string FormatDate(string date)
        {     
            return date.Substring(8, 2) + "/" + date.Substring(5, 2) + "/" + date.Substring(0, 4);
        }

        public static void Save(string path, string data)
        {
            var serializedData = Serialize(data);

            File.WriteAllBytes(path, serializedData);

           
            
        }

       public static object Load(string path)
        {
            return Deserialize(File.ReadAllBytes(path));
        }

        public static byte[] Serialize(string data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(data);
                }
                return ms.ToArray();
            }

        }

        public static string Deserialize(byte[] data)
        {
            string outStr = "";

            for (int i = 0; i < data.Length; i++)
            {
                outStr += Convert.ToChar(data[i]);
            }

            return outStr;
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


       
        

      

    }
}
