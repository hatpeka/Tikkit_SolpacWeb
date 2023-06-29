using System.ComponentModel.DataAnnotations;

namespace Tikkit_SolpacWeb.Models
{
    public class Users
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Partner { get; set; }
        public string Address { get; set; }
        public string Sex { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string? RePassword { get; set; }
        public string Role { get; set; }
        public string Status { get; set; } = "Working";
    }
}
