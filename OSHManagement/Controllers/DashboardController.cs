using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSHManagement.Services.Dashboards;
using System.Security.Claims;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly MyDashboardService _myDashboardService;

        public DashboardController(MyDashboardService myDashboardService)
        {
            _myDashboardService = myDashboardService;
        }

        public IActionResult Index()
        {
            ViewData["PayrollNo"] = User.FindFirst("PayrollNo")?.Value;
            ViewData["FullName"] = User.FindFirst("FullName")?.Value;

            return View();
        }

        /// <summary>
        /// My Dashboard - Personal employee OSH information
        /// Shows actions, teams, incidents and hazards for the logged-in user
        /// </summary>
        [Authorize]
        public async Task<IActionResult> MyDashboard()
        {
            var payrollNo = GetCurrentUserPayrollNo();
            
            if (string.IsNullOrEmpty(payrollNo))
            {
                return RedirectToAction("Index");
            }

            var viewModel = await _myDashboardService.GetDashboardDataAsync(payrollNo);
            return View(viewModel);
        }

        /// <summary>
        /// Helper method to get current user PayrollNo from claims
        /// </summary>
        private string GetCurrentUserPayrollNo()
        {
            return User.FindFirst("PayrollNo")?.Value ?? string.Empty;
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
