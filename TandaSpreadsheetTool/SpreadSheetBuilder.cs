
using System.IO;
using System;
using Newtonsoft;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Office;
using Microsoft.Office.Interop.Excel;
using System.Drawing;

namespace TandaSpreadsheetTool
{
    class SpreadSheetBuilder
    {
        List<FormattedRoster> rosterList;
        private Application app;
        private Workbook workbook;
        private Worksheet worksheet;
        private Range range;
        private Color[] headerColours = { Color.LawnGreen, Color.SkyBlue, Color.SlateBlue, Color.PaleGreen, Color.LightGreen };

        public SpreadSheetBuilder()
        {
            rosterList = new List<FormattedRoster>();
           // workbook = new Workbook();
          //  worksheet = new Worksheet();
            
        }

        public void AddRoster(FormattedRoster roster)
        {
            rosterList.Add(roster);
        }
        public void RemoveRoster(FormattedRoster roster)
        {
            rosterList.Remove(roster);
        }


        public void CreateDocument()
        {
            var roster = new FormattedRoster();

            if (rosterList.Count < 1)
            {
                return;
            }
            else
            {
                roster = rosterList[0];
            }

            try
            {
                app = new Application();
                app.ScreenUpdating = false;
                app.Visible = true;

                //remove this line, for testing only - slows down the process a great deal
                app.ScreenUpdating = true;

                workbook = app.Workbooks.Add(1);
                worksheet = (Worksheet)workbook.Sheets[1];
            }
            catch
            {
                Console.WriteLine("Something went wrong with creating an Excel sheet");
                return;
            }

            

            worksheet.Cells[1, 1].Value = roster.start ;

            

            worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, 2]].Merge();

            var lastDate = new DateTime(1970,1,1);

            int pointPosition = 0;

         

            for (int i = 0; i < roster.schedules.Count; i++)
            {
                var currentDate = roster.schedules[i].startDate;

                if ((currentDate - lastDate).Days>0)
                {
                    worksheet.Cells[2, pointPosition + 4] = GetDay(currentDate.DayOfWeek);
                    worksheet.Cells[3, pointPosition + 4] = currentDate.ToString("dd/MM/yyyy");
                    pointPosition++;
                    lastDate = currentDate;
                }
            }
            

            worksheet.Range[worksheet.Cells[2, 2], worksheet.Cells[2, pointPosition + 3]].Interior.Color = headerColours[0];
            worksheet.Range[worksheet.Cells[3, 2], worksheet.Cells[3, pointPosition + 3]].Interior.Color = headerColours[1];
            
            pointPosition = 0;


            for (int i = 0; i < roster.schedules.Count; i++)
            {
                var nameIndex = -1;
                var currentSchedule = roster.schedules[i];
                var scheduleX = 2;
                

                for (int j = 1; j < pointPosition; j++)
                {
                    var cellValue = Convert.ToString(worksheet.Cells[j + 4, 2].Value);

                    if (cellValue == currentSchedule.staff)
                    {
                        nameIndex = j;
                        break;
                    }
                }
                

                if (nameIndex == -1)
                {
                    pointPosition++;
                    worksheet.Cells[pointPosition + 3, 2] = currentSchedule.staff;
                    nameIndex = pointPosition;
                }
               
                
                    while (currentSchedule.startDate.ToShortDateString() != Convert.ToString(worksheet.Cells[3,scheduleX].Value))
                {
                    scheduleX++;
                }
                var newCellValue = (currentSchedule.team)+": "+ currentSchedule.startTime + " - " + currentSchedule.endTime;
                
                worksheet.Cells[nameIndex+3, scheduleX] = newCellValue;
            }

            
            
           
        }

        public string GetDay(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Friday:
                    return "Friday";
                case DayOfWeek.Monday:
                    return "Monday";
                case DayOfWeek.Saturday:
                    return "Saturday";
                case DayOfWeek.Sunday:
                    return "Sunday";
                case DayOfWeek.Thursday:
                    return "Thursday";
                case DayOfWeek.Tuesday:
                    return "Tuesday";
                default:
                    return "Wednesday";
            }
        }

    }
  

}
