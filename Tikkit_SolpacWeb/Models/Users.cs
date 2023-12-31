﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tikkit_SolpacWeb.Models
{
    public class Users
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public int PartnerID { get; set; }
        public string? Partner { get; set; }
        public string Address { get; set; }
        public string Sex { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string? RePassword { get; set; }
        public string Role { get; set; }
        public string Status { get; set; } = "Working";

        [NotMapped]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [NotMapped]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [NotMapped]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}