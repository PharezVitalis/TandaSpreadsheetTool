

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

    
   
    public class FormattedTimeTable
    {
      public List<Roster> rosters { get; set; }

        public FormattedTimeTable()
        {
            rosters = new List<Roster>();
        }
    }

    public class Roster
    {
        public int id;
        public List<Day> schedules { get; set; }
        public string start { get; set; }
        public string finish { get; set; }
        public int updated_at { get; set; }

       public Roster()
        {
            schedules = new List<Day>();
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
        public object id;
        public object roster_id;
        public object user_id;
        public object start;
        public object finish;
       
        public object department_id;
       
      
        
    }

    public class FormattedRoster
    {


        public List<FormattedSchedule> schedules { get; set; }
        public string start { get; set; }
        public string finish { get; set; }
        
        public FormattedRoster()
        {
            schedules = new List<FormattedSchedule>();
        }
            
    }

    public struct User
    {
         public int id;
       public string name;
        
    }

    

    public class Team
    {
        public int id { get; set; }

        public string name { get; set; }

        public string colour { get; set; }
        public List<int> staff { get; set; }
        public List<int> managers { get; set; }

        public Team()
        {
            staff = new List<int>();
            managers = new List<int>();
        }

    }

    public struct FormattedSchedule
    {
        public string staff;
        public string startTime;
        public string endTime;
        public string startDate;
        public string team;

    }

}
