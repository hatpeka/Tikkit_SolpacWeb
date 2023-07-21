using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace Tikkit_SolpacWeb.Models
{
    public class Requests
    {

        [Key]
        public int RequestNo { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime DeadlineDate
        {
            get
            {
                if (Priority == "Khẩn cấp")
                {
                    return RequestDate;
                }
                return RequestDate.AddDays(1);
            }
        }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TotalTime
        {
            get
            {
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    TimeSpan totalTime = EndDate.Value - StartDate.Value;
                    return $"{totalTime.Hours}h:{totalTime.Minutes}m:{totalTime.Seconds}s";
                }

                return "0h:0m:0s";
            }
        }
        public string? Priority { get; set; }

        public string? RequestPerson { get; set; }
        public int? RequestPersonID { get; set; }
        public string? CreatePerson { get; set; }
        public string? Supporter { get; set; }

        public string? Partner { get; set; }
        public string? Project { get; set; }
        public string? SubjectOfRequest { get; set; }

        public string? ContentsOfRequest { get; set; }
        public string? ImagePath { get; set; }
        public string? Reason { get; set; }
        public string? SupportContent { get; set; }
        public string? Contact { get; set; }
        public string? Status { get; set; } = "Đang chờ";
    }
}
