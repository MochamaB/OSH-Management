using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Services;
using OSHManagement.Services.Notifications;
using OSHManagement.Services.Notifications.Channels;
using OSHManagement.Services.Security;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure Entity Framework
builder.Services.AddDbContext<OshDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

// Configure Hangfire (Optional - can be disabled via appsettings)
var enableHangfire = builder.Configuration.GetValue<bool>("HangfireSettings:Enabled", false);
if (enableHangfire)
{
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromMinutes(1), // Changed from Zero to 1 minute
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            PrepareSchemaIfNecessary = false // Prevent schema reinstallation on every startup
        }));

    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = 1; // Limit to 1 worker to reduce load
        options.ServerTimeout = TimeSpan.FromMinutes(5);
        options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
    });
}

// Register services
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<LegacyDataMigrationService>();
builder.Services.AddScoped<HangfireJobs>();

// Register authentication services
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<ILegacyPasswordService, LegacyPasswordService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Register scope services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserScopeService, UserScopeService>();
builder.Services.AddScoped<IScopeFilterService, ScopeFilterService>();

// Register common query services
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IOrganizationalHierarchyService, OrganizationalHierarchyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Register security services
builder.Services.AddDataProtection(); // ASP.NET Core Data Protection API
builder.Services.AddSingleton<IEncryptionService, DataProtectionEncryptionService>();

// Register notification services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationEventPublisher, NotificationEventPublisher>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
builder.Services.AddScoped<IChannelConfigService, ChannelConfigService>();
builder.Services.AddScoped<INotificationChannelService, InAppNotificationService>();
builder.Services.AddScoped<INotificationChannelService, EmailNotificationService>();

// Register specialized notification services (domain-specific)
builder.Services.AddScoped<IEmployeeNotificationService, EmployeeNotificationService>();
builder.Services.AddScoped<ITeamNotificationService, TeamNotificationService>();

// Register media management services
builder.Services.AddScoped<OSHManagement.Services.Media.IStorageProvider, OSHManagement.Services.Media.LocalStorageProvider>();
builder.Services.AddScoped<OSHManagement.Services.Media.IMediaService, OSHManagement.Services.Media.MediaService>();

// Add memory cache for reference data (Categories, Roles)
builder.Services.AddMemoryCache();

var app = builder.Build();

// Ensure uploads directory exists
var webRootPath = app.Environment.WebRootPath;
var uploadsPath = Path.Combine(webRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    app.Logger.LogInformation("Created uploads directory: {Path}", uploadsPath);
}

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OshDbContext>();
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
        var seeder = new DatabaseSeeder(context, logger);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire Dashboard (only if enabled)
if (enableHangfire)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() },
        DashboardTitle = builder.Configuration["HangfireSettings:DashboardTitle"] ?? "OSH Management Jobs"
    });

    // Schedule recurring jobs
    try
    {
        RecurringJob.AddOrUpdate<HangfireJobs>(
            "daily-legacy-sync",
            job => job.DailySyncJob(),
            builder.Configuration["HangfireSettings:DailySync:CronExpression"] ?? "0 2 * * *", // 2 AM daily
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Failed to schedule Hangfire jobs. Hangfire will be disabled.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
