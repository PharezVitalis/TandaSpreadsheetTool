
using System.IO;
using System;
using Microsoft.Office;
using Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;


namespace TandaSpreadsheetTool
{
    class SpreadSheetBuilder
    {
       

      
        
        private Application app;
        private Workbook workbook;
        private Worksheet worksheet;

        public SpreadSheetBuilder()
        {
            var officeType = Type.GetTypeFromProgID("Excel.Applcation");
            if (officeType == null)
            {
               
                return;
            }


            workbook = new Workbook();
            worksheet = new Worksheet();
            
        }

       


        public SpreadSheetStatus CreateDocument(FormattedRoster roster)
        {
            var officeType = Type.GetTypeFromProgID("Excel.Applcation");
            if (officeType == null)
            {
                return SpreadSheetStatus.NOTINSTALLED;
            }

            try
            {
                if (app == null)
                {
                    //  var excelProcess = Process.GetProcessesByName("excel");


                    try
                    {
                        app = (Application)Marshal.GetActiveObject("Excel.Application");
                    }
                    catch
                    {
                        app = new Application();
                    }

                }

                //  app.ScreenUpdating = false;
                app.Visible = true;

                //remove this line, for testing only - slows down the process a great deal
                app.ScreenUpdating = true;

                workbook = app.Workbooks.Add(1);
                worksheet = (Worksheet)workbook.Sheets[1];
            }
            catch
            {
                Console.WriteLine("Something went wrong with creating an Excel sheet");
                return SpreadSheetStatus.FAILED;
            }



            worksheet.Cells[1, 1].Value = "Roster: " + roster.start.ToString("dd/MM/yyyy") + " - " + roster.finish.ToShortDateString();

            var cellHeading = worksheet.Cells[3, 2];
            cellHeading.Value = "Name";
            cellHeading.Font.Bold = true;
            cellHeading.Interior.Color = ColorTranslator.FromHtml("#3d85c6");

            var titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, 2]];

            titleRange.Merge();
            var titleFont = titleRange.Font;

            titleFont.Bold = true;
            titleFont.Size = 18;

            var lastDate = new DateTime(1970, 1, 1);






            int columnCount= Convert.ToInt32((roster.finish - roster.start).TotalDays) +2;

            for (int i = 1; i < columnCount; i++)
            {
                var date = roster.start.AddDays(i);
                worksheet.Cells[2, i + 3] = Enum.GetName(typeof(DayOfWeek), date.DayOfWeek);
                worksheet.Cells[3, i + 3] = date.ToShortDateString();
            }

            var  pointPosition = 4;


            for (int i = 0; i < roster.staff.Count; i++)
            {
                var currentStaff = roster.staff[i];
                worksheet.Cells[pointPosition, 2] = currentStaff.name;

                foreach (var schedule in currentStaff.schedules)
                {
                    int columnPos = (schedule.startDate - roster.start).Days + 3;
                    var cell = worksheet.Cells[pointPosition, columnPos].
                    cell.Value = schedule.team + ": " + schedule.startTime + " - " + schedule.endTime;
                    cell.Interior.Color = ColorTranslator.FromHtml(schedule.teamColour);
                }

                for (int j = 3; j < columnCount; j++)
                {
                    var interior = worksheet.Cells[pointPosition, j].interior;

                          if (Convert.ToInt32(interior.Color) ==16777215 )
                          {
                              interior.color = Color.FromArgb(206, 206, 206);
                          }
                }
                pointPosition++;
            }

            pointPosition--;

            worksheet.Range[worksheet.Cells[4, 2], worksheet.Cells[pointPosition, 2]].Interior.Color = ColorTranslator.FromHtml("#cfe2f3");

            for (int i = 1; i <= columnCount; i++)
            {
                worksheet.Columns[i].ColumnWidth = 24;
            }
            for (int i = 3; i <= columnCount; i++)
            {
                worksheet.Cells[2, i].Interior.Color = ColorTranslator.FromHtml("#38761d");
                worksheet.Cells[3, i].Interior.Color = ColorTranslator.FromHtml("#93c47d");
            }

            worksheet.Cells[2, 1].EntireRow.Font.Bold = true;
            worksheet.Cells[3, 1].EntireRow.Font.Bold = true;

              var tableRange = worksheet.Cells.Range[worksheet.Cells[2, 2], worksheet.Cells[ pointPosition , columnCount]];



            foreach (var cell in tableRange)
            {
                var dRAlignL = cell.Borders[XlBordersIndex.xlEdgeLeft];
                var dRAlignR = cell.Borders[XlBordersIndex.xlEdgeRight];
                var dRAlignU = cell.Borders[XlBordersIndex.xlEdgeTop];
                var dRAlignD = cell.Borders[XlBordersIndex.xlEdgeBottom];

                dRAlignL.Color = Color.Black;
                dRAlignR.Color = Color.Black;
                dRAlignU.Color = Color.Black;
                dRAlignD.Color = Color.Black;

                dRAlignU = null;
                dRAlignR = null;
                dRAlignL = null;
                dRAlignD = null;

            }








            var tableRangeStyle = tableRange.Style;

            tableRangeStyle.HorizontalAlignment = XlHAlign.xlHAlignLeft;




            titleRange = null;
            titleFont = null;
            tableRange = null;

            return SpreadSheetStatus.DONE;

            // WARNING MEMORY LEAKS, SET ALL EXCEL VARIABLES TO NULL
        }

        ~SpreadSheetBuilder()
        {
            //any class property from excel must be disposed here
            if (app != null)
            {
                Marshal.ReleaseComObject(app);
            }
            
            if (workbook != null)
            {
                Marshal.ReleaseComObject(workbook);
            }
            
            if (worksheet != null)
            {
                Marshal.ReleaseComObject(worksheet);
            }


            
        }

        
    }
  

}
