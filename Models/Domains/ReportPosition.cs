using System;
namespace ReportService.Models.Domains
{
    internal class ReportPosition
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }
}
