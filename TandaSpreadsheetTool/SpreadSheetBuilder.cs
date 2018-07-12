
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
        bool dictSetUp = false;

        Dictionary<string, XSSFCellStyle> teamColourDict;


        public SpreadSheetBuilder()
        {
            teamColourDict = new Dictionary<string, XSSFCellStyle>();
        }

        public void SetTeams(Team[] teams)
        {
            this.teams = teams;
            setTeams = true;
            dictSetUp = false;
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
            
            if (style.useTeamCls & !dictSetUp)
            {
                CreateStyleDict(ref style, workBook, fieldFont);
            }
            var sheet = (ISheet)null;

            switch (style.divBy)
            {
                case SpreadSheetDiv.NONE:
                    var dateString = roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy");
                    sheet = workBook.CreateSheet("Schedule " + dateString);
                    
                    break;
                case SpreadSheetDiv.WEEKLY:
                    break;
                case SpreadSheetDiv.BIWEEKLY:
                    break;
                case SpreadSheetDiv.MONTHLY:
                    break;
                default:
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
            
                var fs = File.Create(path + fileName);
            
            
                
                workBook.Write(fs);
                fs.Close();


           
           

            
        }

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, DateTime from, DateTime to, IFont titleFont, 
            IFont headingsFont, IFont fieldFont, XSSFWorkbook workbook)
        {


            CreatHeadings(style, sheet, from, to, titleFont, headingsFont, workbook);

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


        }        

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, XSSFWorkbook workbook, IFont titleFont, 
            IFont headingsFont, IFont fieldFont, string dateString = null)
        {
            CreatHeadings(style, sheet, roster.start, roster.finish, titleFont, headingsFont, workbook);

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
        }

        private void CreatHeadings(SpreadSheetStyle style, ISheet sheet, DateTime from, DateTime to, IFont titleFont, IFont headingFont, XSSFWorkbook workbook)
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

            //date heading style
            var currentStyle2 = AutoStyle(workbook, headingFont);
            currentStyle2.SetFillForegroundColor(new XSSFColor(style.dateCl));

            var colWidth = style.colWidth * 256;
            sheet.SetColumnWidth(0, colWidth);
            sheet.SetColumnWidth(1, colWidth);

            var nOfDays = (int)(from - to).TotalDays;

            for (int i = 0; i <= nOfDays; i++)
            {// currentStyle = date field style, currentStyle2 = day field style
                sheet.SetColumnWidth(i + 2, colWidth);

                var currentDate = from.AddDays(i);
                currentCell = dayRow.CreateCell(i + 2);
                currentCell.SetCellValue(Enum.GetName(typeof(DayOfWeek), currentDate.DayOfWeek));
                currentCell.CellStyle = currentStyle2;

                currentCell = currentRow.CreateCell(2 + i);
                currentCell.SetCellValue(currentDate.ToShortDateString());
                currentCell.CellStyle = currentStyle;
            }
        }

        private IRow GetRow(int rowIndex, ISheet sheet)
        {
            var row = sheet.GetRow(rowIndex);

            return row != null ? row : sheet.CreateRow(rowIndex);
        }

        private ICell GetCell(IRow row, int columnIndex)
        {
            var cell = row.GetCell(columnIndex);

            return cell != null? cell:row.CreateCell(columnIndex);
            
        }

        private void CreateStyleDict(ref SpreadSheetStyle style, XSSFWorkbook wBk, IFont fieldFont)
        {

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
            dictSetUp = true;


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
            autoValue.SetFont(font);




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


