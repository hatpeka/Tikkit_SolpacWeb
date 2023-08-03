using System.ComponentModel.DataAnnotations;

namespace Tikkit_SolpacWeb.Models
{
    public class RequestsCreateViewModel
    {
        public string? Priority { get; set; }
        [Required(ErrorMessage = "Người yêu cầu là bắt buộc.")]
        public int? RequestPersonID { get; set; }
        public int PartnerID { get; set; }
        [Required(ErrorMessage = "Dự án là bắt buộc.")]
        public string? Project { get; set; }
        [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
        public string? SubjectOfRequest { get; set; }
        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        public string? ContentsOfRequest { get; set; }
        public string? ImagePath { get; set; }
    }
}
