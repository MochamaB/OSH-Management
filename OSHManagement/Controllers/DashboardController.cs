using Microsoft.AspNetCore.Mvc;

namespace OSHManagement.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Check if user is authenticated
          //  var payrollNo = HttpContext.Session.GetString("PayrollNo");
          //  if (string.IsNullOrEmpty(payrollNo))
         //   {
         //       return RedirectToAction("Login", "Account");
        //    }

         //   ViewData["PayrollNo"] = payrollNo;
         //   ViewData["FullName"] = HttpContext.Session.GetString("FullName");

            return View();
        }
    }
}
