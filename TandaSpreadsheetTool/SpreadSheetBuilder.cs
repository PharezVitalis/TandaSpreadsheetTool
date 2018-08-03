
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System;
using System.Collections.Generic;
using NPOI.XSSF.UserModel.Extensions;


namespace TandaSpreadsheetTool
{
    /// <summary>
    /// Class for Building Spreadsheets using NPOI library
    /// </summary>
    class SpreadSheetBuilder
    {
        /// <summary>
        /// array of the teams that the roster manager handles
        /// </summary>
        Team[] teams;

        // wether the teams have been set
        bool setTeams;
        
        //  should make these parameters
        int maxColCount;
        int maxRowCount;
        int nOfDays;


        /// <summary>
        /// dictionary that holds team styles and metadata for the teams
        /// </summary>
        Dictionary<string, TeamValue> teamDict;

        /// <summary>
        /// dictionary that holds other styles as well as team weekend one
        /// </summary>
        Dictionary<string, XSSFCellStyle> styleDict; //team weekend styles are added inside of the create table methods

        /// <summary>
        /// Interface to main form
        /// </summary>
        INotifiable form;

        /// <summary>
        /// Creates a new instance of the Spreadsheet Builder class
        /// </summary>
        /// <param name="form"></param>
        public SpreadSheetBuilder(INotifiable form)
        {
            this.form = form;
            teamDict = new Dictionary<string, TeamValue>();
            styleDict = new Dictionary<string, XSSFCellStyle>();

        }

        /// <summary>
        /// Sets the team array within the class
        /// </summary>
        /// <param name="teams"> The team array to be set</param>
        public void SetTeams(Team[] teams)
        {
            this.teams = teams;
            setTeams = true;

        }

        /// <summary>
        /// The automatic path used when no path is given
        /// </summary>
        public static string SpreadSheetPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Tanda Rosters\\";
            }
        }

        /// <summary>
        /// Checks to see if a given day is a weekend
        /// </summary>
        /// <param name="day">day to be tested</param>
        /// <returns>Returns true if the day is a weekend</returns>
        bool IsWeekend(DayOfWeek day)
        {
            return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
        }

        /// <summary>
        /// returns true if the day is a weekend
        /// </summary>
        /// <remarks>It is based on using the value giving from DayOfWeek inside of  a DateTime</remarks>
        /// <param name="day">day to be tested, must start with capital</param>
        /// <returns>Returns true if the day is a weekend</returns>
        bool IsWeekend(string day)
        {           
            return day == "Saturday" | day == "Sunday";
        }

        /// <summary>
        /// Creates a workbook at the given path and puts all the roster data into it
        /// </summary>
        /// <param name="roster">The roster data used to generate the workbook</param>
        /// <param name="style">Styling options for the workbook</param>
        /// <param name="path">The full path (include file name and .xlsx only) for where it will be saved</param>
        /// <returns>Returns True if completed sucessfully</returns>
        public bool CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style, string path = null)
        {
            var workBook = (IWorkbook)new XSSFWorkbook();
            maxColCount = 0;
            maxRowCount = 0;

            // some style created inside of the table methods need an italic font
            var weekendFont = workBook.CreateFont();
            weekendFont.FontName = style.font;
            weekendFont.FontHeightInPoints = (short)style.fontSize;
            weekendFont.IsItalic = true;
            weekendFont.Underline = FontUnderlineType.Double;

            form.NewNotifier();
            form.UpdateProgress("Building Spreadsheet...");

            form.UpdateProgress("Building styles");
            BuildStyleDicts(ref style,weekendFont, workBook);

            var sheet = (ISheet)null;
            
            var nOfDaysToAdd = -1;

            if (path == null)
            {
                path = SpreadSheetPath + "Roster " + roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy") + ".xlsx";
            }

            switch (style.divBy)
            {

                case SpreadSheetDiv.NONE:
                    nOfDaysToAdd = 0;

                    break;
                case SpreadSheetDiv.WEEKLY:
                    if ((roster.finish - roster.start).TotalDays > 7)
                    {
                        nOfDaysToAdd = 6;
                    }
                    else
                    {
                        nOfDaysToAdd = 0;
                    }

                    break;
                case SpreadSheetDiv.BIWEEKLY:
                    if ((roster.finish - roster.start).TotalDays > 14)
                    {
                        nOfDaysToAdd = 13;
                    }
                    else
                    {
                        nOfDaysToAdd = 0;
                    }
                    break;
                case SpreadSheetDiv.MONTHLY:
                    if (roster.start.Month == roster.finish.Month)
                    {
                        nOfDaysToAdd = 0;
                    }
                    else
                    {
                        nOfDaysToAdd = 30;
                    }

                    break;
                default:
                    break;
            }

            DateTime currentDate;
            form.UpdateProgress("Building cell data");

            switch (nOfDaysToAdd)
            {
                case 0:
                    sheet = workBook.CreateSheet(roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy"));
                    CreateRosterTable(roster, weekendFont, style, sheet, workBook);
                    CreateTotalShiftCount(style, sheet);
                    if (style.useShiftLegends)
                    {
                        GenTeamData(sheet, roster, style);
                    }
                    break;

                case -1:
                    return false;

                case 30:

                    var toDate = roster.start;

                    while (toDate <= roster.finish)
                    {
                        var fromDate = toDate;
                        while (roster.start.Month == toDate.Month)
                        {
                            toDate = toDate.AddDays(1);

                        }
                        sheet = workBook.CreateSheet(fromDate.ToString("dd-MM-yy") + " to " + toDate.ToString("dd-MM-yy"));


                        CreateRosterTable(roster,weekendFont, style, sheet, workBook, fromDate,
                            toDate.AddDays(-1), (fromDate - roster.start).Days - 1);
                        CreateTotalShiftCount(style, sheet);

                        if (style.useShiftLegends)
                        {
                            GenTeamData(sheet, roster, style);
                        }
                    }
                    break;

                default:
                    currentDate = roster.start;
                    do
                    {
                        var nextDate = currentDate.AddDays(nOfDaysToAdd);
                        if (nextDate > roster.finish)
                        {
                            nextDate = roster.finish;
                        }
                        var worksheetName = currentDate.ToString("dd-MM-yy") + " to " + nextDate.ToString("dd-MM-yy");

                        sheet = workBook.CreateSheet(worksheetName);

                        CreateRosterTable(roster,weekendFont, style, sheet, workBook, currentDate, nextDate, (currentDate - roster.start).Days);
                        CreateTotalShiftCount(style, sheet);
                        if (style.useShiftLegends)
                        {
                            GenTeamData(sheet, roster, style);
                        }

                        currentDate = nextDate.AddDays(1);

                    }
                    while (currentDate < roster.finish);

                    break;

            }


            form.UpdateProgress("Saving File");


            try
            {

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    workBook.Write(fs);
                }

            }
            catch (Exception e)
            {
                form.UpdateProgress("Failed to save file: " + e.Message);
                form.RaiseMessage("Failed To save Excel File", "Excel File failed to save: " + e.Message);
                form.RemoveNotifier();
                return false;
            }
            workBook.Close();
            teamDict.Clear();
            styleDict.Clear();
            form.UpdateProgress("Finished creating excel sheet at: " + path);
            form.RemoveNotifier();
            GC.Collect();
            return true;            
        }


        /// <summary>
        /// creates a roster table on the sheet within the range of specified dates
        /// </summary>
        /// <param name="roster">The roster data</param>
        /// <param name="italics"> An weekendFont font used to flag weekends</param>
        /// <param name="style">The styling options for the table</param>
        /// <param name="sheet">The sheet that the roster will be put on. Must be blank</param>
        /// <param name="workbook">The workbook associated with the sheet</param>
        /// <param name="from">The earliest date from which schedules will be added</param>
        /// <param name="to">The latest date to which schedules will be added</param>
        /// <param name="colOffset">Value which is added to column index as it is tied to date</param>
        private void CreateRosterTable(FormattedRoster roster, IFont weekendFont, SpreadSheetStyle style, ISheet sheet, IWorkbook workbook,
            DateTime from, DateTime to, int colOffset = 0)
        {
            from = RosterManager.SetToTime(from, 0, 0);
            to = RosterManager.SetToTime(to, 23, 59);

            CreateHeadings(style, sheet, from, to);



            styleDict.TryGetValue(nameof(SpreadSheetStyle.nameFieldCl), out var cellStyle);
            styleDict.TryGetValue(nameof(SpreadSheetStyle.rotaFieldCl) + "-wknd", out var wkndFieldStyle);





            styleDict.TryGetValue(nameof(SpreadSheetStyle.nameFieldCl), out var currentStyle);
            

           

            var teamStyle = new TeamValue();

            styleDict.TryGetValue(nameof(SpreadSheetStyle.rotaFieldCl), out var fieldStyle);

            var staffCount = roster.staff.Count;
            var currentRow = (IRow)null;
            var currentCell = (ICell)null;



            maxRowCount = staffCount + 1;
            for (int i = 0; i < staffCount; i++)
            {
                currentRow = sheet.CreateRow(i + 3);
                var currentStaff = roster.staff[i];
                var scheduleCount = currentStaff.schedules.Count;

                currentCell = currentRow.CreateCell(1);
                currentCell.SetCellValue(currentStaff.name);
                currentCell.CellStyle = currentStyle;



                for (int j = 0; j < scheduleCount; j++)
                {
                    var currentSchedule = currentStaff.schedules[j];


                    if (currentSchedule.startDate > to)
                    {
                        break;
                    }
                    else if (currentSchedule.startDate < from)
                    {
                        continue;
                    }

                    currentCell = currentRow.CreateCell(2 - colOffset + (currentSchedule.startDate - roster.start).Days);
                    currentCell.SetCellValue(currentSchedule.startTime + "-" + currentSchedule.endTime);

                    if (currentSchedule.team != null)
                    {


                        if (teamDict.TryGetValue(currentSchedule.team, out teamStyle))
                        {
                            if (IsWeekend(currentSchedule.startDate.DayOfWeek))
                            {
                                if (!styleDict.TryGetValue(currentSchedule.team + "-wknd", out var weekendStyle))
                                {
                                    weekendStyle = (XSSFCellStyle)teamStyle.style.Clone();
                                    weekendStyle.SetFont(weekendFont);

                                    styleDict.Add(currentSchedule.team + "-wknd", weekendStyle);

                                }
                                currentCell.CellStyle = weekendStyle;
                            }
                            else
                            {
                                currentCell.CellStyle = teamStyle.style;
                            }


                            currentCell.CellStyle = teamStyle.style;
                            teamStyle.isUsed = true;
                        }
                        else
                        {
                            currentCell.CellStyle = IsWeekend(currentSchedule.startDate.DayOfWeek) ? wkndFieldStyle : fieldStyle;
                        }
                    }
                    else
                    {
                        currentCell.CellStyle = IsWeekend(currentSchedule.startDate.DayOfWeek) ? wkndFieldStyle : fieldStyle;
                    }


                }//end inner for

                currentCell = currentRow.CreateCell(maxColCount);
                currentCell.SetCellFormula(String.Format("CountA(C{0}:{1}{0})", currentRow.RowNum+1, GetColumnName(maxColCount - 1)));
                currentCell.CellStyle = cellStyle;

            }//end outer for


            if (style.autoNameColWidth)
            {
                sheet.AutoSizeColumn(1);
            }


        }


        /// <summary>
        /// creates a roster table on the sheet
        /// </summary>
        /// <param name="roster">The roster data</param>
        /// <param name="weekendFont">Afont used to flag weekends</param>
        /// <param name="style">The styling options for the table</param>
        /// <param name="sheet">The sheet that the roster will be put on. Must be blank</param>
        /// <param name="workbook">The workbook associated with the sheet</param>
        private void CreateRosterTable(FormattedRoster roster, IFont weekendFont, SpreadSheetStyle style, ISheet sheet, IWorkbook workbook)
        {
            CreateHeadings(style, sheet, roster.start, roster.finish);



            styleDict.TryGetValue(nameof(SpreadSheetStyle.nameFieldCl), out var cellStyle);


            var teamStyle = new TeamValue();

            styleDict.TryGetValue(nameof(SpreadSheetStyle.rotaFieldCl), out var fieldStyle);
            styleDict.TryGetValue(nameof(SpreadSheetStyle.rotaFieldCl) + "-wknd", out var wkndFieldStyle);
           
           

            var staffCount = roster.staff.Count;
            var currentRow = (IRow)null;
            var currentCell = (ICell)null;



            maxRowCount = staffCount + 1;
           
            
            for (int i = 0; i < staffCount; i++)
            {//cellstyle = name field style
                currentRow = sheet.CreateRow(i + 3);
                var currentStaff = roster.staff[i];
                var scheduleCount = currentStaff.schedules.Count;

                currentCell = currentRow.CreateCell(1);
                currentCell.SetCellValue(currentStaff.name);
                currentCell.CellStyle = cellStyle;

                for (int j = 0; j < scheduleCount; j++)
                {
                    var currentSchedule = currentStaff.schedules[j];


                    currentCell = currentRow.CreateCell(2 + (currentSchedule.startDate - roster.start).Days);
                    currentCell.SetCellValue(currentSchedule.startTime + "-" + currentSchedule.endTime);

                    if (currentSchedule.team != null)
                    {
                        if (teamDict.TryGetValue(currentSchedule.team, out teamStyle))
                        {
                            teamStyle.isUsed = true;

                            if (IsWeekend(currentSchedule.startDate.DayOfWeek))
                            {
                                if (!styleDict.TryGetValue(currentSchedule.team + "-wknd", out var weekendStyle))
                                {
                                    weekendStyle = (XSSFCellStyle)teamStyle.style.Clone();
                                    weekendStyle.SetFont(weekendFont);

                                    styleDict.Add(currentSchedule.team + "-wknd", weekendStyle);

                                }
                                currentCell.CellStyle = weekendStyle;
                            }
                            else
                            {
                                currentCell.CellStyle = teamStyle.style;
                            }

                        }
                        else
                        {
                            currentCell.CellStyle = IsWeekend(currentSchedule.startDate.DayOfWeek) ? wkndFieldStyle : fieldStyle;
                        }

                    }
                    else
                    {
                        currentCell.CellStyle = IsWeekend(currentSchedule.startDate.DayOfWeek) ? wkndFieldStyle : fieldStyle;
                    }



                }//end inner for
                currentCell = currentRow.CreateCell(maxColCount);
                
                var formula = String.Format("CountA(C{0}:{1}{0})", currentRow.RowNum+1, GetColumnName(maxColCount - 1));
                currentCell.SetCellFormula(formula);
                currentCell.CellStyle = cellStyle;


            }//end outer for

            if (style.autoNameColWidth)
            {
                sheet.AutoSizeColumn(1);
            }
            
        }

        /// <summary>
        /// Adds a row at the bottom of the table, counting the total number of shifts for each day
        /// </summary>
        /// <param name="style">The styling options for the table</param>
        /// <param name="sheet">The sheet that the count will be put on Must contain table</param>
        private void CreateTotalShiftCount(SpreadSheetStyle style, ISheet sheet)
        {
            maxRowCount++;

            var currentRow = sheet.CreateRow(maxRowCount);
            var currentCell = currentRow.CreateCell(1);
            styleDict.TryGetValue(nameof(SpreadSheetStyle.tlShiftHeadCl), out var cellStyle);
            currentCell.SetCellValue("Total Shifts");
            currentCell.CellStyle = cellStyle;
            
            styleDict.TryGetValue(nameof(SpreadSheetStyle.tlShiftFieldCl), out cellStyle);
            styleDict.TryGetValue(nameof(SpreadSheetStyle.wkndTotalCl), out var wkndStlye);
            var dayRow = sheet.GetRow(1);

            for (int i = 2; i < maxColCount; i++)
            {//cellstye = total shift weekday field style
                currentCell = currentRow.CreateCell(i);
                var currentColName = GetColumnName(i);
                var formula = String.Format("COUNTA({0}4:{0}{1})", currentColName, maxRowCount);
                currentCell.SetCellFormula(formula);

                 currentCell.CellStyle =(IsWeekend(dayRow.GetCell(i).StringCellValue))?wkndStlye: cellStyle;
                
            }

        }

        /// <summary>
        /// Gets the lettered name of any columng
        /// </summary>
        /// <param name="index">The 0-based column index </param>
        /// <returns>Returns the letter name of any given column</returns>
        private string GetColumnName(int index)
        {
            if (index == 0)
            {
                return "A";
            }


            var colName = "";

            if (index > 702)
            {
                var divValue = (index / 676) + 65;
                colName += (char)divValue;
                index -= divValue;
            }

            if (index > 26)
            {
                colName += (char)(index / 26 + 64);
            }
            colName += (char)(index % 26 + 65);
            return colName;
        }

        /// <summary>
        /// (Not Implemented) Supposed to draw all the borders within the table and fill it with the empty style
        /// </summary>
        private void DrawLinesFillEmpties()
        {
            throw new NotImplementedException();
            for (int i = 3; i < maxRowCount; i++)
            {
                for (int j = 2; j < maxColCount; j++)
                {

                }
            }
        }

        /// <summary>
        /// Helper method that creates the heading for the roster table
        /// </summary>
        /// <param name="style">The styling options for the table </param>
        /// <param name="sheet">The sheet which will be ammended</param>
        /// <param name="from">Start point of date headings</param>
        /// <param name="to">End point of date headings</param>
        private void CreateHeadings(SpreadSheetStyle style, ISheet sheet, DateTime from, DateTime to)
        {
            //topmost row, only title uses it
            var currentRow = sheet.CreateRow(0);

            //row where the day names across the top appear
            var dayRow = sheet.CreateRow(1);

            //title style
            styleDict.TryGetValue("title", out  var currentStyle);

            var dateString = from.ToShortDateString() + " to " + to.ToShortDateString();

            
            //title cell
            var currentCell = currentRow.CreateCell(0);
            currentCell.SetCellValue(dateString);

            //merges at the top in 2x2 for the title
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 1, 0, 1));
            currentCell.CellStyle = currentStyle;

            //current row is now the date row
            currentRow = sheet.CreateRow(2);

            //current cell is name heading
            currentCell = currentRow.CreateCell(1);
            currentCell.SetCellValue("Name");

            styleDict.TryGetValue(nameof(SpreadSheetStyle.nameHeadingCl), out currentStyle);

            currentCell.CellStyle = currentStyle;

            //day name style
            styleDict.TryGetValue(nameof(SpreadSheetStyle.dayNameCl), out currentStyle);


            //date heading style
            styleDict.TryGetValue(nameof(SpreadSheetStyle.dateCl), out var currentStyle2);

            styleDict.TryGetValue(nameof(SpreadSheetStyle.wkndDateCl), out var wknDateSt);
            styleDict.TryGetValue(nameof(SpreadSheetStyle.wkndDayCl), out var wknDaySt);

            var colWidth = (int)Math.Round(style.colWidth * 256);
            sheet.SetColumnWidth(0, colWidth);
            sheet.SetColumnWidth(1, colWidth);

            bool useVertHead = style.useVertDates == UseVerticalDates.TRUE || (style.useVertDates == UseVerticalDates.AUTO & style.colWidth < 11.15f);

            nOfDays = (int)(to - from).TotalDays;
            maxColCount = 2 + nOfDays;
            if (useVertHead)
            {
                currentStyle.Rotation = 90;
                currentStyle2.Rotation = 90;
                wknDateSt.Rotation = 90;
                wknDaySt.Rotation = 90;
            }

            for (int i = 0; i <= nOfDays; i++)
            {// currentStyle = date field style, currentStyle2 = day field style

                var currentDate = from.AddDays(i);
                var currentDay = currentDate.DayOfWeek;
                currentCell = dayRow.CreateCell(i + 2);
                currentCell.SetCellValue(Enum.GetName(typeof(DayOfWeek), currentDay));


                var dateCell = currentRow.CreateCell(2 + i);
                dateCell.SetCellValue(currentDate.ToShortDateString());


           
                var isWeekend = IsWeekend(currentDay);

                currentCell.CellStyle =(isWeekend)?wknDaySt: currentStyle;
                dateCell.CellStyle =(isWeekend)?wknDateSt: currentStyle2;

                sheet.SetColumnWidth(i + 2, colWidth);
            }
            maxColCount++;
            currentCell = currentRow.CreateCell(maxColCount);
            currentCell.SetCellValue("Total");
            styleDict.TryGetValue(nameof(SpreadSheetStyle.nameHeadingCl), out currentStyle);
            currentCell.CellStyle = currentStyle;
        }

        /// <summary>
        /// Gets a row from the sheet. If it is null, creates one
        /// </summary>
        /// <param name="rowIndex">0ed Index value for row</param>
        /// <param name="sheet">The sheet from which to get the row</param>
        /// <returns>The row specified by the index</returns>
        private IRow GetRow(int rowIndex, ISheet sheet)
        {
            var row = sheet.GetRow(rowIndex);

            return row != null ? row : sheet.CreateRow(rowIndex);
        }

        /// <summary>
        /// Helper method to build the styles for the sheet. WARNING! does not build weekend team styles
        /// </summary>
        /// <param name="style">The styling options for the table </param>
        /// <param name="italics">The font used to flag weekends</param>
        /// <param name="wBk"></param>
        private void BuildStyleDicts(ref SpreadSheetStyle style, IFont weekendFont, IWorkbook wBk)
        {
            teamDict.Clear();
            styleDict.Clear();



            var fieldFont = wBk.CreateFont();
            fieldFont.FontName = style.font; ;
            fieldFont.FontHeight = style.fontSize;
            fieldFont.IsBold = style.boldFs;
            fieldFont.IsItalic = style.italicFs;
            fieldFont.Underline = style.underLineFs ? FontUnderlineType.Single : FontUnderlineType.None;
            fieldFont.IsStrikeout = style.strikeThroughFs;



            var nextStyle = (XSSFCellStyle)null;




            var headingFont = wBk.CreateFont();
            headingFont.IsBold = style.boldHeadings;
            headingFont.FontHeight = style.fontSize;
            headingFont.FontName = style.font;


            var titleFont = wBk.CreateFont();
            titleFont.IsBold = true;
            titleFont.FontHeight = 19;
            titleFont.FontName = style.font;

            //title style
            nextStyle = AutoStyle(wBk, titleFont, HorizontalAlignment.Left, new XSSFColor(System.Drawing.Color.White));
            styleDict.Add("title", nextStyle);

            // weekdate field style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.dateCl));
            styleDict.Add(nameof(SpreadSheetStyle.dateCl), nextStyle);

            //weekday field style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.dayNameCl));
            styleDict.Add(nameof(SpreadSheetStyle.dayNameCl), nextStyle);

            //weekend date style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.wkndDateCl));
            styleDict.Add(nameof(SpreadSheetStyle.wkndDateCl), nextStyle);

            //weekend day style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.wkndDayCl));
            styleDict.Add(nameof(SpreadSheetStyle.wkndDayCl), nextStyle);

            //rota field style
            nextStyle = AutoStyle(wBk, fieldFont, style.rotaAlign, new XSSFColor(style.rotaFieldCl));
            styleDict.Add(nameof(SpreadSheetStyle.rotaFieldCl), nextStyle);

            //weekend rota field style
            nextStyle = (XSSFCellStyle)nextStyle.Clone();
            nextStyle.SetFont(weekendFont);
        
            styleDict.Add(nameof(SpreadSheetStyle.rotaFieldCl) + "-wknd", nextStyle);

            //empty rota field style
            nextStyle = AutoStyle(wBk, null, HorizontalAlignment.Left, new XSSFColor(style.rotaEmptyCl));
            styleDict.Add(nameof(SpreadSheetStyle.rotaEmptyCl), nextStyle);

            // empty weekend rota field style
            nextStyle = (XSSFCellStyle)nextStyle.Clone();
            nextStyle.SetFont(weekendFont);

            styleDict.Add(nameof(SpreadSheetStyle.rotaEmptyCl) + "-wknd", nextStyle);

            //name field style
            nextStyle = AutoStyle(wBk, fieldFont, style.nameAlign, new XSSFColor(style.nameFieldCl));
            styleDict.Add(nameof(SpreadSheetStyle.nameFieldCl), nextStyle);

            //name heading style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.nameHeadingCl));
            styleDict.Add(nameof(SpreadSheetStyle.nameHeadingCl), nextStyle);

            //Team Legend heading style (can be conditional)
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.teamLegHeadCl));
            styleDict.Add(nameof(SpreadSheetStyle.teamLegHeadCl), nextStyle);

            //style for total shifts (weekday) 
            nextStyle = AutoStyle(wBk, headingFont, style.rotaAlign, new XSSFColor(style.tlShiftFieldCl));
            styleDict.Add(nameof(SpreadSheetStyle.tlShiftFieldCl), nextStyle);

            //style for total shifts (weekend) 
            nextStyle = AutoStyle(wBk, headingFont, style.rotaAlign, new XSSFColor(style.wkndTotalCl));
            styleDict.Add(nameof(SpreadSheetStyle.wkndTotalCl), nextStyle);

            //style for total shifts header
            nextStyle = AutoStyle(wBk, headingFont, style.nameAlign, new XSSFColor(style.tlShiftHeadCl));
            styleDict.Add(nameof(SpreadSheetStyle.tlShiftHeadCl), nextStyle);

            

            //if (!style.useTeamCls) - could either remove completely or add | for using shift analysis
            //{
            //    return;
            //}


            // team style dictionary 

            var brigthnessValid = style.minBrightness > 0 & style.minBrightness < 1;
            for (int i = 0; i < teams.Length; i++)
            {
                var currentTeam = teams[i];
                if (teamDict.ContainsKey(currentTeam.name))
                {
                    continue;
                }

                var colour = GetColourFromHex(currentTeam.colour);
                
                    if (brigthnessValid)
                    {
                        colour = SetMinBrightness(colour, style.minBrightness);
                    
                    }

                    nextStyle = AutoStyle(wBk, fieldFont, style.rotaAlign, colour);
                  var  boldStyle = AutoStyle(wBk,headingFont, style.headAlign, colour);
                
                    teamDict.Add(currentTeam.name, new TeamValue(nextStyle,boldStyle));
            }


        }

        /// <summary>
        /// Generates a summary of each team underneath the roster table
        /// </summary>
        /// <param name="sheet">The sheet which will be ammended</param>
        /// <param name="roster">The roster data</param>
        /// <param name="style">The styling options for the table </param>
        /// <param name="legendOnly"> If true, only shows a legend of teams instead of a summary</param>
        private void GenTeamData(ISheet sheet, FormattedRoster roster, SpreadSheetStyle style, bool legendOnly = false)
        {

            maxRowCount+=2;
          
           
            var currentRow = sheet.CreateRow(maxRowCount);
            var currentCell = currentRow.CreateCell(1);

            
            if (legendOnly)
            {
                currentCell.SetCellValue("Team Legend");
            }
            else
            {
                currentCell.SetCellValue("Team Analysis");
            }



            styleDict.TryGetValue(nameof(SpreadSheetStyle.teamLegHeadCl), out var currentStyle);
            currentCell.CellStyle = currentStyle;

            if (legendOnly)
            {
                maxRowCount++;
                foreach (var entry in teamDict)
                {
                    if (!entry.Value.isUsed)
                    {
                        continue;
                    }
                    
                    currentRow = sheet.CreateRow(maxRowCount);
                    currentCell = currentRow.CreateCell(1);
                    currentCell.SetCellValue(entry.Key);
                    currentCell.CellStyle = entry.Value.styleBold;
                    maxRowCount++;

                    

                }
                return;
            }

            var teamFieldVal = currentRow.RowNum + 1;

            currentCell = currentRow.CreateCell(maxColCount);
            currentCell.SetCellValue("Total");

            currentCell.CellStyle = currentStyle;
            maxRowCount++;
            foreach (var entry in teamDict)
                {
                    if (!entry.Value.isUsed)
                    {
                        continue;
                    }
                 
                    currentRow = sheet.CreateRow(maxRowCount);
                entry.Value.legendPosition = maxRowCount;
                    currentCell = currentRow.CreateCell(1);
                    currentCell.SetCellValue(entry.Key);
                currentCell.CellStyle = entry.Value.styleBold;
                maxRowCount++;
                
                }

            var fieldSize = roster.staff.Count+1;

            for (int i = 2; i < maxColCount; i++)
            {
                for (int j= 3; j < fieldSize; j++)
                {
                    currentCell = sheet.GetRow(j).GetCell(i);
                    if (currentCell == null)
                    {
                        continue;
                    }
                  
                    foreach (var team in teamDict)
                    {
                        if (!team.Value.isUsed)
                        {
                            continue;
                        }
                       
                        var sameRgb =
                            IsSameRGB(currentCell.CellStyle.FillForegroundColorColor.RGB, team.Value.style.FillForegroundColorColor.RGB);
                        if (team.Value.style.FillForegroundColorColor.RGB[0] == 127  || currentCell.CellStyle.FillForegroundColorColor.RGB[0] == 127)
                        {
                            ;
                        }

                        if (sameRgb)
                        {
                            team.Value.dayCount++;
                            break;
                        }
                    }
                    
                }

                foreach (var team in teamDict)
                {
                    if (!team.Value.isUsed)
                    {
                        continue;
                    }

                    currentRow = sheet.GetRow(team.Value.legendPosition);
                    currentCell = currentRow.CreateCell(i);
                    currentCell.CellStyle = team.Value.style;
                    currentCell.SetCellValue(team.Value.dayCount);
                    team.Value.dayCount = 0;
                }
            }


            for (;teamFieldVal<maxRowCount;teamFieldVal++ )
            {
                currentRow = sheet.GetRow(teamFieldVal);
                currentCell = currentRow.CreateCell(maxColCount);
                currentCell.SetCellFormula(String.Format("SUM(C{0}:{1}{0})",currentRow.RowNum+1,GetColumnName(currentCell.ColumnIndex-1)));
                currentCell.CellStyle = currentRow.GetCell(currentCell.ColumnIndex-1).CellStyle;
            }
        }

        /// <summary>
        /// Changes the value of a colour if it is below a minimum value
        /// </summary>
        /// <param name="colour">The colour to be changed</param>
        /// <param name="minBrightness"> The minimum brightness value (0-1)</param>
        /// <remarks>Could use an HSV model to more accurately change the values</remarks>
        /// <returns>The adjusted colour value </returns>
        XSSFColor SetMinBrightness(XSSFColor colour, float minBrightness)
        {
            minBrightness *= 255;
            var rgb = colour.GetRgb();

            var average = (rgb[0] + rgb[1] + rgb[2]) / 3;

            if (average < minBrightness)
            {
                var adjustment = (minBrightness - average) / 3;

                var newRValue = rgb[0] + adjustment;
                var newGValue = rgb[1] + adjustment;
                var newBValue = rgb[2] + adjustment;

                if (newRValue > 255)
                {
                    newGValue += (newRValue - 255) / 2;
                    newBValue += (newRValue - 255) / 2;

                    newRValue = 255;
                }


                if (newGValue > 255)
                {
                    newRValue += (newGValue - 255) / 2;
                    newBValue += (newGValue - 255) / 2;

                    newGValue = 255;
                }


                if (newBValue > 255)
                {
                    newGValue += (newBValue - 255) / 2;
                    newRValue += (newBValue - 255) / 2;

                    newBValue = 255;
                }


                if (newRValue > 255)
                {

                    newRValue = 255;
                }
                if (newGValue > 255)
                {
                    newGValue = 255;
                }
                if (newBValue > 255)
                {
                    newBValue = 255;
                }

                rgb[0] = Convert.ToByte(newRValue);
                rgb[1] = Convert.ToByte(newGValue);
                rgb[2] = Convert.ToByte(newBValue);

                colour = new XSSFColor(rgb);
            }


            return colour;
        }

        /// <summary>
        /// Creates a XSSFColor based on a hex value
        /// </summary>
        /// <param name="hexValue">colour hext value</param>
        /// <returns>Returns an XSSFColor object based on the hex value</returns>
        private XSSFColor GetColourFromHex(string hexValue)
        {
            if (hexValue.Length != 7)
            {
                return null;
            }

            var bytes = new byte[3];


            try
            {
                bytes[0] = Convert.ToByte(hexValue.Substring(1, 2), 16);
                bytes[1] = Convert.ToByte(hexValue.Substring(3, 2), 16);
                bytes[2] = Convert.ToByte(hexValue.Substring(5, 2), 16);
            }
            catch
            {
                return null;
            }

            return new XSSFColor(bytes);
        }

        /// <summary>
        /// The custom settings used to generate a generic style
        /// </summary>
        /// <param name="wbk">The workbook the style is associated to</param>
        /// <param name="font">The font for the style</param>
        /// <param name="alignment">The word alignmkent for the style</param>
        /// <param name="colour">The style colour</param>
        /// <returns>A new cell style object based on the given values</returns>
        private XSSFCellStyle AutoStyle(IWorkbook wbk, IFont font = null, HorizontalAlignment alignment = HorizontalAlignment.Left,
            XSSFColor colour = null)
        {
            var autoValue = (XSSFCellStyle)wbk.CreateCellStyle();
            autoValue.FillPattern = FillPattern.SolidForeground;

            autoValue.BorderBottom = BorderStyle.Thin;
            autoValue.BorderTop = BorderStyle.Thin;
            autoValue.BorderLeft = BorderStyle.Thin;
            autoValue.BorderRight = BorderStyle.Thin;
            
            autoValue.Alignment = alignment;

            if (font != null)
            {
                autoValue.SetFont(font);
            }

            if (colour != null)
            {
                autoValue.SetFillForegroundColor(colour);
            }



            return autoValue;
        }     

        /// <summary>
        /// Helper function to test if 2 rgb values are the same
        /// </summary>
        /// <param name="rgb1">first rgb value</param>
        /// <param name="rgb2">second rgb value</param>
        /// <remarks>Doesn't detect length of array as it assumes it is rgb</remarks>
        /// <returns> True if they are exactly the same</returns>
        bool IsSameRGB(byte[]rgb1, byte[] rgb2)
        {
            for (int i = 0; i < rgb1.Length; i++)
            {
                if (rgb1[i] != rgb2[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Whether the teams have been set or not
        /// </summary>
        public bool TeamsSet
        {
            get
            {
                return setTeams;
            }
        }
    }
}


