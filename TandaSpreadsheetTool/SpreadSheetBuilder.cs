
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


        public static byte[] ChangeColorBrightness(byte[] rgb, float correctionFactor)
        {
            float red = rgb[0];
            float green = rgb[1];
            float blue = rgb[2];

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            rgb[0] = Convert.ToByte(red);
            rgb[1] = Convert.ToByte(blue);
            rgb[2] = Convert.ToByte(green);

            return rgb;
        }


        public void CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style,  SpreadSheetDiv div = SpreadSheetDiv.NONE, string path  = null,
            bool filterTeamNames = true)
        {
            var workBook = new XSSFWorkbook();
            
            var nameHeadCl = new XSSFColor(style.nameHeadingCl);
            var nameFieldCL = new XSSFColor(style.nameFieldCl);
            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);
            var dayNameCL = new XSSFColor(style.dayNameCl);
            var dateCl = new XSSFColor(style.dateCl);

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
                CreateStyleDict(ref style, workBook);
            }
            

            var dateString = roster.start.ToString("dd-MM-yy") + " to " + roster.finish.ToString("dd-MM-yy");


            var sheet = workBook.CreateSheet("Schedule " + dateString);
      
            //current row = top row, only title uses this row
            var currentRow = sheet.CreateRow(0);

            //row where the day names across the top appear
            var dayRow = sheet.CreateRow(1);

            //style for title
            var cellStyle = AutoStyle(workBook,titleFont);
            
            //style for name headings
            var cellStyle2 = AutoStyle(workBook,headingFont);

            var nOfDays = (roster.finish - roster.start).TotalDays;
            var staffCount = roster.staff.Count;

            //title cell
            var currentCell = currentRow.CreateCell(0);
            currentCell.SetCellValue(dateString);

            
            //top left corner where title is, merges 2x2
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 1, 0, 1));
            cellStyle.SetFillForegroundColor(new XSSFColor(System.Drawing.Color.White));
            currentCell.CellStyle = cellStyle;

            // current row is now date row
           currentRow = sheet.CreateRow(2);
           
            currentCell = currentRow.CreateCell(1);
            currentCell.SetCellValue("Name");




            //style for name heading
            cellStyle = AutoStyle(workBook, headingFont);           
            cellStyle.SetFillForegroundColor(nameHeadCl);
            currentCell.CellStyle = cellStyle;
            
            //style for day name headers
            cellStyle2.SetFillForegroundColor(dayNameCL);          

            //style for date headers
            cellStyle = AutoStyle(workBook,headingFont);            
            cellStyle.SetFillForegroundColor(dateCl);

            
       
            sheet.SetColumnWidth(0, style.colWidth * 256);
            sheet.SetColumnWidth(1, style.colWidth * 256);


            //populates the day and date rows
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

            //style for the name fields
            cellStyle = AutoStyle(workBook);
            cellStyle.SetFillForegroundColor(nameFieldCL);

            //rota field colour,in case there are people with no teams
            var rotaFieldColour = new XSSFColor(style.rotaFieldCl);

            //populates the rotafields
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

                    currentCell = currentRow.CreateCell(2 +  ( currentSchedule.startDate- roster.start).Days);
                    currentCell.SetCellValue(currentSchedule.team + ": " + currentSchedule.startTime + " - " + currentSchedule.endTime);

                   if (!teamColourDict.TryGetValue(currentSchedule.team,out cellStyle2))
                    {
                        currentCell.CellStyle = cellStyle;
                    }
                    else
                    {
                        currentCell.CellStyle = cellStyle2;
                    }
                   
                    
                }
            }
           
            if (filterTeamNames)
            {
                foreach (var team in teamColourDict)
                {

                }
            }

            if (path == null)
            {
                path = SpreadSheetPath;
            }
            // add try- catch here
                var fs = File.Create(path + "Tanda Roster  " + roster.start.ToString("dd-MM-yy") + " - " + roster.finish.ToString("dd-MM-yy") + ".xlsx");
            
            
                
                workBook.Write(fs);
                fs.Close();


           
            workBook = null;
            currentRow = null;
            currentCell = null;
            cellStyle = null;
            titleFont = null;
            headingFont = null;
            fieldFont = null;

            
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

        private void CreateStyleDict(ref SpreadSheetStyle style, XSSFWorkbook wBk)
        {

            try
            {
                for (int i = 0; i < teams.Length; i++)
                {
                    var currentTeam = teams[i];
                    var nextStyle = AutoStyle(wBk);
                    var colour = GetColourFromHex(currentTeam.colour);
                   
                    if (colour != null)
                    {
                        if (style.minBrightness > 0 )
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

        private XSSFCellStyle AutoStyle(XSSFWorkbook wbk)
        {
            var autoValue = (XSSFCellStyle)wbk.CreateCellStyle();
            autoValue.FillPattern = FillPattern.SolidForeground;

            autoValue.BorderBottom = BorderStyle.Thin;
            autoValue.BorderTop = BorderStyle.Thin;
            autoValue.BorderLeft = BorderStyle.Thin;
            autoValue.BorderRight = BorderStyle.Thin;

            
            


            return autoValue;
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


