

using System;
using System.Collections.Generic;

namespace TandaSpreadsheetTool
{
    interface INetworkListener
    {
        

        void NetStatusChanged(NetworkStatus status);
    }

    public enum NetworkStatus
    {
        BUSY,IDLE,ERROR
    }

    public enum CurrentGet
    {
        NONE,STAFF,TEAMS,ROSTER
    }


    public class Roster
    {
        public int id;
        public List<Day> schedules { get; set; }
        public DateTime start { get; set; }
        public string finish { get; set; }
        public int updated_at { get; set; }

       public Roster()
        {
            schedules = new List<Day>();
        }

    }

    public struct SpreadSheetStyle
    {
        bool boldHeadings;
        string nameHeadingCl;

        


        public SpreadSheetStyle Default()
        {
            throw new NotImplementedException();
        }
    }

    public class Day
    {
        public string date { get; set; }
        public List<Schedule> schedules { get; set; }

        public Day()
        {
            schedules = new List<Schedule>();
        }
    }

    public class StaffHolder
    {
        public List<User> staff { get; set; }
        
        public StaffHolder()
        {
            staff = new List<User>();
        }

    }

    public struct Schedule
    {
      
        public object roster_id;
        public object user_id;
        public object start;
        public object finish;
        public object department_id;
        
    }
    [Serializable]
    public class FormattedRoster
    {
        public List<FormattedStaff> staff {get; set; }
        public DateTime start { get; set; }
        public DateTime finish { get; set; }
        
        public FormattedRoster()
        {
            staff = new List<FormattedStaff>();
        }
            
    }

    

    public struct User
    {
         public int id;
       public string name;
        
    }
    [Serializable]
    public class FormattedStaff
    {
        public List<FormattedSchedule> schedules { get; set; }

        public string name { get; set; }
        public int id { get; set; }

        public FormattedStaff()
        {
            schedules = new List<FormattedSchedule>();
           
        }
    }

    public class Team
    {
        public int id { get; set; }

        public string name { get; set; }

        public string export_name { get; set; }

        public string colour { get; set; }
        public List<int> staff { get; set; }
        public List<int> managers { get; set; }

        public Team()
        {
            staff = new List<int>();
            managers = new List<int>();
        }

    }

    [Serializable]
    public struct FormattedSchedule
    {
        
        public string startTime;
        public string endTime;
        public DateTime startDate;
        public string teamNameShort;
        public string team;
        public string teamColour;

    }

}
