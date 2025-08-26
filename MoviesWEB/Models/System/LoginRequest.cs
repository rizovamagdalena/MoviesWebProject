﻿using System.ComponentModel.DataAnnotations;

namespace MoviesWEB.Models.System
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
