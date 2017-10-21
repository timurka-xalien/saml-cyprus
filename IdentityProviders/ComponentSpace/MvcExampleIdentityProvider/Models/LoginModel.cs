using System.ComponentModel.DataAnnotations;

namespace MvcExampleIdentityProvider.Models
{
    public class LoginModel
    {
        public LoginModel()
        {
        }

        public LoginModel(string userName)
        {
            this.UserName = userName;
        }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
