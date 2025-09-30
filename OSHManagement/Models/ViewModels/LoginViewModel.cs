using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Payroll Number is required")]
        [Display(Name = "Payroll Number")]
        public string PayrollNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
