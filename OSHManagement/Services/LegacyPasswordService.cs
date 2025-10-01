using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;

namespace OSHManagement.Services
{
    public interface ILegacyPasswordService
    {
        Task<bool> VerifyLegacyPasswordAsync(string payrollNo, string password);
    }

    public class LegacyPasswordService : ILegacyPasswordService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LegacyPasswordService> _logger;

        public LegacyPasswordService(IConfiguration configuration, ILogger<LegacyPasswordService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> VerifyLegacyPasswordAsync(string payrollNo, string password)
        {
            try
            {
                var legacyConnectionString = _configuration.GetConnectionString("KTDALeaveContext");

                using (var connection = new SqlConnection(legacyConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("Pro_password", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@username", payrollNo);
                        command.Parameters.AddWithValue("@pass_word", password);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var passer = reader["passer"]?.ToString();
                                // If passer equals the password, authentication successful
                                // If passer equals "Fail", authentication failed
                                return passer == password;
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying legacy password for {payrollNo}");
                return false;
            }
        }
    }
}
