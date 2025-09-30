using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSHManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaCollections",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollectionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CollectionDisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxFileSizeMb = table.Column<int>(type: "int", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetentionPolicyDays = table.Column<int>(type: "int", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    RequiresAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    AllowedRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaCollections", x => x.CollectionId);
                });

            migrationBuilder.CreateTable(
                name: "OrgCategories",
                columns: table => new
                {
                    OrgCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgCategories", x => x.OrgCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    OriginalFilename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SystemFilename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StorageProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    ParentMediaId = table.Column<int>(type: "int", nullable: true),
                    IsLatestVersion = table.Column<bool>(type: "bit", nullable: false),
                    UploadStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadedByPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_MediaFiles_MediaCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "MediaCollections",
                        principalColumn: "CollectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaFiles_MediaFiles_ParentMediaId",
                        column: x => x.ParentMediaId,
                        principalTable: "MediaFiles",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    StationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrgCategoryId = table.Column<int>(type: "int", nullable: false),
                    ParentStationId = table.Column<int>(type: "int", nullable: true),
                    LegacyStationMapping = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.StationId);
                    table.ForeignKey(
                        name: "FK_Stations_OrgCategories_OrgCategoryId",
                        column: x => x.OrgCategoryId,
                        principalTable: "OrgCategories",
                        principalColumn: "OrgCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stations_Stations_ParentStationId",
                        column: x => x.ParentStationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediaAccessLogs",
                columns: table => new
                {
                    AccessLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccessTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResponseSizeBytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAccessLogs", x => x.AccessLogId);
                    table.ForeignKey(
                        name: "FK_MediaAccessLogs_MediaFiles_MediaId",
                        column: x => x.MediaId,
                        principalTable: "MediaFiles",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssociations",
                columns: table => new
                {
                    AssociationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    AssociatedTable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssociatedRecordId = table.Column<int>(type: "int", nullable: false),
                    AssociationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssociationLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MaxFilesAllowed = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssociations", x => x.AssociationId);
                    table.ForeignKey(
                        name: "FK_MediaAssociations_MediaFiles_MediaId",
                        column: x => x.MediaId,
                        principalTable: "MediaFiles",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaConversionJobs",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JobParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutputMediaId = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaConversionJobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_MediaConversionJobs_MediaFiles_MediaId",
                        column: x => x.MediaId,
                        principalTable: "MediaFiles",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaConversionJobs_MediaFiles_OutputMediaId",
                        column: x => x.OutputMediaId,
                        principalTable: "MediaFiles",
                        principalColumn: "MediaId");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                    DepartmentHeadPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LegacyDepartmentMapping = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrgMetadata",
                columns: table => new
                {
                    MetadataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    FactoryManagerPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TotalEmployeesMale = table.Column<int>(type: "int", nullable: true),
                    TotalEmployeesFemale = table.Column<int>(type: "int", nullable: true),
                    OutsourcedEmployeesMale = table.Column<int>(type: "int", nullable: true),
                    OutsourcedEmployeesFemale = table.Column<int>(type: "int", nullable: true),
                    OshPolicyExists = table.Column<bool>(type: "bit", nullable: false),
                    ComplianceStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgMetadata", x => x.MetadataId);
                    table.ForeignKey(
                        name: "FK_OrgMetadata_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OshPolicies",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    HasTopManagementSignature = table.Column<bool>(type: "bit", nullable: false),
                    DateSigned = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignedByPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPolicyImplemented = table.Column<bool>(type: "bit", nullable: false),
                    CommunicationMethods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementResponsibilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisorResponsibilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutsourcedResponsibilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractorResponsibilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractorCharterExists = table.Column<bool>(type: "bit", nullable: false),
                    PolicyStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OshPolicies", x => x.PolicyId);
                    table.ForeignKey(
                        name: "FK_OshPolicies_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    SectionSupervisorPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_Sections_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    TeamType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeamDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiredMemberCount = table.Column<int>(type: "int", nullable: true),
                    MaxMemberCount = table.Column<int>(type: "int", nullable: true),
                    RequiresSectionRepresentation = table.Column<bool>(type: "bit", nullable: false),
                    TeamStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FormationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DisbandDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RollNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LegacyPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmployeeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_Employees_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hazards",
                columns: table => new
                {
                    HazardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    HazardCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HazardDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImpactDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CircumstancesDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExistingPrecautions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeverityLevel = table.Column<int>(type: "int", nullable: false),
                    LikelihoodLevel = table.Column<int>(type: "int", nullable: false),
                    RiskRating = table.Column<int>(type: "int", nullable: false),
                    PriorityLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IdentifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hazards", x => x.HazardId);
                    table.ForeignKey(
                        name: "FK_Hazards_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hazards_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hazards_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentInvestigationConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    InvestigationScope = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TeamExpertise = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResponseTimeHours = table.Column<int>(type: "int", nullable: false),
                    EscalationThreshold = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentInvestigationConfigs", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_IncidentInvestigationConfigs_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    IncidentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    InvestigationTeamId = table.Column<int>(type: "int", nullable: true),
                    IncidentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IncidentSeverity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncidentTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    LocationDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    PersonAffectedPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PersonAffectedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PersonAffectedGender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IncidentDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InjuryDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedOnSite = table.Column<bool>(type: "bit", nullable: false),
                    ReportedToDoshmis = table.Column<bool>(type: "bit", nullable: false),
                    ReportedToMajaniInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IncidentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReportedByPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.IncidentId);
                    table.ForeignKey(
                        name: "FK_Incidents_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidents_Teams_InvestigationTeamId",
                        column: x => x.InvestigationTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId");
                });

            migrationBuilder.CreateTable(
                name: "OshCommitteeConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    IsCommitteeTrained = table.Column<bool>(type: "bit", nullable: false),
                    TrainingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasMeetingSchedule = table.Column<bool>(type: "bit", nullable: false),
                    InspectionFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastInspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextInspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OshCommitteeConfigs", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_OshCommitteeConfigs_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessmentConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    AssessmentFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastAssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextAssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeamQualifications = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessmentConfigs", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_RiskAssessmentConfigs_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    EmployeePayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MemberRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelevantExperience = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVotingMember = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskMitigationPlans",
                columns: table => new
                {
                    MitigationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HazardId = table.Column<int>(type: "int", nullable: false),
                    MitigationMeasures = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImplementationPeriod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsiblePersonPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MonitoringSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImplementationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskMitigationPlans", x => x.MitigationId);
                    table.ForeignKey(
                        name: "FK_RiskMitigationPlans_Hazards_HazardId",
                        column: x => x.HazardId,
                        principalTable: "Hazards",
                        principalColumn: "HazardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlActions",
                columns: table => new
                {
                    ActionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedToPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedDepartmentId = table.Column<int>(type: "int", nullable: true),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletionVerificationBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EffectivenessRating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlActions", x => x.ActionId);
                    table.ForeignKey(
                        name: "FK_ControlActions_Departments_AssignedDepartmentId",
                        column: x => x.AssignedDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_ControlActions_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCauses",
                columns: table => new
                {
                    CauseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    NotUsingPpes = table.Column<bool>(type: "bit", nullable: false),
                    IgnoringSops = table.Column<bool>(type: "bit", nullable: false),
                    TakingShortcuts = table.Column<bool>(type: "bit", nullable: false),
                    WorkingWithoutAuthorization = table.Column<bool>(type: "bit", nullable: false),
                    PoorLighting = table.Column<bool>(type: "bit", nullable: false),
                    SlipperyFloor = table.Column<bool>(type: "bit", nullable: false),
                    DamagedEquipment = table.Column<bool>(type: "bit", nullable: false),
                    UnguardedMachine = table.Column<bool>(type: "bit", nullable: false),
                    ExposedElectricalSystem = table.Column<bool>(type: "bit", nullable: false),
                    HumanFactors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationalFactors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnvironmentalFactors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCauses", x => x.CauseId);
                    table.ForeignKey(
                        name: "FK_IncidentCauses_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentInvestigations",
                columns: table => new
                {
                    InvestigationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    InvestigationTeamId = table.Column<int>(type: "int", nullable: false),
                    InvestigationStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvestigationCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvestigationMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EvidenceCollected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WitnessesInterviewed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImmediateCauses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RootCauses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestigationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvestigationLeadPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentInvestigations", x => x.InvestigationId);
                    table.ForeignKey(
                        name: "FK_IncidentInvestigations_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentInvestigations_Teams_InvestigationTeamId",
                        column: x => x.InvestigationTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonsLearned",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    LessonTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LessonDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LessonCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApplicableToStations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicableToSections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonPriority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SharedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImplementationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SharedByPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonsLearned", x => x.LessonId);
                    table.ForeignKey(
                        name: "FK_LessonsLearned_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeIssues",
                columns: table => new
                {
                    IssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    IssueTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IssueDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RaisedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RaisedByMemberId = table.Column<int>(type: "int", nullable: false),
                    PreviousOutcome = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IssueStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResolutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeIssues", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_CommitteeIssues_TeamMembers_RaisedByMemberId",
                        column: x => x.RaisedByMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommitteeIssues_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeRecommendations",
                columns: table => new
                {
                    RecommendationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    RecommendationDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecommendedByMemberId = table.Column<int>(type: "int", nullable: false),
                    ImplementationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeRecommendations", x => x.RecommendationId);
                    table.ForeignKey(
                        name: "FK_CommitteeRecommendations_CommitteeIssues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "CommitteeIssues",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeRecommendations_TeamMembers_RecommendedByMemberId",
                        column: x => x.RecommendedByMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommitteeRecommendations_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeActions",
                columns: table => new
                {
                    ActionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    RecommendationId = table.Column<int>(type: "int", nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToPayroll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeActions", x => x.ActionId);
                    table.ForeignKey(
                        name: "FK_CommitteeActions_CommitteeRecommendations_RecommendationId",
                        column: x => x.RecommendationId,
                        principalTable: "CommitteeRecommendations",
                        principalColumn: "RecommendationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeActions_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeActions_RecommendationId",
                table: "CommitteeActions",
                column: "RecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeActions_TeamId",
                table: "CommitteeActions",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeIssues_RaisedByMemberId",
                table: "CommitteeIssues",
                column: "RaisedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeIssues_TeamId",
                table: "CommitteeIssues",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeRecommendations_IssueId",
                table: "CommitteeRecommendations",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeRecommendations_RecommendedByMemberId",
                table: "CommitteeRecommendations",
                column: "RecommendedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeRecommendations_TeamId",
                table: "CommitteeRecommendations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlActions_AssignedDepartmentId",
                table: "ControlActions",
                column: "AssignedDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlActions_IncidentId",
                table: "ControlActions",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_StationId",
                table: "Departments",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PayrollNo",
                table: "Employees",
                column: "PayrollNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_StationId",
                table: "Employees",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_SectionId",
                table: "Hazards",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_StationId",
                table: "Hazards",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_TeamId",
                table: "Hazards",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCauses_IncidentId",
                table: "IncidentCauses",
                column: "IncidentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvestigationConfigs_TeamId",
                table: "IncidentInvestigationConfigs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvestigations_IncidentId",
                table: "IncidentInvestigations",
                column: "IncidentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvestigations_InvestigationTeamId",
                table: "IncidentInvestigations",
                column: "InvestigationTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_InvestigationTeamId",
                table: "Incidents",
                column: "InvestigationTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_SectionId",
                table: "Incidents",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_StationId",
                table: "Incidents",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonsLearned_IncidentId",
                table: "LessonsLearned",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAccessLogs_MediaId",
                table: "MediaAccessLogs",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssociations_MediaId",
                table: "MediaAssociations",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCollections_CollectionName",
                table: "MediaCollections",
                column: "CollectionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaConversionJobs_MediaId",
                table: "MediaConversionJobs",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaConversionJobs_OutputMediaId",
                table: "MediaConversionJobs",
                column: "OutputMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_CollectionId",
                table: "MediaFiles",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_ParentMediaId",
                table: "MediaFiles",
                column: "ParentMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgMetadata_StationId",
                table: "OrgMetadata",
                column: "StationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OshCommitteeConfigs_TeamId",
                table: "OshCommitteeConfigs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_OshPolicies_StationId",
                table: "OshPolicies",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessmentConfigs_TeamId",
                table: "RiskAssessmentConfigs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskMitigationPlans_HazardId",
                table: "RiskMitigationPlans",
                column: "HazardId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_StationId",
                table: "Sections",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_OrgCategoryId",
                table: "Stations",
                column: "OrgCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_ParentStationId",
                table: "Stations",
                column: "ParentStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StationCode",
                table: "Stations",
                column: "StationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_SectionId",
                table: "TeamMembers",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId",
                table: "TeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_StationId",
                table: "Teams",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeActions");

            migrationBuilder.DropTable(
                name: "ControlActions");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "IncidentCauses");

            migrationBuilder.DropTable(
                name: "IncidentInvestigationConfigs");

            migrationBuilder.DropTable(
                name: "IncidentInvestigations");

            migrationBuilder.DropTable(
                name: "LessonsLearned");

            migrationBuilder.DropTable(
                name: "MediaAccessLogs");

            migrationBuilder.DropTable(
                name: "MediaAssociations");

            migrationBuilder.DropTable(
                name: "MediaConversionJobs");

            migrationBuilder.DropTable(
                name: "OrgMetadata");

            migrationBuilder.DropTable(
                name: "OshCommitteeConfigs");

            migrationBuilder.DropTable(
                name: "OshPolicies");

            migrationBuilder.DropTable(
                name: "RiskAssessmentConfigs");

            migrationBuilder.DropTable(
                name: "RiskMitigationPlans");

            migrationBuilder.DropTable(
                name: "CommitteeRecommendations");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropTable(
                name: "Hazards");

            migrationBuilder.DropTable(
                name: "CommitteeIssues");

            migrationBuilder.DropTable(
                name: "MediaCollections");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "OrgCategories");
        }
    }
}
