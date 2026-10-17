using ReportService.Models.Domains;
using System;
using System.Collections.Generic;

namespace ReportService.Repositories
{
    internal class ReportRepository
    {
        public Report GetLastNotSendReport()
        {
            //pobieranie z bazy danych ostatniego raportu
            return new Report
            {
                Id = 1,
                Title = "R/1/2021",
                Date = new DateTime(2021, 1, 1, 12, 0, 0),
                Positions = new List<ReportPosition>
                {
                    new ReportPosition
                    {
                        Id =1,
                        ReportId = 1,
                        Title = "Position 1",
                        Description = "Description 1",
                        Value = 43.01M
                    },

                      new ReportPosition
                    {
                        Id =2,
                        ReportId = 1,
                        Title = "Position 2",
                        Description = "Description 2",
                        Value = 3.01M
                    },

                        new ReportPosition
                    {
                        Id =3,
                        ReportId = 3,
                        Title = "Position 3",
                        Description = "Description 3",
                        Value = 423.01M
                    }
                }
            };
        }

        public void ReportSent(Report report)
        {
            report.IsSend = true;
            //zapis w bazie danych
        }
    }
}
