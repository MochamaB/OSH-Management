using Microsoft.AspNetCore.Mvc;

namespace OSHManagement.Controllers
{
    public class TestController : Controller
    {
        public IActionResult LayoutTest()
        {
            ViewData["Title"] = "Layout Test";
            ViewData["Breadcrumb"] = @"
                <li class=""breadcrumb-item""><a href=""/"">Dashboard</a></li>
                <li class=""breadcrumb-item""><a href=""/test"">Testing</a></li>
                <li class=""breadcrumb-item active"">Layout Test</li>
            ";
            
            return View();
        }
    }
}
