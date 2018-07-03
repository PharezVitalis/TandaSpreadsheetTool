
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
            workbook = new Workbook();
            worksheet = new Worksheet();
            
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
                workbook = app.Workbooks.Add(1);
                worksheet = (Worksheet)workbook.Sheets[1];
            }
            catch
            {
                Console.WriteLine("Something went wrong with creating an Excel sheet");
                return;
            }

            

            worksheet.Cells[0, 0] = roster.start ;

            

            worksheet.Range[worksheet.Cells[0, 0], worksheet.Cells[0, 0]].Merge();

            var lastDate = new DateTime();

            int pointPosition = 0;

         

            for (int i = 0; i < roster.schedules.Count; i++)
            {
                var currentDate = roster.schedules[i].startDate;

                if (lastDate != currentDate)
                {
                    worksheet.Cells[1, pointPosition + 2] = currentDate.Day.ToString();
                    worksheet.Cells[2, pointPosition + 2] = currentDate.ToString("dd/MM/yyyy");
                    pointPosition++;
                    lastDate = currentDate;
                }
            }

            worksheet.Range[worksheet.Cells[1, 2], worksheet.Cells[1, pointPosition + 2]].Interior.Color = headerColours[0];
            worksheet.Range[worksheet.Cells[2, 2], worksheet.Cells[2, pointPosition + 2]].Interior.Color = headerColours[1];

            pointPosition = 0;


            for (int i = 0; i < roster.schedules.Count; i++)
            {
                var currentSchedule = roster.schedules[i];

                for (int j = 0; j < pointPosition+1; j++)
                {
                    if (worksheet.Cells[j,1].)
                }
            }
           
        }

    }
  

}
