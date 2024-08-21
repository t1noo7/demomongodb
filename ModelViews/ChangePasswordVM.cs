using System.ComponentModel.DataAnnotations;

namespace DemoMongoDB.ModelViews
{
    public class ChangePasswordVM
    {
        [Key]
        public int CustomerId { get; set; }
        [Display(Name = "Current Password")]
        public string PasswordNow { get; set; }
        [Display(Name = "New Password")]
        [Required(ErrorMessage = "Pls enter your password")]
        [MinLength(5, ErrorMessage = "Your minimum password must be 5 characters!")]
        public string Password { get; set; }
        [MinLength(5, ErrorMessage = "Your minimum password must be 5 characters!")]
        [Display(Name = "Comfirm new password")]
        [Compare("Password", ErrorMessage = "The confirm password does not match with the password")]
        public string ConfirmPassword { get; set; }
    }
}
