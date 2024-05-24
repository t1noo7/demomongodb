using System.ComponentModel.DataAnnotations;

namespace DemoMongoDB.ModelViews
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(100)]
        [Required(ErrorMessage ="Pls Enter your Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Display(Name ="Email")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Pls Enter your password")]
        [MinLength(5,ErrorMessage = "Your minimum password must be 5 characters!")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
