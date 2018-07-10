
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System;
using System.Collections.Generic;

namespace TandaSpreadsheetTool
{
    class SpreadSheetBuilder
    {

        Team[] teams;
        Dictionary<string, XSSFCellStyle> teamColourDict;

        public SpreadSheetBuilder(Team[] teams)
        {
            this.teams = teams;
        }

        public void CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style,  SpreadSheetDiv div = SpreadSheetDiv.NONE, string path  = null)
        {
            var workBook = new XSSFWorkbook();
            
            var nameHeadCl = new XSSFColor(style.nameHeadingCl);
            var nameFieldCL = new XSSFColor(style.nameFieldCl);
            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);
            var dayNameCL = new XSSFColor(style.dayNameCl);
            var dateCl = new XSSFColor(style.dateCl);

            if (teamColourDict.Count < 1 && teams.Length > 0 && style.useTeamCls)
            {
                try
                {
                    for (int i = 0; i < teams.Length; i++)
                    {
                        var currentTeam = teams[i];
                        var nextStyle = AutoStyle(workBook);
                        var colour = GetColourFromHex(currentTeam.colour);
                        if (colour != null)
                        {
                            nextStyle.SetFillForegroundColor(colour);
                        }
                       
                        teamColourDict.Add(currentTeam.name, nextStyle);
                    }
                }
                catch
                {
                    style.useTeamCls = false;
                }


              }

            var dateString = roster.start.ToString("dd-MM") + "- " + roster.finish.ToString("dd-MM-yy");


            var sheet = workBook.CreateSheet("Schedule " + dateString);
      
            var currentRow = sheet.CreateRow(0);
            var dayRow = sheet.CreateRow(1);

            var cellStyle = AutoStyle(workBook, false);
            var cellStyle2 = AutoStyle(workBook, false);

            var nOfDays = (roster.finish - roster.start).TotalDays;
            var staffCount = roster.staff.Count;


            var currentCell = currentRow.CreateCell(0);
            currentCell.SetCellValue(dateString);

            

            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 1, 0, 1));
          

            //Title font
            var font = workBook.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 25;
            font.FontName = "Arial";
            currentCell.CellStyle.SetFont(font);


           currentRow = sheet.CreateRow(2);
           
            currentCell = currentRow.CreateCell(1);
            currentCell.SetCellValue("Name");

          
            

            
          
            font = workBook.CreateFont();
            font.IsBold = style.boldHeadings;
            font.FontName = "Arial";
            cellStyle.SetFont(font);
            cellStyle.SetFillForegroundColor(nameHeadCl);
            currentCell.CellStyle = cellStyle;

            cellStyle2.SetFont(font);
            
            cellStyle2.SetFillForegroundColor(dayNameCL);
           


            cellStyle = (XSSFCellStyle)workBook.CreateCellStyle();
            cellStyle.SetFont(font);
            cellStyle.FillPattern = FillPattern.SolidForeground;
            cellStyle.SetFillForegroundColor(dateCl);

            

            sheet.SetColumnWidth(0, style.colWidth * 256);
            sheet.SetColumnWidth(1, style.colWidth * 256);

            for (int i = 0; i <= nOfDays; i++)
            {
                sheet.SetColumnWidth(i + 2, style.colWidth * 256);
               
                var currentDate = roster.start.AddDays(i);
                currentCell = dayRow.CreateCell(2 + i);
                currentCell.SetCellValue(Enum.GetName(typeof(DayOfWeek),currentDate.DayOfWeek));
                currentCell.CellStyle = cellStyle2;


                currentCell = currentRow.CreateCell(2 + i);
                currentCell.SetCellValue(currentDate.ToShortDateString());
                currentCell.CellStyle = cellStyle;

            }

            
            cellStyle = AutoStyle(workBook);
            cellStyle.SetFillForegroundColor(nameFieldCL);

            for (int i = 0; i < staffCount; i++)
            {
                currentRow = sheet.CreateRow(i+3);
                var currentStaff = roster.staff[i];
                var scheduleCount = currentStaff.schedules.Count;
                currentCell = currentRow.CreateCell(1);

                currentCell.SetCellValue(currentStaff.name);
                currentCell.CellStyle = cellStyle;

                for (int j = 0; j < scheduleCount; j++)
                {
                    var currentSchedule = currentStaff.schedules[j];
                    currentCell =
                   
                }
            }
           

            if (path == null)
            {
                path = RosterManager.Path;
            }
            
                var fs = File.Create(path+"Tanda Roster  " + roster.start.ToString("dd-MM-yy") + " - " + roster.finish.ToString("dd-MM-yy")+".xlsx");
                workBook.Write(fs);
                fs.Close();

            font = null;
            workBook = null;
            currentRow = null;
            currentCell = null;
            cellStyle = null;
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

        private XSSFCellStyle AutoStyle(XSSFWorkbook wbk, bool autoFont = true)
        {
            var autoValue = (XSSFCellStyle)wbk.CreateCellStyle();
            autoValue.FillPattern = FillPattern.SolidForeground;

            autoValue.BorderBottom = BorderStyle.Thin;
            autoValue.BorderTop = BorderStyle.Thin;
            autoValue.BorderLeft = BorderStyle.Thin;
            autoValue.BorderRight = BorderStyle.Thin;

            if (autoFont)
            {
                var font = wbk.CreateFont();
                font.FontName = "Calibru";
                font.FontHeightInPoints = 11;

                autoValue.SetFont(font);
            }
            


            return autoValue;
        }
      
    }
}


