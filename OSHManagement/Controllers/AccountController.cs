using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace OSHManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly OshDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(OshDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var employee = await _context.Employees
                    .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                    .FirstOrDefaultAsync(e => e.PayrollNo == model.PayrollNo);

                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid payroll number or password.");
                    return View(model);
                }

                // Verify password (you'll need to implement proper password hashing)
                bool passwordValid = VerifyPassword(model.Password, employee.PasswordHash, employee.LegacyPassword);

                if (!passwordValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid payroll number or password.");
                    return View(model);
                }

                // TODO: Implement proper authentication with ASP.NET Core Identity or Cookie Authentication
                // For now, store user info in session
                HttpContext.Session.SetString("PayrollNo", employee.PayrollNo);
                HttpContext.Session.SetString("FullName", $"{employee.FirstName} {employee.LastName}");
                HttpContext.Session.SetInt32("EmployeeId", employee.EmployeeId);

                _logger.LogInformation($"User {employee.PayrollNo} logged in successfully");

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.PayrollNo == model.PayrollNumber && e.EmailAddress == model.Email);

                if (employee == null)
                {
                    // Don't reveal that the user doesn't exist
                    ViewBag.Message = "If your account exists, you will receive a password reset link at your email.";
                    return View();
                }

                // TODO: Implement password reset token generation and email sending
                _logger.LogInformation($"Password reset requested for {employee.PayrollNo}");

                ViewBag.Message = "If your account exists, you will receive a password reset link at your email.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // TODO: Implement token validation and password reset
                _logger.LogInformation($"Password reset attempted for {model.PayrollNumber}");

                ViewBag.Message = "Your password has been reset successfully. Please login with your new password.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            // TODO: Check if user is authenticated
            var payrollNo = HttpContext.Session.GetString("PayrollNo");
            if (string.IsNullOrEmpty(payrollNo))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var payrollNo = HttpContext.Session.GetString("PayrollNo");
                if (string.IsNullOrEmpty(payrollNo))
                {
                    return RedirectToAction("Login");
                }

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.PayrollNo == payrollNo);

                if (employee == null)
                {
                    return RedirectToAction("Login");
                }

                // Verify current password
                if (!VerifyPassword(model.CurrentPassword, employee.PasswordHash, employee.LegacyPassword))
                {
                    ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                    return View(model);
                }

                // Hash and update new password
                employee.PasswordHash = HashPassword(model.NewPassword);
                employee.LegacyPassword = null; // Clear legacy password after update
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed for {employee.PayrollNo}");

                ViewBag.Message = "Your password has been changed successfully.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during change password");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helper Methods

        private bool VerifyPassword(string password, string? passwordHash, string? legacyPassword)
        {
            // Check modern hash first
            if (!string.IsNullOrEmpty(passwordHash))
            {
                var computedHash = HashPassword(password);
                if (computedHash == passwordHash)
                {
                    return true;
                }
            }

            // Fall back to legacy password (plain text comparison for migration)
            if (!string.IsNullOrEmpty(legacyPassword))
            {
                return password == legacyPassword;
            }

            return false;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        #endregion
    }
}
