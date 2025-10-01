using Microsoft.Data.SqlClient;
using System.Data;

namespace OSHManagement.Services
{
    public class LegacyDataMigrationService
    {
        private readonly string _connectionString;
        private readonly string _legacyConnectionString;
        private readonly ILogger<LegacyDataMigrationService> _logger;

        public LegacyDataMigrationService(
            IConfiguration configuration,
            ILogger<LegacyDataMigrationService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection is not configured");

            var adoConnectionString = configuration.GetConnectionString("KTDALeaveContext")
                ?? throw new ArgumentNullException("KTDALeaveContext is not configured");

            // Convert ADO.NET connection string to OPENROWSET format
            _legacyConnectionString = ConvertToOpenRowsetFormat(adoConnectionString);
            _logger = logger;
        }

        private string ConvertToOpenRowsetFormat(string adoConnectionString)
        {
            // Parse ADO.NET connection string
            var builder = new SqlConnectionStringBuilder(adoConnectionString);

            // Build OPENROWSET format: Server=.;Database=KTDALeave;UID=sa;PWD=password
            return $"Server={builder.DataSource};Database={builder.InitialCatalog};UID={builder.UserID};PWD={builder.Password}";
        }

        public async Task<(bool Success, string Message)> SyncAllDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting full legacy data synchronization");

                var stationsResult = await SyncStationsAsync();
                if (!stationsResult.Success)
                {
                    return stationsResult;
                }

                var departmentsResult = await SyncDepartmentsAsync();
                if (!departmentsResult.Success)
                {
                    return departmentsResult;
                }

                var employeesResult = await SyncEmployeesAsync();
                if (!employeesResult.Success)
                {
                    return employeesResult;
                }

                var rolesResult = await SyncRolesAsync();
                if (!rolesResult.Success)
                {
                    return rolesResult;
                }

                _logger.LogInformation("Full legacy data synchronization completed successfully");
                return (true, "All data synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during full data synchronization");
                return (false, $"Synchronization failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SyncStationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting stations synchronization");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_SyncStationsFromLegacy", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 300
                };

                command.Parameters.AddWithValue("@LegacyConnectionString", _legacyConnectionString);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Stations synchronization completed");
                return (true, "Stations synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing stations");
                return (false, $"Stations sync failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SyncDepartmentsAsync()
        {
            try
            {
                _logger.LogInformation("Starting departments synchronization");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_SyncDepartmentsFromLegacy", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 300
                };

                command.Parameters.AddWithValue("@LegacyConnectionString", _legacyConnectionString);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Departments synchronization completed");
                return (true, "Departments synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing departments");
                return (false, $"Departments sync failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SyncEmployeesAsync()
        {
            try
            {
                _logger.LogInformation("Starting employees synchronization");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_SyncEmployeesFromLegacy", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 300
                };

                command.Parameters.AddWithValue("@LegacyConnectionString", _legacyConnectionString);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Employees synchronization completed");
                return (true, "Employees synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing employees");
                return (false, $"Employees sync failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SyncRolesAsync()
        {
            try
            {
                _logger.LogInformation("Starting roles synchronization");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_SyncRolesFromLegacy", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 300
                };

                command.Parameters.AddWithValue("@LegacyConnectionString", _legacyConnectionString);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Roles synchronization completed");
                return (true, "Roles synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing roles");
                return (false, $"Roles sync failed: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, int>> GetSyncStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get counts
                stats["Stations"] = await GetCountAsync(connection, "SELECT COUNT(*) FROM Stations");
                stats["Departments"] = await GetCountAsync(connection, "SELECT COUNT(*) FROM Departments");
                stats["Employees"] = await GetCountAsync(connection, "SELECT COUNT(*) FROM Employees");
                stats["Roles"] = await GetCountAsync(connection, "SELECT COUNT(*) FROM Roles");
                stats["EmployeeRoles"] = await GetCountAsync(connection, "SELECT COUNT(*) FROM EmployeeRoles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync statistics");
            }

            return stats;
        }

        private async Task<int> GetCountAsync(SqlConnection connection, string query)
        {
            using var command = new SqlCommand(query, connection);
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
