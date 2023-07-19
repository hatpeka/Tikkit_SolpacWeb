using System.ComponentModel.DataAnnotations;

namespace Tikkit_SolpacWeb.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public int? Target { get; set; }
        public DateTime CreateTime { get; set; }
        public string Title { get; set; }
        public int RequestID { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
