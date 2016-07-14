using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models
{
    public class EditUserModel : IUser
    {
        public string Create { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string IsAdmin { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        bool IUser.IsAdmin => !string.IsNullOrEmpty(IsAdmin);
    }

    public class UserLoginModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
