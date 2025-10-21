# C# Integration Code - OSH to SAP HCM

## Table of Contents
1. [Configuration Setup](#configuration-setup)
2. [Service Interface](#service-interface)
3. [OData Implementation](#odata-implementation)
4. [RFC Implementation](#rfc-implementation)
5. [Hangfire Job](#hangfire-job)
6. [Data Mapping](#data-mapping)
7. [Error Handling](#error-handling)

---

## Configuration Setup

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OshManagement;Trusted_Connection=True;"
  },
  "SapIntegration": {
    "Enabled": true,
    "Mode": "OData",
    "OData": {
      "BaseUrl": "https://sap.company.com:8000",
      "ServicePath": "/sap/opu/odata/sap/Z_OSH_EMPLOYEE_SRV",
      "Username": "OSH_INTEGRATION",
      "Password": "encrypted_password_here",
      "Timeout": 300
    },
    "Rfc": {
      "Host": "sap.company.com",
      "SystemNumber": "00",
      "Client": "400",
      "Username": "OSH_INTEGRATION",
      "Password": "encrypted_password_here",
      "Language": "EN"
    },
    "Sync": {
      "Schedule": "0 */6 * * *",
      "BatchSize": 1000,
      "RetryCount": 3,
      "RetryDelaySeconds": 60
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "OSHManagement.Services.SapIntegration": "Debug"
    }
  }
}
```

### appsettings.Development.json

```json
{
  "SapIntegration": {
    "OData": {
      "BaseUrl": "https://sapdev.company.com:8000",
      "Username": "DEV_USER",
      "Password": "dev_password"
    },
    "Sync": {
      "Schedule": "0 */1 * * *"
    }
  }
}
```

### appsettings.Production.json

```json
{
  "SapIntegration": {
    "OData": {
      "BaseUrl": "https://sapprod.company.com:8000",
      "Username": "OSH_PROD_USER",
      "Password": "use_azure_key_vault"
    },
    "Sync": {
      "Schedule": "0 2 * * *"
    }
  }
}
```

---

## Service Interface

### Models/DTOs/SapEmployee.cs

```csharp
namespace OSHManagement.Models.DTOs.Sap
{
    public class SapEmployeeDto
    {
        public string PersonnelNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PlantCode { get; set; } = string.Empty;
        public string PersonnelSubarea { get; set; } = string.Empty;
        public string OrgUnit { get; set; } = string.Empty;
        public string CostCenter { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastSyncDate { get; set; }
    }

    public class SapOrgMappingDto
    {
        public int MappingId { get; set; }
        public string MappingType { get; set; } = string.Empty; // STATION or DEPT
        public string SapCode { get; set; } = string.Empty;
        public int OshId { get; set; }
        public string OshName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
```

### Services/ISapHcmIntegrationService.cs

```csharp
namespace OSHManagement.Services
{
    public interface ISapHcmIntegrationService
    {
        /// <summary>
        /// Get all active employees from SAP
        /// </summary>
        Task<List<SapEmployeeDto>> GetEmployeesAsync();

        /// <summary>
        /// Get employees filtered by plant
        /// </summary>
        Task<List<SapEmployeeDto>> GetEmployeesByPlantAsync(string plantCode);

        /// <summary>
        /// Get employees changed since specified date
        /// </summary>
        Task<List<SapEmployeeDto>> GetChangedEmployeesAsync(DateTime since);

        /// <summary>
        /// Get organization mapping (SAP codes to OSH IDs)
        /// </summary>
        Task<List<SapOrgMappingDto>> GetOrgMappingsAsync();

        /// <summary>
        /// Test connection to SAP
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Sync employees from SAP to OSH database
        /// </summary>
        Task<SyncResult> SyncEmployeesAsync();
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int InsertedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public TimeSpan Duration { get; set; }
        public DateTime SyncTime { get; set; }
    }
}
```

---

## OData Implementation

### Services/SapODataService.cs

```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OSHManagement.Services
{
    public class SapODataService : ISapHcmIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SapODataService> _logger;
        private readonly OshDbContext _context;

        public SapODataService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<SapODataService> logger,
            OshDbContext context)
        {
            _httpClient = httpClientFactory.CreateClient("SapOData");
            _configuration = configuration;
            _logger = logger;
            _context = context;

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            var baseUrl = _configuration["SapIntegration:OData:BaseUrl"];
            var servicePath = _configuration["SapIntegration:OData:ServicePath"];
            var username = _configuration["SapIntegration:OData:Username"];
            var password = _configuration["SapIntegration:OData:Password"];

            _httpClient.BaseAddress = new Uri($"{baseUrl}{servicePath}");

            // Basic Authentication
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", authValue);

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // SAP-specific headers
            _httpClient.DefaultRequestHeaders.Add("sap-client", "400");
        }

        public async Task<List<SapEmployeeDto>> GetEmployeesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching employees from SAP OData");

                var response = await _httpClient.GetAsync(
                    "/EmployeeSet?$format=json&$filter=Status eq '3'");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<SapEmployeeDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var employees = oDataResponse?.D?.Results ?? new List<SapEmployeeDto>();

                _logger.LogInformation($"Retrieved {employees.Count} employees from SAP");

                return employees;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching employees from SAP");
                throw new SapIntegrationException("Failed to connect to SAP", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees from SAP");
                throw;
            }
        }

        public async Task<List<SapEmployeeDto>> GetEmployeesByPlantAsync(string plantCode)
        {
            try
            {
                _logger.LogInformation($"Fetching employees for plant {plantCode}");

                var filter = $"Status eq '3' and PlantCode eq '{plantCode}'";
                var response = await _httpClient.GetAsync(
                    $"/EmployeeSet?$format=json&$filter={Uri.EscapeDataString(filter)}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<SapEmployeeDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return oDataResponse?.D?.Results ?? new List<SapEmployeeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching employees for plant {plantCode}");
                throw;
            }
        }

        public async Task<List<SapEmployeeDto>> GetChangedEmployeesAsync(DateTime since)
        {
            try
            {
                var sinceStr = since.ToString("yyyy-MM-dd");
                var filter = $"Status eq '3' and LastSyncDate ge datetime'{sinceStr}T00:00:00'";
                
                var response = await _httpClient.GetAsync(
                    $"/EmployeeSet?$format=json&$filter={Uri.EscapeDataString(filter)}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<SapEmployeeDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return oDataResponse?.D?.Results ?? new List<SapEmployeeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching changed employees");
                throw;
            }
        }

        public async Task<List<SapOrgMappingDto>> GetOrgMappingsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching organization mappings from SAP");

                var response = await _httpClient.GetAsync(
                    "/OrgMappingSet?$format=json&$filter=IsActive eq true");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<SapOrgMappingDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return oDataResponse?.D?.Results ?? new List<SapOrgMappingDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching organization mappings");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing SAP connection");

                var response = await _httpClient.GetAsync("/$metadata");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SAP connection test failed");
                return false;
            }
        }

        public async Task<SyncResult> SyncEmployeesAsync()
        {
            var result = new SyncResult
            {
                SyncTime = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting employee sync from SAP");

                // 1. Get employees from SAP
                var sapEmployees = await GetEmployeesAsync();
                result.TotalRecords = sapEmployees.Count;

                // 2. Get organization mappings
                var mappings = await GetOrgMappingsAsync();
                var stationMappings = mappings
                    .Where(m => m.MappingType == "STATION")
                    .ToDictionary(m => m.SapCode, m => m.OshId);
                var deptMappings = mappings
                    .Where(m => m.MappingType == "DEPT")
                    .ToDictionary(m => m.SapCode, m => m.OshId);

                // 3. Process each employee
                foreach (var sapEmp in sapEmployees)
                {
                    try
                    {
                        await ProcessEmployeeAsync(sapEmp, stationMappings, deptMappings, result);
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Error processing {sapEmp.PersonnelNumber}: {ex.Message}");
                        _logger.LogError(ex, $"Error processing employee {sapEmp.PersonnelNumber}");
                    }
                }

                // 4. Save changes
                await _context.SaveChangesAsync();

                result.Success = result.ErrorCount == 0;
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;

                _logger.LogInformation(
                    $"Sync completed: {result.InsertedRecords} inserted, " +
                    $"{result.UpdatedRecords} updated, {result.ErrorCount} errors in {result.Duration.TotalSeconds:F2}s");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee sync failed");
                result.Success = false;
                result.Errors.Add(ex.Message);
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }
        }

        private async Task ProcessEmployeeAsync(
            SapEmployeeDto sapEmp,
            Dictionary<string, int> stationMappings,
            Dictionary<string, int> deptMappings,
            SyncResult result)
        {
            // Map SAP codes to OSH IDs
            if (!stationMappings.TryGetValue(sapEmp.PlantCode, out int stationId))
            {
                throw new InvalidOperationException(
                    $"No station mapping for plant code: {sapEmp.PlantCode}");
            }

            deptMappings.TryGetValue(sapEmp.OrgUnit, out int departmentId);

            // Find existing employee
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.PayrollNo == sapEmp.PersonnelNumber);

            if (employee == null)
            {
                // Create new employee
                employee = new Employee
                {
                    PayrollNo = sapEmp.PersonnelNumber,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Employees.Add(employee);
                result.InsertedRecords++;
            }
            else
            {
                result.UpdatedRecords++;
            }

            // Update fields
            employee.FirstName = sapEmp.FirstName;
            employee.LastName = sapEmp.LastName;
            employee.Designation = sapEmp.Position;
            employee.StationId = stationId;
            employee.DepartmentId = departmentId > 0 ? departmentId : null;
            employee.EmailAddress = sapEmp.Email;
            employee.EmploymentStatus = sapEmp.Status == "3" ? "Active" : "Inactive";
            employee.UpdatedAt = DateTime.UtcNow;
        }

        // Helper classes for OData response deserialization
        private class ODataResponse<T>
        {
            public ODataData<T> D { get; set; }
        }

        private class ODataData<T>
        {
            public List<T> Results { get; set; }
        }
    }

    public class SapIntegrationException : Exception
    {
        public SapIntegrationException(string message) : base(message) { }
        public SapIntegrationException(string message, Exception inner) : base(message, inner) { }
    }
}
```

---

## RFC Implementation

### Services/SapRfcService.cs

**Note:** Requires SAP .NET Connector (NCo) library

```csharp
using SAP.Middleware.Connector;

namespace OSHManagement.Services
{
    public class SapRfcService : ISapHcmIntegrationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SapRfcService> _logger;
        private readonly OshDbContext _context;
        private RfcDestination _destination;

        public SapRfcService(
            IConfiguration configuration,
            ILogger<SapRfcService> logger,
            OshDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;

            InitializeDestination();
        }

        private void InitializeDestination()
        {
            var destConfig = new RfcConfigParameters
            {
                { RfcConfigParameters.Name, "OSH_DEST" },
                { RfcConfigParameters.AppServerHost, _configuration["SapIntegration:Rfc:Host"] },
                { RfcConfigParameters.SystemNumber, _configuration["SapIntegration:Rfc:SystemNumber"] },
                { RfcConfigParameters.User, _configuration["SapIntegration:Rfc:Username"] },
                { RfcConfigParameters.Password, _configuration["SapIntegration:Rfc:Password"] },
                { RfcConfigParameters.Client, _configuration["SapIntegration:Rfc:Client"] },
                { RfcConfigParameters.Language, _configuration["SapIntegration:Rfc:Language"] }
            };

            _destination = RfcDestinationManager.GetDestination(destConfig);
        }

        public async Task<List<SapEmployeeDto>> GetEmployeesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Calling SAP RFC Z_OSH_GET_EMPLOYEES");

                    var function = _destination.Repository.CreateFunction("Z_OSH_GET_EMPLOYEES");

                    // Execute RFC
                    function.Invoke(_destination);

                    // Get results
                    var employeeTable = function.GetTable("ET_EMPLOYEES");
                    var count = function.GetInt("EV_COUNT");
                    var message = function.GetString("EV_MESSAGE");

                    _logger.LogInformation($"RFC returned {count} employees: {message}");

                    // Convert to DTO list
                    var employees = new List<SapEmployeeDto>();
                    for (int i = 0; i < employeeTable.RowCount; i++)
                    {
                        employeeTable.CurrentIndex = i;
                        employees.Add(new SapEmployeeDto
                        {
                            PersonnelNumber = employeeTable.GetString("PERNR"),
                            FirstName = employeeTable.GetString("VORNA"),
                            LastName = employeeTable.GetString("NACHN"),
                            Position = employeeTable.GetString("STELL"),
                            PlantCode = employeeTable.GetString("WERKS"),
                            PersonnelSubarea = employeeTable.GetString("BTRTL"),
                            OrgUnit = employeeTable.GetString("ORGEH"),
                            CostCenter = employeeTable.GetString("KOSTL"),
                            Email = employeeTable.GetString("EMAIL"),
                            Status = employeeTable.GetString("STAT2")
                        });
                    }

                    return employees;
                }
                catch (RfcAbapException ex)
                {
                    _logger.LogError(ex, $"SAP RFC ABAP exception: {ex.Key}");
                    throw new SapIntegrationException($"SAP error: {ex.Message}", ex);
                }
                catch (RfcException ex)
                {
                    _logger.LogError(ex, "SAP RFC communication error");
                    throw new SapIntegrationException("SAP connection failed", ex);
                }
            });
        }

        public async Task<List<SapEmployeeDto>> GetEmployeesByPlantAsync(string plantCode)
        {
            return await Task.Run(() =>
            {
                var function = _destination.Repository.CreateFunction("Z_OSH_GET_EMPLOYEES");
                function.SetValue("IV_PLANT", plantCode);
                function.Invoke(_destination);

                var employeeTable = function.GetTable("ET_EMPLOYEES");
                var employees = new List<SapEmployeeDto>();

                for (int i = 0; i < employeeTable.RowCount; i++)
                {
                    employeeTable.CurrentIndex = i;
                    employees.Add(new SapEmployeeDto
                    {
                        PersonnelNumber = employeeTable.GetString("PERNR"),
                        FirstName = employeeTable.GetString("VORNA"),
                        LastName = employeeTable.GetString("NACHN"),
                        PlantCode = employeeTable.GetString("WERKS"),
                        OrgUnit = employeeTable.GetString("ORGEH")
                        // ... map other fields
                    });
                }

                return employees;
            });
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    _destination.Ping();
                    return true;
                });
            }
            catch
            {
                return false;
            }
        }

        // Implement other interface methods similar to OData version
        // SyncEmployeesAsync follows same pattern as OData version
    }
}
```

---

## Hangfire Job

### Services/HangfireJobs.cs (Add SAP Sync)

```csharp
namespace OSHManagement.Services
{
    public class HangfireJobs
    {
        private readonly ISapHcmIntegrationService _sapService;
        private readonly ILogger<HangfireJobs> _logger;
        private readonly IConfiguration _configuration;

        public HangfireJobs(
            ISapHcmIntegrationService sapService,
            ILogger<HangfireJobs> logger,
            IConfiguration configuration)
        {
            _sapService = sapService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SapEmployeeSyncJob()
        {
            try
            {
                if (!_configuration.GetValue<bool>("SapIntegration:Enabled"))
                {
                    _logger.LogInformation("SAP integration is disabled");
                    return;
                }

                _logger.LogInformation("Starting SAP employee sync job");

                var result = await _sapService.SyncEmployeesAsync();

                if (result.Success)
                {
                    _logger.LogInformation(
                        $"SAP sync completed successfully: " +
                        $"{result.InsertedRecords} inserted, " +
                        $"{result.UpdatedRecords} updated in {result.Duration.TotalSeconds:F2}s");
                }
                else
                {
                    _logger.LogError(
                        $"SAP sync completed with errors: {result.ErrorCount} errors");
                    
                    // Send notification to admins
                    await SendSyncErrorNotificationAsync(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SAP employee sync job failed");
                throw;
            }
        }

        private async Task SendSyncErrorNotificationAsync(SyncResult result)
        {
            // Implement email notification to admins
            // Use your existing email service
        }
    }
}
```

### Program.cs (Registration)

```csharp
// Register SAP Integration Service
var sapMode = builder.Configuration["SapIntegration:Mode"];
if (sapMode == "OData")
{
    builder.Services.AddHttpClient("SapOData")
        .ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(
                builder.Configuration.GetValue<int>("SapIntegration:OData:Timeout"));
        });
    
    builder.Services.AddScoped<ISapHcmIntegrationService, SapODataService>();
}
else if (sapMode == "RFC")
{
    builder.Services.AddScoped<ISapHcmIntegrationService, SapRfcService>();
}

// Schedule SAP sync job
if (enableHangfire && builder.Configuration.GetValue<bool>("SapIntegration:Enabled"))
{
    RecurringJob.AddOrUpdate<HangfireJobs>(
        "sap-employee-sync",
        job => job.SapEmployeeSyncJob(),
        builder.Configuration["SapIntegration:Sync:Schedule"] ?? "0 */6 * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Local
        });
}
```

---

## Data Mapping

### Services/SapEmployeeMapper.cs

```csharp
namespace OSHManagement.Services
{
    public static class SapEmployeeMapper
    {
        public static Employee MapToEmployee(
            SapEmployeeDto sapEmployee,
            Dictionary<string, int> stationMappings,
            Dictionary<string, int> deptMappings)
        {
            if (!stationMappings.TryGetValue(sapEmployee.PlantCode, out int stationId))
            {
                throw new InvalidOperationException(
                    $"No station mapping found for plant code: {sapEmployee.PlantCode}");
            }

            deptMappings.TryGetValue(sapEmployee.OrgUnit, out int departmentId);

            return new Employee
            {
                PayrollNo = sapEmployee.PersonnelNumber,
                FirstName = sapEmployee.FirstName,
                LastName = sapEmployee.LastName,
                Designation = sapEmployee.Position,
                StationId = stationId,
                DepartmentId = departmentId > 0 ? departmentId : null,
                EmailAddress = sapEmployee.Email,
                EmploymentStatus = MapEmploymentStatus(sapEmployee.Status),
                EmployeeType = "Regular", // Default or map from SAP
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static string MapEmploymentStatus(string sapStatus)
        {
            return sapStatus switch
            {
                "3" => "Active",
                "0" => "Inactive",
                _ => "Unknown"
            };
        }
    }
}
```

---

## Error Handling

### Middleware/SapErrorHandlingMiddleware.cs

```csharp
namespace OSHManagement.Middleware
{
    public class SapErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SapErrorHandlingMiddleware> _logger;

        public SapErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<SapErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SapIntegrationException ex)
            {
                _logger.LogError(ex, "SAP integration error");
                await HandleSapExceptionAsync(context, ex);
            }
        }

        private static Task HandleSapExceptionAsync(HttpContext context, SapIntegrationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "SAP Integration Error",
                message = "Unable to connect to SAP system. Please try again later.",
                details = ex.Message
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

---

## Testing

### Test Controller (for manual testing)

```csharp
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class SapTestController : ControllerBase
{
    private readonly ISapHcmIntegrationService _sapService;

    public SapTestController(ISapHcmIntegrationService sapService)
    {
        _sapService = sapService;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        var result = await _sapService.TestConnectionAsync();
        return Ok(new { connected = result });
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _sapService.GetEmployeesAsync();
        return Ok(new { count = employees.Count, employees });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync()
    {
        var result = await _sapService.SyncEmployeesAsync();
        return Ok(result);
    }
}
```

---

## Summary

This implementation provides:
- Configuration-driven SAP connection
- Support for both OData and RFC
- Automated Hangfire sync jobs
- Error handling and logging
- Data mapping between SAP and OSH
- Test endpoints for manual verification

**Next**: See **09_Troubleshooting_Guide.md** for common issues and solutions!
