
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TandaSpreadsheetTool
{
    class SpreadSheetBuilder
    {

        Team[] teams;
        bool setTeams = false;
        //  need to make these parameters
        int maxColCount;
        int maxRowCount;
        int nOfDays;
       


        Dictionary<string, TeamValue> teamDict;
        Dictionary<string, XSSFCellStyle> styleDict;
        INotifiable form;

        public SpreadSheetBuilder(INotifiable form)
        {
            this.form = form;
            teamDict = new Dictionary<string, TeamValue>();
            styleDict = new Dictionary<string, XSSFCellStyle>();

        }

        public void SetTeams(Team[] teams)
        {
            this.teams = teams;
            setTeams = true;

        }

        public static string SpreadSheetPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Tanda Rosters\\";
            }
        }

        bool IsWeekend(DayOfWeek day)
        {
            return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
        }

        public bool CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style, string path = null)
        {
            var workBook = (IWorkbook)new XSSFWorkbook();
            maxColCount = 0;
            maxRowCount = 0;
            
            form.EnableNotifiers();
            form.UpdateProgress("Building Spreadsheet...");

            form.UpdateProgress("Building styles");
            BuildStyleDicts(ref style, workBook);

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
                        nOfDaysToAdd = 7;
                    }
                    else
                    {
                        nOfDaysToAdd = 0;
                    }

                    break;
                case SpreadSheetDiv.BIWEEKLY:
                    if ((roster.finish - roster.start).TotalDays > 14)
                    {
                        nOfDaysToAdd = 14;
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
                    CreateRosterTable(roster, style, sheet, workBook);
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


                        CreateRosterTable(roster, style, sheet, workBook, fromDate, toDate.AddDays(-1), (fromDate - roster.start).Days - 1);

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

                        CreateRosterTable(roster, style, sheet, workBook, currentDate, nextDate, (currentDate - roster.start).Days);
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
                form.DisableNotifiers();
                return false;
            }
            workBook.Close();
            teamDict.Clear();
            styleDict.Clear();
            form.UpdateProgress("Finished creating excel sheet at: " + path);
            form.DisableNotifiers();
            GC.Collect();
            return true;            
        }

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, IWorkbook workbook,
            DateTime from, DateTime to, int colOffset = 0)
        {
            from = RosterManager.SetToTime(from, 0, 0);
            to = RosterManager.SetToTime(to, 23, 59);

            CreateHeadings(style, sheet, from, to);



            styleDict.TryGetValue(nameof(style.nameFieldCl), out var cellStyle);






            styleDict.TryGetValue(nameof(style.nameFieldCl), out var currentStyle);
            currentStyle.SetFillForegroundColor(new XSSFColor(style.nameFieldCl));

            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);

            var teamStyle = new TeamValue();

            styleDict.TryGetValue(nameof(style.rotaFieldCl), out var fieldStyle);

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
                    currentCell.SetCellValue(currentSchedule.team + ": " + currentSchedule.startTime + " - " + currentSchedule.endTime);


                    if (teamDict.TryGetValue(currentSchedule.team, out teamStyle))
                    {
                        currentCell.CellStyle = teamStyle.style;
                        teamStyle.isUsed = true;
                    }
                    else
                    {
                        currentCell.CellStyle = fieldStyle;
                    }



                }//end inner for
            }//end outer for
            //should be done inside of create workbook
            CreateTotalShiftCount(style, sheet);
            

        }

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, IWorkbook workbook)
        {
            CreateHeadings(style, sheet, roster.start, roster.finish);



            styleDict.TryGetValue(nameof(style.nameFieldCl), out var cellStyle);


            var teamStyle = new TeamValue();

            styleDict.TryGetValue(nameof(style.rotaFieldCl), out var fieldStyle);

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
                currentCell.CellStyle = cellStyle;

                for (int j = 0; j < scheduleCount; j++)
                {
                    var currentSchedule = currentStaff.schedules[j];

                    //if (currentSchedule.startDate < roster.start)
                    //{
                    //    continue;
                    //}



                    currentCell = currentRow.CreateCell(2 + (currentSchedule.startDate - roster.start).Days);
                    currentCell.SetCellValue(currentSchedule.team + ": " + currentSchedule.startTime + " - " + currentSchedule.endTime);


                    if (teamDict.TryGetValue(currentSchedule.team, out teamStyle))
                    {
                        currentCell.CellStyle = teamStyle.style;
                        teamStyle.isUsed = true;
                    }
                    else
                    {
                        currentCell.CellStyle = fieldStyle;
                    }




                }//end inner for
            }//end outer for






            CreateTotalShiftCount(style, sheet);

        }

        private void CreateTotalShiftCount(SpreadSheetStyle style, ISheet sheet)
        {
            maxRowCount++;

            var currentRow = sheet.CreateRow(maxRowCount);
            var currentCell = currentRow.CreateCell(1);
            styleDict.TryGetValue(nameof(style.tlShiftHeadCl), out var cellStyle);
            currentCell.SetCellValue("Total Shifts");
            currentCell.CellStyle = cellStyle;

            cellStyle = styleDict[nameof(style.tlShiftFieldCl)];
            styleDict.TryGetValue(nameof(style.tlShiftFieldCl),)
           
            

            for (int i = 2; i <= maxColCount; i++)
            {//cellstye = total shift weekday field style
                currentCell = currentRow.CreateCell(i);
                var currentColName = GetColumnName(i);
                var formula = String.Format("COUNTA({0}4:{0}{1})", currentColName, maxRowCount);
                currentCell.SetCellFormula(formula);
                currentCell.CellStyle = cellStyle;
            }

        }


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

        private void DrawLinesFillEmpties(XSSFCellStyle emptyStyle, ISheet sheet, int fromCol, int toCol, int fromRow, int toRow)
        {

            for (int i = fromRow; i <= toRow; i++)
            {
                var currentRow = GetRow(i, sheet);

                for (int j = fromCol; j < toCol; j++)
                {
                    var currentCell = currentRow.GetCell(j);
                    if (currentCell == null)
                    {
                        currentCell = currentRow.CreateCell(j);
                        currentCell.CellStyle = emptyStyle;
                    }

                }
            }
        }

        private void CreateHeadings(SpreadSheetStyle style, ISheet sheet, DateTime from, DateTime to)
        {
            //topmost row, only title uses it
            var currentRow = sheet.CreateRow(0);

            //row where the day names across the top appear
            var dayRow = sheet.CreateRow(1);
            //title style
            var currentStyle = (XSSFCellStyle)null;

            styleDict.TryGetValue("title", out currentStyle);

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

            styleDict.TryGetValue(nameof(style.nameHeadingCl), out currentStyle);

            currentCell.CellStyle = currentStyle;

            //day name style
            styleDict.TryGetValue(nameof(style.dayNameCl), out currentStyle);


            //date heading style
            styleDict.TryGetValue(nameof(style.dateCl), out var currentStyle2);

            styleDict.TryGetValue(nameof(style.wkndDateCl), out var wknDateSt);
            styleDict.TryGetValue(nameof(style.wkndDateCl), out var wknDaySt);

            var colWidth = (int)Math.Round(style.colWidth * 256);
            sheet.SetColumnWidth(0, colWidth);
            sheet.SetColumnWidth(1, colWidth);

            bool useVertHead = style.useVertDates == UseVerticalDates.TRUE || style.useVertDates == UseVerticalDates.AUTO & style.colWidth < 11.15f;

            nOfDays = (int)(to - from).TotalDays;
            maxColCount = 2 + nOfDays;
            for (int i = 0; i <= nOfDays; i++)
            {// currentStyle = date field style, currentStyle2 = day field style

                var currentDate = from.AddDays(i);
                var currentDay = currentDate.DayOfWeek;
                currentCell = dayRow.CreateCell(i + 2);
                currentCell.SetCellValue(Enum.GetName(typeof(DayOfWeek), currentDay));


                var dateCell = currentRow.CreateCell(2 + i);
                dateCell.SetCellValue(currentDate.ToShortDateString());


                if (useVertHead)
                {
                    currentStyle.Rotation = 90;
                    currentStyle2.Rotation = 90;
                }
                var isWeekend = IsWeekend(currentDay);

                currentCell.CellStyle =(isWeekend)?wknDaySt: currentStyle;
                dateCell.CellStyle =(isWeekend)?wknDateSt: currentStyle2;

                sheet.SetColumnWidth(i + 2, colWidth);
            }

        }

        private IRow GetRow(int rowIndex, ISheet sheet)
        {
            var row = sheet.GetRow(rowIndex);

            return row != null ? row : sheet.CreateRow(rowIndex);
        }

        private void BuildStyleDicts(ref SpreadSheetStyle style, IWorkbook wBk)
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
            styleDict.Add(nameof(style.dateCl), nextStyle);

            //weekday field style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.dayNameCl));
            styleDict.Add(nameof(style.dayNameCl), nextStyle);

            //weekend date style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.wkndDateCl));
            styleDict.Add(nameof(style.wkndDateCl), nextStyle);

            //weekend day style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.wkndDayCl));
            styleDict.Add(nameof(style.wkndDayCl), nextStyle);

            //rota field style
            nextStyle = AutoStyle(wBk, fieldFont, style.nameAlign, new XSSFColor(style.rotaFieldCl));
            styleDict.Add(nameof(style.rotaFieldCl), nextStyle);

            //empty rota field style
            nextStyle = AutoStyle(wBk, null, HorizontalAlignment.Left, new XSSFColor(style.rotaEmptyCl));
            styleDict.Add(nameof(style.rotaEmptyCl), nextStyle);

            //name field style
            nextStyle = AutoStyle(wBk, fieldFont, style.nameAlign, new XSSFColor(style.nameFieldCl));
            styleDict.Add(nameof(style.nameFieldCl), nextStyle);

            //name heading style
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.nameHeadingCl));
            styleDict.Add(nameof(style.nameHeadingCl), nextStyle);

            //Team Legend heading style (can be conditional)
            nextStyle = AutoStyle(wBk, headingFont, style.headAlign, new XSSFColor(style.teamLegHeadCl));
            styleDict.Add(nameof(style.teamLegHeadCl), nextStyle);

            //style for total shifts (weekday only, may need to create weekend version)
            nextStyle = AutoStyle(wBk, headingFont, style.rotaAlign, new XSSFColor(style.tlShiftFieldCl));
            styleDict.Add(nameof(style.tlShiftFieldCl), nextStyle);

            //style for total shifts header
            nextStyle = AutoStyle(wBk, headingFont, style.nameAlign, new XSSFColor(style.tlShiftHeadCl));
            styleDict.Add(nameof(style.tlShiftHeadCl), nextStyle);

            

            //if (!style.useTeamCls) - could either remove completely or add | for using shift analysis
            //{
            //    return;
            //}




            bool brigthnessValid = style.minBrightness > 0 & style.minBrightness < 1;
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

        private void GenTeamData(ISheet sheet, FormattedRoster roster, SpreadSheetStyle style, bool legendOnly = false)
        {

            maxRowCount++;
            sheet.CreateRow(maxRowCount);
            maxRowCount++;
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

            styleDict.TryGetValue(nameof(style.teamLegHeadCl), out var currentStyle);
            currentCell.CellStyle = currentStyle;
            maxRowCount++;

            var teamOffset = 0;


            

            if (legendOnly)
            {
                foreach (var entry in teamDict)
                {
                    if (!entry.Value.isUsed)
                    {
                        continue;
                    }
                    teamOffset--;
                    currentRow = sheet.CreateRow(maxRowCount);
                    currentCell = currentRow.CreateCell(1);
                    currentCell.SetCellValue(entry.Key);
                    currentCell.CellStyle = entry.Value.styleBold;
                    maxRowCount++;

                    

                }
                return;
            }

            int usedTeamCount = 0;    
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
                usedTeamCount++;
                }

            var fieldSize = roster.staff.Count+1;

            for (int i = 2; i <= maxColCount; i++)
            {
                for (int j= 3; j <= fieldSize; j++)
                {
                    currentCell = sheet.GetRow(j).GetCell(i);
                    if (currentCell == null)
                    {
                        continue;
                    }
                    
                    foreach (var team in teamDict)
                    {
                        bool sameRgb =
                            IsSameRGB(currentCell.CellStyle.FillForegroundColorColor.RGB, team.Value.style.FillForegroundColorColor.RGB);
                        if (sameRgb ||string.Equals( team.Key, TeamFromCell(style.teamNameSep, currentCell)))
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

        }

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

        public string TeamFromCell(char seperator,ICell cell)
        {
            if (cell.CellType != CellType.String)
            {
                return null;
            }
            var cellStr = cell.StringCellValue;
            var subIndex = 0;
            for ( ; subIndex < cellStr.Length; subIndex++)
            {
                if(cellStr[subIndex] == seperator)
                {
                    
                    break;
                }
            }

            return cellStr.Substring(0,subIndex);
        }

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

        public bool TeamsSet
        {
            get
            {
                return setTeams;
            }
        }
    }
}


