using System.ComponentModel.DataAnnotations;

namespace Tikkit_SolpacWeb.Models
{
    public class Requests
    {
        [Key]
        public int RequestNo { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string? RequestPerson { get; set; }
        public string? Partner { get; set; }
        public string? EndUser { get; set; }
        public string? SoftwareProduct { get; set; }
        public string? ProgramName { get; set; }
        public string? CurrentSituation { get; set; }
        public string? ContentsOfRequest { get; set; }
        public string? CorrectSituation { get; set; }
        public string? Contact { get; set; }
        public string? Type { get; set; }

    }
}
