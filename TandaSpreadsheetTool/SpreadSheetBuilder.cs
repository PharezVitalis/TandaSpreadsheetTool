
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
        INotifiable form;

        public SpreadSheetBuilder(INotifiable form)
        {
            this.form = form;
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


        public bool CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style,  string path  = null)
        {
            var workBook = (IWorkbook)new XSSFWorkbook();

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
            form.UpdateProgress("Building cell data");
           
            switch (nOfDaysToAdd)
            {
                case 0:
                    sheet = workBook.CreateSheet(roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy"));
                    CreateRosterTable(roster, style, sheet, workBook);
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
                       
                        
                        CreateRosterTable(roster, style, sheet, workBook, fromDate, toDate.AddDays(-1),(fromDate-roster.start).Days-1);
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
                        
                        CreateRosterTable(roster, style, sheet, workBook, currentDate, nextDate,(currentDate-roster.start).Days);
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
            catch(Exception e)
            {
                form.UpdateProgress("Failed to save file: " + e.Message);
                form.RaiseMessage("Failed To save Excel File", "Excel File failed to save: " + e.Message);
                form.DisableNotifiers();
                    return false;
            }
            workBook.Close();
            form.UpdateProgress("Finished creating excel sheet at: " + path);
            form.DisableNotifiers();
            return true;
        }

        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, IWorkbook workbook, 
            DateTime from, DateTime to, int colOffset = 0 )
        {
          from=  RosterManager.SetToTime(from, 0, 0);
           to = RosterManager.SetToTime(to, 23, 59);

            CreateHeadings(style, sheet, from, to);

           
            

            styleDict.TryGetValue(nameof(style.nameFieldCl), out var currentStyle);
            currentStyle.SetFillForegroundColor(new XSSFColor(style.nameFieldCl));

            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);

            var teamStyle = (XSSFCellStyle)null;

            styleDict.TryGetValue(nameof(style.rotaFieldCl), out var fieldStyle);

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

                    currentCell = currentRow.CreateCell(2-colOffset + (currentSchedule.startDate - roster.start).Days);
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
        
        private void CreateRosterTable(FormattedRoster roster, SpreadSheetStyle style, ISheet sheet, IWorkbook workbook)
        {
            CreateHeadings(style, sheet, roster.start, roster.finish);

      

            styleDict.TryGetValue(nameof(style.nameFieldCl), out var currentStyle);

            

            var teamStyle = (XSSFCellStyle)null;

            styleDict.TryGetValue(nameof(style.rotaFieldCl), out var fieldStyle);

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

                    if (currentSchedule.startDate < roster.start)
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
                    else
                    {
                        currentCell.CellStyle = fieldStyle;
                    }
                }//end inner for
            }//end outer for

           
           // DrawLinesFillEmpties(sheet, 2, colCount + 2, 3, rowCount + 3);

        }

        private void DrawLinesFillEmpties( XSSFCellStyle emptyStyle, ISheet sheet, int fromCol,int toCol, int fromRow, int toRow)
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

        private void BuildStyleDicts(ref SpreadSheetStyle style, IWorkbook wBk)
        {
            teamColourDict.Clear();
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
            nextStyle = AutoStyle(wBk, titleFont);
            nextStyle.SetFillForegroundColor(new XSSFColor(Color.White));
            styleDict.Add("title", nextStyle);

            // date field style
            nextStyle = AutoStyle(wBk, headingFont);
            nextStyle.SetFillForegroundColor(new XSSFColor(style.dateCl));
            nextStyle.Alignment = style.headAlign;
            styleDict.Add(nameof(style.dateCl), nextStyle);

            //day field style
            nextStyle = AutoStyle(wBk, headingFont);
            nextStyle.SetFillForegroundColor(new XSSFColor(style.dayNameCl));
            nextStyle.Alignment = style.headAlign;
            styleDict.Add(nameof(style.dayNameCl), nextStyle);

            //rota field style
            nextStyle = AutoStyle(wBk, fieldFont);
            nextStyle.SetFillForegroundColor(new XSSFColor(style.rotaFieldCl));
            nextStyle.Alignment = style.nameAlign;
            styleDict.Add(nameof(style.rotaFieldCl), nextStyle);

            //empty rota field style
            nextStyle = AutoStyle(wBk);
            nextStyle.SetFillForegroundColor(new XSSFColor(style.rotaEmptyCl));
            styleDict.Add(nameof(style.rotaEmptyCl), nextStyle);

            //name field style
            nextStyle = AutoStyle(wBk, fieldFont);
            nextStyle.SetFillForegroundColor(new XSSFColor(style.nameFieldCl));
            nextStyle.Alignment = style.nameAlign;
            styleDict.Add(nameof(style.nameFieldCl), nextStyle);

            //name heading style
            nextStyle = AutoStyle(wBk, headingFont);
            nextStyle.SetFillForegroundColor(new XSSFColor(style.nameHeadingCl));
            nextStyle.Alignment = style.headAlign;
            styleDict.Add(nameof(style.nameHeadingCl), nextStyle);

            if (!style.useTeamCls)
            {
                return;
            }

            
            
           
                bool brigthnessValid = style.minBrightness > 0 & style.minBrightness < 1;
                for (int i = 0; i < teams.Length; i++)
                {
                    var currentTeam = teams[i];
                    nextStyle = AutoStyle(wBk, fieldFont);
                    nextStyle.Alignment = style.nameAlign;
                    var colour = GetColourFromHex(currentTeam.colour);
                   
                    if (colour != null)
                    {
                        if (brigthnessValid)
                        {


                        colour = SetMinBrightness(colour, style.minBrightness);
                                
                           
                        }
                        nextStyle.SetFillForegroundColor(colour);
                    }
                    if (!teamColourDict.ContainsKey(currentTeam.name))
                {
                    teamColourDict.Add(currentTeam.name, nextStyle);
                }
                    
                }
            
            
        }

       XSSFColor SetMinBrightness(XSSFColor colour, float minBrightness)
        {
            minBrightness *= 255;
            var rgb = colour.GetRgb();

            var average = (rgb[0] + rgb[1] + rgb[2])/3;

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


        private XSSFCellStyle AutoStyle(IWorkbook wbk, IFont font=null)
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


