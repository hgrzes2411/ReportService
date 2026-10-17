using System;
using System.Collections.Generic;
namespace ReportService.Models.Domains
{
    internal class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public bool IsSend { get; set; }
        public List<ReportPosition> Positions { get; set; }
    }
}
