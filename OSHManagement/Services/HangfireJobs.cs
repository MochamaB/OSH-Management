using Hangfire;

namespace OSHManagement.Services
{
    public class HangfireJobs
    {
        private readonly LegacyDataMigrationService _migrationService;
        private readonly ILogger<HangfireJobs> _logger;

        public HangfireJobs(
            LegacyDataMigrationService migrationService,
            ILogger<HangfireJobs> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        /// <summary>
        /// Daily sync job - runs at 2 AM every day
        /// </summary>
        [AutomaticRetry(Attempts = 3)]
        public async Task DailySyncJob()
        {
            _logger.LogInformation("Starting daily legacy data sync job");

            try
            {
                var result = await _migrationService.SyncAllDataAsync();

                if (result.Success)
                {
                    _logger.LogInformation($"Daily sync completed successfully: {result.Message}");
                }
                else
                {
                    _logger.LogError($"Daily sync failed: {result.Message}");
                    throw new Exception($"Sync failed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during daily sync job");
                throw;
            }
        }

        /// <summary>
        /// Manual sync job - can be triggered from admin panel
        /// </summary>
        [AutomaticRetry(Attempts = 2)]
        public async Task ManualSyncJob()
        {
            _logger.LogInformation("Starting manual legacy data sync job");

            try
            {
                var result = await _migrationService.SyncAllDataAsync();

                if (result.Success)
                {
                    _logger.LogInformation($"Manual sync completed successfully: {result.Message}");
                }
                else
                {
                    _logger.LogError($"Manual sync failed: {result.Message}");
                    throw new Exception($"Sync failed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual sync job");
                throw;
            }
        }

        /// <summary>
        /// Sync individual entity type - for targeted syncs
        /// </summary>
        [AutomaticRetry(Attempts = 2)]
        public async Task SyncEntityJob(string entityType)
        {
            _logger.LogInformation($"Starting sync for entity type: {entityType}");

            try
            {
                (bool Success, string Message) result = entityType.ToLower() switch
                {
                    "stations" => await _migrationService.SyncStationsAsync(),
                    "departments" => await _migrationService.SyncDepartmentsAsync(),
                    "employees" => await _migrationService.SyncEmployeesAsync(),
                    _ => (false, $"Unknown entity type: {entityType}")
                };

                if (result.Success)
                {
                    _logger.LogInformation($"Sync for {entityType} completed: {result.Message}");
                }
                else
                {
                    _logger.LogError($"Sync for {entityType} failed: {result.Message}");
                    throw new Exception($"Sync failed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during {entityType} sync job");
                throw;
            }
        }
    }
}
