

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
        NONE,STAFF,TEAMS,
    }

    



    public struct Roster
    {
        public int id;
        public IList<Day> schedules;
        public string start;
        public string finish;
        public int updated_at;
    }

    public struct Day
    {
        public string date;
        public IList<Schedule> schedules;
    }

    public struct StaffHolder
    {
        public IList<User> staff;
    }

    public struct Schedule
    {
        public int id;
        public int roster_id;
        public int user_id;
        public int start;
        public object finish;
        public object[] breaks;
        public int automatic_break_length;
        public int department_id;
        public object shift_detail_id;
        public object last_published_at;
        public int updated_at;
        
    }

    public struct FormattedRoster
    {
        public IList<FormattedSchedule> schedules;
        public string start;
        public string finish;
        
            
    }

    public struct User
    {
      public int id;
       public string name;
        
    }

    

    public struct Team
    {
        public int id;
        public int location_id;
        public string name;
        public string export_name;
        public string colour;
        public IList<int> staff;
        public IList<int> managers;

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
