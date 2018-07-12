
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace TandaSpreadsheetTool
{
    class SpreadSheetBuilder
    {

        Team[] teams;
        bool setTeams = false;
      

        Dictionary<string, XSSFCellStyle> teamColourDict;
        Dictionary<string, XSSFCellStyle> styleDict;

        public SpreadSheetBuilder()
        {
            teamColourDict = new Dictionary<string, XSSFCellStyle>();
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


        public void CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style,  string path  = null, string fileName = null)
        {
            var workBook = new XSSFWorkbook();
            
            
          
            var titleFont = workBook.CreateFont();
            titleFont.IsBold = true;
            titleFont.FontHeightInPoints = 22;
            titleFont.FontName = style.font;

            var headingFont = workBook.CreateFont();
            headingFont.IsBold = style.boldHeadings;
            headingFont.FontName = style.font;

            var fieldFont = workBook.CreateFont();
            fieldFont.FontName = style.font;
            
            if (style.useTeamCls)
            {
                BuildStyleDicts(ref style, workBook, fieldFont);
            }
            var sheet = (ISheet)null;

            var nOfDaysToAdd = -1;

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
                    if ((roster.finish- roster.start).TotalDays > 14)
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

            switch (nOfDaysToAdd)
            {
                case 0:
                    sheet = workBook.CreateSheet("Schedule " + roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy"));
                    CreateRosterTable(roster, style, sheet, workBook, titleFont, headingFont, fieldFont);
                    break;

                case -1:
                    return;

                case 30:
                   
                    DateTime toDate = roster.start;
                    
                    while (toDate <= roster.finish)
                    {
                        var fromDate = toDate;
                        while (roster.start.Month == toDate.Month)
                        {
                            toDate = toDate.AddDays(1);
                        }
                        sheet = workBook.CreateSheet("Schedule " + fromDate.ToString("dd-MM-yy") + " to " + toDate.ToString("dd-MM-yy"));
                        CreateRosterTable(roster, style, sheet, fromDate, toDate, titleFont, headingFont, fieldFont, workBook);
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

                        sheet = workBook.CreateSheet("Schedule " + currentDate.ToString("dd-MM-yy") + nextDate.ToString("dd-MM-yy"));
                        CreateRosterTable(roster, style, sheet, currentDate, nextDate, titleFont, headingFont, fieldFont, workBook);
                        currentDate = nextDate;
                    }
                    while (currentDate < roster.finish);

                    break;
                    
            }


            if (path == null)
            {
                path = SpreadSheetPath;
            }
            // add try- catch here

            if (fileName == null)
            {
                fileName = "Tanda Roster  " + roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy") + ".xlsx";
            }
            try
            {
                var fs = File.Create(path + fileName);
                workBook.Write(fs);
                fs.Close();
            }
            catch
            {

            }
        }

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, DateTime from, DateTime to, IFont titleFont, 
            IFont headingsFont, IFont fieldFont, XSSFWorkbook workbook)
        {


            CreateHeadings(style, sheet, from, to, titleFont, headingsFont, workbook);

            var currentStyle = AutoStyle(workbook, fieldFont);
            currentStyle.SetFillForegroundColor(new XSSFColor(style.nameFieldCl));

            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);

            var teamStyle = (XSSFCellStyle)null;

            var fieldStyle = AutoStyle(workbook, fieldFont);
            fieldStyle.SetFillForegroundColor(new XSSFColor(style.rotaFieldCl));

            var staffCount = roster.staff.Count;
            var currentRow = (IRow)null;
            var currentCell = (ICell)null;

           
           

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
                    if ( currentSchedule.startDate > to)
                    {
                        break;
                    }
                    else if (currentSchedule.startDate< from)
                    {
                        continue;
                    }

                    currentCell = currentRow.CreateCell(2 + (currentSchedule.startDate - roster.start).Days);
                    currentCell.SetCellValue(currentSchedule.team + ": " + currentSchedule.startTime + " - " + currentSchedule.endTime);

                    if (style.useTeamCls)
                    {
                        if (teamColourDict.TryGetValue(currentSchedule.team, out teamStyle))
                        {
                            currentCell.CellStyle = teamStyle;
                        }
                        else
                        {
                            currentCell.CellStyle = fieldStyle;
                        }
                    }
                }//end inner for
            }//end outer for

            var emptyStyle = AutoStyle(workbook, null);
            emptyStyle.SetFillBackgroundColor(new XSSFColor(style.rotaEmptyCl));
            //DrawLinesFillEmpties(emptyStyle, sheet, 2, j + 2, 3, i + 3);
        }        

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, XSSFWorkbook workbook, IFont titleFont, 
            IFont headingsFont, IFont fieldFont)
        {
            CreateHeadings(style, sheet, roster.start, roster.finish, titleFont, headingsFont, workbook);

            var currentStyle = AutoStyle(workbook, fieldFont);
            currentStyle.SetFillForegroundColor(new XSSFColor(style.nameFieldCl));

            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);

            var teamStyle = (XSSFCellStyle)null;

            var fieldStyle = AutoStyle(workbook, fieldFont);
            fieldStyle.SetFillForegroundColor(new XSSFColor(style.rotaFieldCl));

            var staffCount = roster.staff.Count;
            var currentRow = (IRow)null;
            var currentCell = (ICell)null;
            
            


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
                    currentCell = currentRow.CreateCell(2 + (currentSchedule.startDate - roster.start).Days);
                    currentCell.SetCellValue(currentSchedule.team + ": " + currentSchedule.startTime + " - " + currentSchedule.endTime);

                    if (style.useTeamCls)
                    {
                        if (teamColourDict.TryGetValue(currentSchedule.team, out teamStyle))
                        {
                            currentCell.CellStyle = teamStyle;
                        }
                        else
                        {
                            currentCell.CellStyle = fieldStyle;
                        }
                    }
                }//end inner for
            }//end outer for

            var emptyStyle = AutoStyle(workbook, null);
            emptyStyle.SetFillBackgroundColor(new XSSFColor(style.rotaEmptyCl));
           // DrawLinesFillEmpties(emptyStyle, sheet, 2, colCount + 2, 3, rowCount + 3);

        }

        private void DrawLinesFillEmpties(ICellStyle emptyStyle, ISheet sheet, int fromCol,int toCol, int fromRow, int toRow)
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

        private void CreateHeadings(SpreadSheetStyle style, ISheet sheet, DateTime from, DateTime to, IFont titleFont, IFont headingFont, XSSFWorkbook workbook)
        {
            //topmost row, only title uses it
            var currentRow = sheet.CreateRow(0);

            //row where the day names across the top appear
            var dayRow = sheet.CreateRow(1);
            //title style
            var currentStyle = AutoStyle(workbook, titleFont);

            var dateString = from.ToShortDateString() + " to " + to.ToShortDateString();

            //title cell
            var currentCell = currentRow.CreateCell(0);
            currentCell.SetCellValue(dateString);

            //merges at the top in 2x2 for the title
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 1, 0, 1));
            currentStyle.SetFillForegroundColor(new XSSFColor(Color.White));
            currentCell.CellStyle = currentStyle;

            //current row is now the date row
            currentRow = sheet.CreateRow(2);

            //current cell is name heading
            currentCell = currentRow.CreateCell(1);
            currentCell.SetCellValue("Name");

            currentStyle = AutoStyle(workbook, headingFont);
            currentStyle.SetFillForegroundColor(new XSSFColor(style.nameHeadingCl));
            currentCell.CellStyle = currentStyle;

            //day name style
            currentStyle = AutoStyle(workbook, headingFont);
            currentStyle.SetFillForegroundColor(new XSSFColor(style.dayNameCl));

            //date heading style
            var currentStyle2 = AutoStyle(workbook, headingFont);
            currentStyle2.SetFillForegroundColor(new XSSFColor(style.dateCl));

            var colWidth = style.colWidth * 256;
            sheet.SetColumnWidth(0, colWidth);
            sheet.SetColumnWidth(1, colWidth);

            var nOfDays = (int)(to-from).TotalDays;

            for (int i = 0; i <= nOfDays; i++)
            {// currentStyle = date field style, currentStyle2 = day field style
                sheet.SetColumnWidth(i + 2, colWidth);

                var currentDate = from.AddDays(i);
                currentCell = dayRow.CreateCell(i + 2);
                currentCell.SetCellValue(Enum.GetName(typeof(DayOfWeek), currentDate.DayOfWeek));
                currentCell.CellStyle = currentStyle;

                currentCell = currentRow.CreateCell(2 + i);
                currentCell.SetCellValue(currentDate.ToShortDateString());
                currentCell.CellStyle = currentStyle2;
            }
        }

        private IRow GetRow(int rowIndex, ISheet sheet)
        {
            var row = sheet.GetRow(rowIndex);

            return row != null ? row : sheet.CreateRow(rowIndex);
        }

      

        private void BuildStyleDicts(ref SpreadSheetStyle style, XSSFWorkbook wBk, IFont fieldFont)
        {
            teamColourDict.Clear();
            styleDict.Clear();
            try
            {
                bool brigthnessValid = style.minBrightness > 0 & style.minBrightness < 1;
                for (int i = 0; i < teams.Length; i++)
                {
                    var currentTeam = teams[i];
                    var nextStyle = AutoStyle(wBk, fieldFont);
                    var colour = GetColourFromHex(currentTeam.colour);
                   
                    if (colour != null)
                    {
                        if (brigthnessValid)
                        {
                            if (colour.Tint < style.minBrightness)
                            {
                                
                                colour.Tint = style.minBrightness;
                            }
                        }
                        nextStyle.SetFillForegroundColor(colour);
                    }

                    teamColourDict.Add(currentTeam.name, nextStyle);
                }
            }
            catch
            {
                style.useTeamCls = false;
            }
            // add font size options?
            var headingFont = wBk.CreateFont();
            headingFont.IsBold = style.boldHeadings;
            headingFont.FontName = style.font;

            var titleFont = wBk.CreateFont();
            titleFont.IsBold = true;
            titleFont.FontName = style.font;

          


        }

        private XSSFColor GetColourFromHex(string hexValue)
        {
            if (hexValue.Length!= 7)
            {
                return null;
            }

            var bytes = new byte[3];

            
            try
            {
                bytes[0] = Convert.ToByte(hexValue.Substring(1, 2),16);
                bytes[1] = Convert.ToByte(hexValue.Substring(3, 2), 16);
                bytes[2] = Convert.ToByte(hexValue.Substring(5, 2), 16);
            }
            catch
            {
                return null;
            }

            return new XSSFColor(bytes);
        }


        private XSSFCellStyle AutoStyle(XSSFWorkbook wbk, IFont font)
        {
            var autoValue = (XSSFCellStyle)wbk.CreateCellStyle();
            autoValue.FillPattern = FillPattern.SolidForeground;

            autoValue.BorderBottom = BorderStyle.Thin;
            autoValue.BorderTop = BorderStyle.Thin;
            autoValue.BorderLeft = BorderStyle.Thin;
            autoValue.BorderRight = BorderStyle.Thin;
            if (font != null)
            {
                autoValue.SetFont(font);
            }
            




            return autoValue;
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


