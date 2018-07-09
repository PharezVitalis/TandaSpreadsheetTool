
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System;

namespace TandaSpreadsheetTool
{
    class SpreadSheetBuilder
    {
        
        


        public void CreateWorkbook(FormattedRoster roster, SpreadSheetStyle style, SpreadSheetDiv div = SpreadSheetDiv.NONE, string path  = null)
        {
            var workBook = new XSSFWorkbook();

            var nameHeadCl = new XSSFColor(style.nameHeadingCl);
            var nameFieldCL = new XSSFColor(style.nameFieldCl);
            var rotaFieldCl = new XSSFColor(style.rotaFieldCl);
            var dayNameCL = new XSSFColor(style.dayNameCl);
            var dateCl = new XSSFColor(style.dateCl);

            var dateString = roster.start.ToString("dd-MM") + "- " + roster.finish.ToString("dd-MM-yy");


            var sheet = workBook.CreateSheet("Schedule " + dateString);
      
            var currentRow = sheet.CreateRow(0);
            var dayRow = sheet.CreateRow(1);
            var cellStyle2 = AutoStyle(workBook, false);
          

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

            var cellStyle = AutoStyle(workBook,false);
            

            
          
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

            var nOfDays = (roster.finish - roster.start).TotalDays;

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
            font = null;

            var staffCount = roster.staff.Count;

           

            if (path == null)
            {
                path = RosterManager.Path;
            }
            
                var fs = File.Create(path+"Tanda Roster  " + roster.start.ToString("dd-MM-yy") + " - " + roster.finish.ToString("dd-MM-yy")+".xlsx");
                workBook.Write(fs);
                fs.Close();

            workBook = null;
            currentRow = null;
            currentCell = null;
            cellStyle = null;
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


