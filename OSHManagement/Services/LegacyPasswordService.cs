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
        private readonly OshDbContext _context;
        private readonly ILogger<LegacyPasswordService> _logger;

        public LegacyPasswordService(OshDbContext context, ILogger<LegacyPasswordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> VerifyLegacyPasswordAsync(string payrollNo, string password)
        {
            try
            {
                var sql = @"
                    SELECT CAST(PWDCOMPARE(@password, LegacyPassword) AS BIT) AS IsValid
                    FROM Employees
                    WHERE PayrollNo = @payrollNo AND LegacyPassword IS NOT NULL";

                var parameters = new[]
                {
                    new SqlParameter("@password", password),
                    new SqlParameter("@payrollNo", payrollNo)
                };

                var result = await _context.Database
                    .SqlQueryRaw<bool>(sql, parameters)
                    .FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying legacy password for {payrollNo}");
                return false;
            }
        }
    }
}
