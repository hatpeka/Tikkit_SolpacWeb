using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace Tikkit_SolpacWeb.Models
{
    public class Requests
    {
        public enum RequestStatus
        {
            Pending,
            Processing,
            Done
        }

        [Key]
        public int RequestNo { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime DeadlineDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan TotalTime { get; set; }
        public string? Priority { get; set; }

        public string? RequestPerson { get; set; }
        public string? CreatePerson { get; set; }
        public string? Supporter { get; set; }

        public string? Partner { get; set; }
        public string? Project { get; set; }
        public string? SubjectOfRequest { get; set; }
        public string? ContentsOfRequest { get; set; }
        public string? Reason { get; set; }
        public string? SupportContent { get; set; }
        public string? Contact { get; set; }
        public string? Type { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;


        public Requests()
        {
            DeadlineDate = CalculateDeadlineDate();
        }

        private DateTime CalculateDeadlineDate()
        {
            if (Priority?.ToLower() == "urgent")
            {
                return RequestDate;
            }
            return RequestDate.AddDays(1);
        }

    }
}
