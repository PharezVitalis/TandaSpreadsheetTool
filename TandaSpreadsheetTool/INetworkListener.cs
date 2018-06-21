

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

    public struct Schedule
    {
        public int id;
        public int roster_id;
        public int user_id;
        public int start;
        public int finish;
        public object[] breaks;
        public int automatic_break_length;
        public int department_id;
        public object shift_detail_id;
        public object last_published_at;
        public int updated_at;
        
    }

    public struct FormattedRoster
    {
        IList<Schedule> schedules;
        string start;
        string finish;
        
            
    }

    public struct FormattedSchedule
    {
        public string staff;
        string startTime;
        string endTime;
        string startDate;
        string team;

    }

}
