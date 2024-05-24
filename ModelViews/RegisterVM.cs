using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DemoMongoDB.ModelViews
{
    public class RegisterVM
    {
        [Key]
        [Display(Name = "Full name")]
        [Required(ErrorMessage = "Pls Enter your Full name")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Pls Enter your Email")]
        [MaxLength(150)]
        [DataType(DataType.EmailAddress)]
        [Remote(action: "ValidateEmail", controller: "Accounts")]
        public string Email { get; set; }
        [MaxLength(11)]
        [Required(ErrorMessage = "Pls Enter your Phone no.")]
        [Display(Name = "Phone no.")]
        [DataType(DataType.PhoneNumber)]
        [Remote(action: "ValidatePhone", controller: "Accounts")]
        public string Phone { get; set; }
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Pls Enter your Password")]
        [MinLength(5, ErrorMessage = "Your minium password must be 5 characters!")]
        public string Password { get; set; }
        [MinLength(5, ErrorMessage = "Your minimum password must be 5 characters!")]
        [Display(Name = "Re-enter your Password")]
        [Compare("Password", ErrorMessage = "Pls Enter the same as the previous")]
        public string ConfirmPassword { get; set; }
    }
}
