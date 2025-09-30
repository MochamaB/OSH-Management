using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Payroll Number is required")]
        [Display(Name = "Payroll Number")]
        public string PayrollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
