using System;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TandaSpreadsheetTool
{
    class RosterBuilder
    {
        
        Networker networker;
        Roster rosterObj;

        List<JObject> staffJson;
        
        Departments teamObjs;
        List<User> staffObjs;

      

        FormattedRoster formRoster;

        public RosterBuilder(Networker networker )
        {
            this.networker = networker;
            networker.GetDepartments();

           staffJson = new List<JObject>();
          
            staffObjs = new List<User>();

            if (networker.Roster == null)
            {
                return;
            }

           
            rosterObj = JsonConvert.DeserializeObject<Roster>(networker.Roster.ToString());

            List<int> staffIds = new List<int>();
         

            for (int i = 0; i < rosterObj.schedules.Count; i++)
            {
                var currentDate = rosterObj.schedules[i];

                for (int j = 0; j < currentDate.schedules.Count; j++)
                {
                    var currentSchedule = currentDate.schedules[j];

                    var currentStaffId = currentSchedule.user_id;

                    if (!staffIds.Contains(currentStaffId))
                    {
                        staffIds.Add(currentStaffId);
                    }
                }
            }
            networker.GetStaff(staffIds.ToArray());
        }


        private void GenerateStaffObjects()
        {
            if (staffJson.Count < 1)
            {
                return;
            }
            staffObjs.Clear();

            for (int i = 0; i < staffJson.Count; i++)
            {
                staffObjs.Add(JsonConvert.DeserializeObject<User>(staffJson[i].ToString()));
            }
        }

       public void BuildRoster()
        {

            var staffArr = networker.Staff;

            if (staffArr != null)
            {
                for (int i = 0; i < staffArr.Length; i++)
                {
                    var currentStaff = staffArr[i];

                    if (!staffJson.Contains(currentStaff))
                    {
                       staffJson.Add(staffArr[i]);
                    }                    
                }

                GenerateStaffObjects();
            }

            
            teamObjs = JsonConvert.DeserializeObject<Departments>(networker.Teams.ToString());
            formRoster = new FormattedRoster();

            formRoster.start = FormatDate(rosterObj.start);
            formRoster.finish = FormatDate(rosterObj.finish);

            for (int i = 0; i < rosterObj.schedules.Count; i++)
            {
                var currentDay = rosterObj.schedules[i];

                for (int j = 0; j < currentDay.schedules.Count; j++)
                {
                    formRoster.schedules.Add(GenerateSchedule(currentDay.schedules[j]));

                    
                }
            }

        }

        string FormatDate(string date)
        {
            string formDate = "";

            formDate += date.Substring(8, 2)+"/"+date.Substring(5,2)+"/"+date.Substring(0,4);
        

            return formDate;
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
            outSchedule.endTime = new DateTime(unformSchedule.finish * 10000000).ToString("HH:mm");

            for (int i = 0; i < teamObjs.teams.Count; i++)
            {
                if (unformSchedule.department_id == teamObjs.teams[i].id)
                {
                    outSchedule.team = teamObjs.teams[i].name;
                    break;
                }
            }

            return outSchedule;
        }

    }
}
