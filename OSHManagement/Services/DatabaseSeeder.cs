using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using System.Security.Cryptography;
using System.Text;

namespace OSHManagement.Services
{
    public class DatabaseSeeder
    {
        private readonly OshDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(OshDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedOrgCategoriesAsync();
                await SeedDefaultStationAsync();
                await SeedDefaultAdminAsync();

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding");
                throw;
            }
        }

        private async Task SeedOrgCategoriesAsync()
        {
            var categoriesToSeed = new List<OrgCategory>
            {
                new OrgCategory { CategoryName = "Factory", Description = "Tea processing factories", IsActive = true },
                new OrgCategory { CategoryName = "Head Office", Description = "Corporate headquarters and central offices", IsActive = true },
                new OrgCategory { CategoryName = "Regional Office", Description = "Regional administrative offices", IsActive = true }
            };

            foreach (var category in categoriesToSeed)
            {
                var existingCategory = await _context.OrgCategories
                    .FirstOrDefaultAsync(c => c.CategoryName == category.CategoryName);

                if (existingCategory == null)
                {
                    category.CreatedAt = DateTime.UtcNow;
                    await _context.OrgCategories.AddAsync(category);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Organization categories seeding completed");
            Console.WriteLine("Organization categories seeding completed");
        }
        private async Task SeedDefaultStationAsync()
        {
            if (!await _context.Stations.AnyAsync())
            {
                var defaultCategory = await _context.OrgCategories.FirstOrDefaultAsync();
                if (defaultCategory != null)
                {
                    var defaultStation = new Station
                    {
                        StationCode = "HQ",
                        StationName = "Head Office",
                        OrgCategoryId = defaultCategory.OrgCategoryId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Stations.AddAsync(defaultStation);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Seeded default station (Head Office)");
                    Console.WriteLine("Seeded default station (Head Office)");
                }
            }
        }

        private async Task SeedDefaultAdminAsync()
        {
            // Check if admin already exists
            var existingAdmin = await _context.Employees
                .FirstOrDefaultAsync(e => e.PayrollNo == "ADMIN001");

            if (existingAdmin == null)
            {
                var defaultStation = await _context.Stations.FirstOrDefaultAsync();
                if (defaultStation == null)
                {
                    _logger.LogWarning("Cannot seed admin user - no station available");
                    return;
                }

                var admin = new Employee
                {
                    PayrollNo = "ADMIN001",
                    RollNo = "001",
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailAddress = "admin@ktda.co.ke",
                    Username = "admin",
                    PasswordHash = HashPassword("Admin@123"),
                    StationId = defaultStation.StationId,
                    EmploymentStatus = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Employees.AddAsync(admin);
                await _context.SaveChangesAsync();

                // Create Admin role if it doesn't exist
                var adminRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == "Admin");

                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        RoleName = "Admin",
                        Description = "System Administrator - Full access to all features",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Roles.AddAsync(adminRole);
                    await _context.SaveChangesAsync();
                }

                // Assign admin role to admin user
                var employeeRole = new EmployeeRole
                {
                    EmployeeId = admin.EmployeeId,
                    RoleId = adminRole.RoleId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.EmployeeRoles.AddAsync(employeeRole);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Seeded default admin user (PayrollNo: ADMIN001, Password: Admin@123)");
                Console.WriteLine("Seeded default admin user (PayrollNo: ADMIN001, Password: Admin@123)");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
