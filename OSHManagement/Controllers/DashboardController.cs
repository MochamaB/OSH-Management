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

        /// <summary>
        /// Test page for Progress Widget components
        /// Shows standard, icon, and threshold-based variants
        /// </summary>
        [AllowAnonymous] // Allow access for testing
        public IActionResult TestProgressWidgets()
        {
            return View();
        }

        /// <summary>
        /// Test page for List Widget components
        /// Shows standard, compact, timeline, and notification variants
        /// </summary>
        [AllowAnonymous] // Allow access for testing
        public IActionResult TestListWidgets()
        {
            return View();
        }

        /// <summary>
        /// Test page for Table Widget components
        /// Shows standard tables with various column types and pre-built tables
        /// </summary>
        [AllowAnonymous] // Allow access for testing
        public IActionResult TestTableWidgets()
        {
            return View();
        }

        /// <summary>
        /// Test page for Donut Chart components
        /// Shows ApexCharts donut charts with various data configurations
        /// </summary>
        [AllowAnonymous] // Allow access for testing
        public IActionResult TestDonutCharts()
        {
            return View();
        }

        /// <summary>
        /// Test page for Bar and Line Chart components
        /// Shows ApexCharts bar and line charts with various data configurations
        /// </summary>
        [AllowAnonymous] // Allow access for testing
        public IActionResult TestCharts()
        {
            return View();
        }
    }
}
