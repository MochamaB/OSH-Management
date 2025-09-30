using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OSHManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly Services.IAuthenticationService _authenticationService;
        private readonly OshDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            Services.IAuthenticationService authenticationService,
            OshDbContext context,
            ILogger<AccountController> logger)
        {
            _authenticationService = authenticationService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            Console.WriteLine("===== LOGIN GET ACTION CALLED =====");
            _logger.LogInformation("Login GET action called");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine("===== LOGIN POST ACTION CALLED =====");
            Console.WriteLine($"PayrollNo: {model?.PayrollNo ?? "NULL"}");
            Console.WriteLine($"Password: {(string.IsNullOrEmpty(model?.Password) ? "EMPTY" : "PROVIDED")}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            _logger.LogInformation($"Login POST action called for PayrollNo: {model?.PayrollNo}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("===== MODEL STATE INVALID =====");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                Console.WriteLine("===== CALLING AUTHENTICATION SERVICE =====");
                var authResult = await _authenticationService.AuthenticateAsync(
                    model.PayrollNo,
                    model.Password);

                Console.WriteLine($"Authentication Result: {authResult.Success}");

                if (!authResult.Success)
                {
                    Console.WriteLine("===== AUTHENTICATION FAILED =====");
                    ModelState.AddModelError(string.Empty, "Invalid payroll number or password.");
                    return View(model);
                }

                Console.WriteLine("===== AUTHENTICATION SUCCESS - CREATING CLAIMS =====");
                // Create claims principal and sign in
                var claimsIdentity = new ClaimsIdentity(authResult.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                };

                Console.WriteLine("===== SIGNING IN USER =====");
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                Console.WriteLine("===== USER SIGNED IN - REDIRECTING TO DASHBOARD =====");
                _logger.LogInformation($"User {model.PayrollNo} logged in successfully");

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"===== EXCEPTION OCCURRED =====");
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out successfully");
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
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
