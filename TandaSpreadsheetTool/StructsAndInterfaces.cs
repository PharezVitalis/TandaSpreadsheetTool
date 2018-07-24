

using System;
using System.Collections.Generic;

namespace TandaSpreadsheetTool
{
   
  
    

    public enum SpreadSheetDiv
    {
        NONE,WEEKLY,BIWEEKLY,MONTHLY
    }
    
    public interface INotifiable
    {
        void EnableNotifiers();
        void DisableNotifiers();
        void UpdateProgress(string progressUpdate, int progress = -1);
        int ProcessCount { get; }
        void RaiseMessage(string title, string message,System.Windows.Forms.MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Information);

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
        //cl = colour

        public byte[] nameHeadingCl;
        public byte[] nameFieldCl;
        public byte[] rotaFieldCl;
        public byte[] rotaEmptyCl;
        public byte[] dayNameCl;
        public byte[] dateCl;

        public bool boldHeadings;
        public bool teamTxtFilter;
        public float minBrightness;
        public int colWidth;
        public bool useTeamCls;
        public string font;
        public int fontSize;
        public bool boldFs;
        public bool italicFs;
        public bool strikeThroughFs;
        public bool underLineFs;

        public bool useTeamLegends;
        public bool useShiftLegends;
        public SpreadSheetDiv divBy;

        

        public static SpreadSheetStyle Default()
        {
            return new SpreadSheetStyle()
            {
                boldHeadings = true,
                nameHeadingCl = new byte[] { 61, 133, 198 },
                nameFieldCl = new byte[] { 207, 226, 243 },
                teamTxtFilter = true,
                minBrightness = 0.7f,
                useTeamCls = true,
                rotaFieldCl = new byte[] { 255, 255, 255 },
                rotaEmptyCl = new byte[] { 163, 168, 175 },
                dayNameCl = new byte[] { 56, 118, 29 },
                dateCl = new byte[] { 147, 196, 125 },
                colWidth = 22,
                font = "Arial",
                fontSize = 11,
                divBy = SpreadSheetDiv.WEEKLY,
                useShiftLegends = false,
                useTeamLegends = false,
                boldFs = false,
                italicFs = false,
                strikeThroughFs = false,
                underLineFs = false            
            };
           

          
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
