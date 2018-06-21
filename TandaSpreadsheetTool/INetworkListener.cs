

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
        int id;
        IList<Day> schedules;
        string start;
        string finish;
        int updated_at;
    }

    public struct Day
    {
        string date;
        IList<Schedule> schedules;
    }

    public struct Schedule
    {
        int id;
        int roster_id;
        int user_id;
        int start;
        int finish;
        object[] breaks;
        int automatic_break_length;
        int department_id;
        object shift_detail_id;
        object last_published_at;
        int updated_at;
        
    }
}
