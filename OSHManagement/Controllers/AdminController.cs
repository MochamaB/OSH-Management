using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSHManagement.Services;
using Hangfire;

namespace OSHManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LegacyDataMigrationService _migrationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            LegacyDataMigrationService migrationService,
            ILogger<AdminController> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        public async Task<IActionResult> DataSynchronization()
        {
            // Get current statistics
            var stats = await _migrationService.GetSyncStatisticsAsync();
            ViewBag.Stats = stats;

            return View();
        }

        [HttpPost]
        public IActionResult TriggerFullSync()
        {
            try
            {
                // Enqueue background job
                var jobId = BackgroundJob.Enqueue<HangfireJobs>(job => job.ManualSyncJob());

                TempData["Success"] = $"Full synchronization job queued successfully. Job ID: {jobId}";
                _logger.LogInformation($"Manual full sync triggered. Job ID: {jobId}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to queue synchronization job: {ex.Message}";
                _logger.LogError(ex, "Error triggering manual sync");
            }

            return RedirectToAction("DataSynchronization");
        }

        [HttpPost]
        public IActionResult TriggerEntitySync(string entityType)
        {
            try
            {
                // Enqueue background job for specific entity
                var jobId = BackgroundJob.Enqueue<HangfireJobs>(job => job.SyncEntityJob(entityType));

                TempData["Success"] = $"{entityType} synchronization job queued successfully. Job ID: {jobId}";
                _logger.LogInformation($"Manual {entityType} sync triggered. Job ID: {jobId}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to queue synchronization job: {ex.Message}";
                _logger.LogError(ex, $"Error triggering {entityType} sync");
            }

            return RedirectToAction("DataSynchronization");
        }

        [HttpGet]
        public async Task<IActionResult> GetSyncStatistics()
        {
            try
            {
                var stats = await _migrationService.GetSyncStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync statistics");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
