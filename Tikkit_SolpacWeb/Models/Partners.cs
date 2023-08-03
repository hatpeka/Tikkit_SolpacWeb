using System.ComponentModel.DataAnnotations;

namespace Tikkit_SolpacWeb.Models
{
    public class Partners
    {
        [Key]
        public int PartnerID { get; set; }
        public string Name { get; set; }
        public virtual List<Projects>? Projects { get; set; }
    }
}
