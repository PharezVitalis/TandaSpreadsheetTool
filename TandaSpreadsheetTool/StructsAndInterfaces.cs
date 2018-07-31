

using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using NPOI.XSSF.UserModel;

namespace TandaSpreadsheetTool
{
   //File to hold Data Classes, interfaces, simple structures and enumerators
  
    
        /// <summary>
        /// Division setting for Spreadsheet
        /// </summary>
    public enum SpreadSheetDiv
    {
        NONE,WEEKLY,BIWEEKLY,MONTHLY
    }
    
    /// <summary>
    /// A Interface used to notify forms of any background progress
    /// </summary>
    public interface INotifiable
    {
        void NewNotifier();
        void RemoveNotifier();
        void UpdateProgress(string progressUpdate, int progress = -1);
        int NotifiersCount { get; }
        void RaiseMessage(string title, string message,System.Windows.Forms.MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Information);

    }

    /// <summary>
    /// Roster class that JSON Roster deserialises to
    /// </summary>
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

    /// <summary>
    /// Whether vertical date and day heading should be used
    /// </summary>
    public enum UseVerticalDates
    {
        TRUE, AUTO, FALSE
    }

    /// <summary>
    /// Data class for Storing meta data and style for each team when building spreadsheet
    /// </summary>
   public class TeamValue
    {
        public XSSFCellStyle style;
        public XSSFCellStyle styleBold;
        public bool isUsed;
        public int dayCount;
        public int legendPosition;

        public TeamValue()
        {
        }

        public TeamValue( XSSFCellStyle style,XSSFCellStyle styleBold, bool isUsed=false, int dayCount =0, int legendPosition = 0)
        {
            this.isUsed = isUsed;
            this.style = style;
            this.dayCount = dayCount;
            this.legendPosition = legendPosition;
            this.styleBold = styleBold;
        }
    }

    /// <summary>
    /// Spreadsheet style settings
    /// </summary>
    public struct SpreadSheetStyle
    {
        //cl = colour

        public byte[] nameHeadingCl;
        public byte[] nameFieldCl;
        public byte[] rotaFieldCl;
        public byte[] rotaEmptyCl;
        public byte[] dayNameCl;
        public byte[] dateCl;
        public byte[] teamLegHeadCl;
        public byte[] tlShiftHeadCl;
        public byte[] tlShiftFieldCl;
        public byte[] wkndDayCl;
        public byte[] wkndDateCl;
        public byte[] wkndTotalCl;

        public char teamNameSep;
        public FillPattern fieldWkndPat; 
        public bool boldHeadings;      
        public float minBrightness;
        public float colWidth;
        public UseVerticalDates useVertDates;      
        public string font;
        public int fontSize;
        public bool boldFs;
        public bool italicFs;
        public bool strikeThroughFs;
        public bool underLineFs;
        public HorizontalAlignment nameAlign;
        public HorizontalAlignment headAlign;
        public HorizontalAlignment rotaAlign;

        public bool useTeamLegends;
        public bool useShiftLegends;
        public bool shiftAnalysis;
        public SpreadSheetDiv divBy;

       

        public static SpreadSheetStyle Default()
        {
            return new SpreadSheetStyle()
            {
                boldHeadings = true,
                nameHeadingCl = new byte[] { 61, 133, 198 },
                nameFieldCl = new byte[] { 207, 226, 243 },
                tlShiftHeadCl = new byte[] { 142, 169, 219 },
                tlShiftFieldCl = new byte[] { 56, 118, 29 },
                wkndDayCl = new byte[] { 255, 255, 0 },
                wkndDateCl = new byte[] {255,255,153},
                wkndTotalCl = new byte[] {255,255,0},
                minBrightness = 0.7f,
                teamNameSep = ':',
                rotaFieldCl = new byte[] { 255, 255, 255 },
                rotaEmptyCl = new byte[] { 163, 168, 175 },
                dayNameCl = new byte[] { 56, 118, 29 },
                dateCl = new byte[] { 147, 196, 125 },
                teamLegHeadCl = new byte[] { 108, 101, 245},
                colWidth = 12,
                font = "Arial",
                fontSize = 11,
                divBy = SpreadSheetDiv.NONE,
                useShiftLegends = true,
                useTeamLegends = false,
                boldFs = false,
                italicFs = false,
                strikeThroughFs = false,
                underLineFs = false,
                shiftAnalysis = true,
                nameAlign = HorizontalAlignment.Left,
                headAlign = HorizontalAlignment.Left,
                rotaAlign = HorizontalAlignment.Left,              
                useVertDates = UseVerticalDates.AUTO
            };
           
          
        }
       
    }

    /// <summary>
    /// Container class for scedules
    /// </summary>
    public class Day
    {
        public string date { get; set; }
        public List<Schedule> schedules { get; set; }

        public Day()
        {
            schedules = new List<Schedule>();
        }
    }

 /// <summary>
 /// JSON deseriliases to this schedule
 /// </summary>
    public struct Schedule
    {
      
        public object roster_id;
        public object user_id;
        public object start;
        public object finish;
        public object department_id;
        
    }

    /// <summary>
    /// A Formatter roster structure used to create spreadsheets
    /// </summary>
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

    /// <summary>
    /// Staff JSON objects are deserialised to this structure
    /// </summary>

    public struct User
    {
         public int id;
       public string name;
        
    }
    /// <summary>
    /// Formatted staff member (has 1-m relationship with schedules) used in FormattedRoster
    /// </summary>
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

    /// <summary>
    /// Team Data Class which JSON object deserialises to
    /// </summary>
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

    /// <summary>
    /// A formatted schedule used by FormattedRoster
    /// </summary>
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
