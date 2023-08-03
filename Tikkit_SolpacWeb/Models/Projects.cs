using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tikkit_SolpacWeb.Models
{
    public class Projects
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public int PartnerID { get; set; }
        [ForeignKey("PartnerID")]
        public virtual Partners? Partners { get; set; }
    }
}
