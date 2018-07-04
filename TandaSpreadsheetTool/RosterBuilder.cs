using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TandaSpreadsheetTool
{
    class RosterBuilder
    {
        
        Networker networker;
     
        Form1 form;

        int maxTries;
        

       
       

        List<User> staffObjs;

        List<Team> teamObjs;


        bool hasTeams;
      

  

       

        public RosterBuilder(Networker networker, Form1 form, int maxTries = 5  )
        {
            this.maxTries = maxTries;

            this.networker = networker;
            this.form = form;

            
          
            staffObjs = new List<User>();
            
            teamObjs = new List<Team>();

           


           
        }


        public async Task<bool> GetStaff()
        {
           

            var staffJArr = await networker.GetStaff();
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
                
                return false;
            }
           
            return true;

        }

        public async Task<bool> GetTeams()
        {
            

            var teamsArr = await networker.GetDepartments();

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
               
                return false;
            }
           

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

                rosters[i] = await networker.GetRooster(dateFrom);

                currentDate = currentDate.AddDays(7);

            }
            if (dateFrom.DayOfWeek!= DayOfWeek.Monday)
            {
                rosters[rosters.Length - 1] = await networker.GetRooster(dateTo);
            }
            

            if (!hasTeams)
            {
                bool gotTeams = false;
                bool gotStaff = false;
                while (!gotTeams & currentTries<maxTries )
                {
                    gotTeams = await GetTeams();
                    

                    currentTries++;
                }
                currentTries = 0;

                while (!gotStaff & currentTries < maxTries)
                {
                    gotStaff = await GetStaff();


                    currentTries++;
                }
                currentTries = 0;

                if (gotStaff & gotTeams)
                {
                    hasTeams = true;
                }
                else
                {
                   
                    return null;
                }

            }


           

          
            

            var outRoster = new FormattedRoster();

            

          
          

            // format rosters
            for (int i = 0; i < rosters.Length; i++)
            {
                var currentRoster = JsonConvert.DeserializeObject<Roster>(rosters[i].ToString());

                for (int j = 0; j < currentRoster.schedules.Count; j++)
                {
                    var currentDay = currentRoster.schedules[j];

                    for (int k = 0; k < currentDay.schedules.Count; k++)
                    {
                        var currentSchedule = currentDay.schedules[k];
                        currentDate = UnixToDate(Convert.ToInt32(currentSchedule.start));


                        if (currentDate.CompareTo(dateFrom)<0)
                        {
                            continue;
                        }
                        else if (dateTo.CompareTo(currentDate)< 0)
                        {
                            break;
                        }
                        outRoster.schedules.Add(GenerateSchedule(currentSchedule));
                    }
                }

               
            }
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

           

            var startTime = UnixToDate(Convert.ToInt32(unformSchedule.start));



            outSchedule.startDate = startTime;
            outSchedule.startTime = startTime.ToShortTimeString();
            if(unformSchedule.finish != null)
            {
                var endTime = UnixToDate(Convert.ToInt32(unformSchedule.finish));
                

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
                    outSchedule.teamNameShort = teamObjs[i].export_name;
                    break;
                }
            }

            return outSchedule;
        }


       
        

      

    }
}
