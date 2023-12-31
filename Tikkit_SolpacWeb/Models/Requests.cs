﻿using Microsoft.AspNetCore.Mvc;
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
            set { }
        }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? TotalTime { get; set; } = TimeSpan.Zero;
        public string? Priority { get; set; }
        public string? RequestPerson { get; set; }
        public int? RequestPersonID { get; set; }
        public string? CreatePerson { get; set; }
        public string? Supporter { get; set; }
        public int? SupporterID { get; set; }
        public string? Partner { get; set; }
        public string? Project { get; set; }
        public string? SubjectOfRequest { get; set; }
        public string? ContentsOfRequest { get; set; }
        public string? WordPath { get; set; }
        public string? Reason { get; set; }
        public string? SupportContent { get; set; }
        public string? Contact { get; set; }
        public string? Status { get; set; } = "Đang chờ";
        public string? CancelReason { get; set; }
    }
}
