using Hangfire.Dashboard;

namespace OSHManagement.Services
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Check if user is authenticated
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                // Check session for authenticated user
                var payrollNo = httpContext.Session.GetString("PayrollNo");
                if (string.IsNullOrEmpty(payrollNo))
                {
                    return false;
                }
            }

            // TODO: Add role-based authorization
            // For now, allow any authenticated user
            // Later, check for Admin role:
            // return httpContext.User.IsInRole("Admin");

            return true;
        }
    }
}
