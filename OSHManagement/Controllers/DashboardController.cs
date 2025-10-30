using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewData["PayrollNo"] = User.FindFirst("PayrollNo")?.Value;
            ViewData["FullName"] = User.FindFirst("FullName")?.Value;

            return View();
        }

        /// <summary>
        /// Test page for KPI Card components
        /// Shows all three patterns and usage examples
        /// </summary>
        [AllowAnonymous] // Allow access for testing
        public IActionResult TestKPICards()
        {
            return View();
        }
    }
}
