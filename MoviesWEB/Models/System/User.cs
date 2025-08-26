using System.ComponentModel.DataAnnotations;

namespace MoviesWEB.Models.System
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }


    }

    public class UserProfile
    {
        public string Name { get; set; }

       
        public string Phone { get; set; }

    
        public string Username { get; set; }
    }

    public class SendRequest
    {

        public string username { get; set; }

        public List<int> SelectedSeatsId { get; set; } = new List<int>();
    }
}
