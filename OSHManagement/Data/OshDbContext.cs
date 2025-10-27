using Microsoft.EntityFrameworkCore;
using OSHManagement.Models;

namespace OSHManagement.Data
{
    public class OshDbContext : DbContext
    {
        public OshDbContext(DbContextOptions<OshDbContext> options) : base(options)
        {
        }

        // Authentication & Authorization
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Organization Structure
        public DbSet<OrgCategory> OrgCategories { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<OrgMetadata> OrgMetadata { get; set; }

        // OSH Policy
        public DbSet<OshPolicy> OshPolicies { get; set; }

        // Teams
        public DbSet<TeamTypeDefinition> TeamTypeDefinitions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamRoleDefinition> TeamRoleDefinitions { get; set; }
        public DbSet<OshCommitteeConfig> OshCommitteeConfigs { get; set; }
        public DbSet<RiskAssessmentConfig> RiskAssessmentConfigs { get; set; }
        public DbSet<IncidentInvestigationConfig> IncidentInvestigationConfigs { get; set; }
        public DbSet<CommitteeIssue> CommitteeIssues { get; set; }
        public DbSet<CommitteeRecommendation> CommitteeRecommendations { get; set; }
        public DbSet<CommitteeAction> CommitteeActions { get; set; }

        // Hazards & Risks
        public DbSet<Hazard> Hazards { get; set; }
        public DbSet<RiskMitigationPlan> RiskMitigationPlans { get; set; }

        // Incidents
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentCause> IncidentCauses { get; set; }
        public DbSet<IncidentInvestigation> IncidentInvestigations { get; set; }
        public DbSet<ControlAction> ControlActions { get; set; }
        public DbSet<LessonLearned> LessonsLearned { get; set; }

        // Media Management
        public DbSet<MediaCollection> MediaCollections { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }
        public DbSet<MediaAssociation> MediaAssociations { get; set; }
        public DbSet<MediaAccessLog> MediaAccessLogs { get; set; }
        public DbSet<MediaConversionJob> MediaConversionJobs { get; set; }

        // Notification System
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<NotificationChannelConfig> NotificationChannelConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.HasIndex(e => e.PayrollNo).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
            });

            // Permission configuration
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);
            });

            // EmployeeRole configuration
            modelBuilder.Entity<EmployeeRole>(entity =>
            {
                entity.HasKey(e => e.EmployeeRoleId);
                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.EmployeeRoles)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.EmployeeRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RolePermission configuration
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.RolePermissionId);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TeamTypeDefinition configuration
            modelBuilder.Entity<TeamTypeDefinition>(entity =>
            {
                entity.HasKey(e => e.TeamTypeDefinitionId);
                entity.HasIndex(e => e.TypeCode).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure decimal properties with precision and scale
                entity.Property(e => e.RequiredEmployeeRepRatio).HasPrecision(5, 4);
                entity.Property(e => e.RequiredEmployerRepRatio).HasPrecision(5, 4);
                entity.Property(e => e.MinFemaleRatio).HasPrecision(5, 4);
                entity.Property(e => e.MinMaleRatio).HasPrecision(5, 4);
                entity.Property(e => e.QuorumPercentage).HasPrecision(5, 4);
            });

            // Team configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.TeamId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationship to TeamTypeDefinition
                entity.HasOne(t => t.TeamTypeDefinition)
                    .WithMany(ttd => ttd.Teams)
                    .HasForeignKey(t => t.TeamTypeDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ignore deprecated columns that were removed from database
                entity.Ignore(t => t.TeamType);
                entity.Ignore(t => t.RequiredMemberCount);
                entity.Ignore(t => t.MaxMemberCount);
                entity.Ignore(t => t.RequiresSectionRepresentation);
            });

            // TeamRoleDefinition configuration (update existing)
            modelBuilder.Entity<TeamRoleDefinition>(entity =>
            {
                entity.HasKey(e => e.TeamRoleDefinitionId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationship to TeamTypeDefinition
                entity.HasOne(trd => trd.TeamTypeDefinition)
                    .WithMany(ttd => ttd.TeamRoleDefinitions)
                    .HasForeignKey(trd => trd.TeamTypeDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ignore deprecated TeamType column if it was removed from database
                entity.Ignore(trd => trd.TeamType);
            });

            // TeamMember configuration
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasKey(e => e.MemberId);
                
                // Configure Employee relationship using PayrollNo (string FK)
                entity.HasOne(tm => tm.Employee)
                    .WithMany()
                    .HasForeignKey(tm => tm.EmployeePayroll)
                    .HasPrincipalKey(e => e.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Configure TeamRoleDefinition relationship (nullable)
                entity.HasOne(tm => tm.TeamRoleDefinition)
                    .WithMany(trd => trd.TeamMembers)
                    .HasForeignKey(tm => tm.TeamRoleDefinitionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Indexes for performance
                entity.HasIndex(e => new { e.RecipientType, e.RecipientId, e.IsRead })
                    .HasDatabaseName("IX_Notifications_Recipient");
                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_Notifications_Created")
                    .IsDescending();
                entity.HasIndex(e => new { e.Category, e.CreatedAt })
                    .HasDatabaseName("IX_Notifications_Category");
                entity.HasIndex(e => new { e.Priority, e.IsRead, e.CreatedAt })
                    .HasDatabaseName("IX_Notifications_Priority");
            });

            // NotificationTemplate configuration
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(e => e.TemplateId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Unique constraint: One template per event+channel
                entity.HasIndex(e => new { e.TemplateName, e.Channel })
                    .IsUnique()
                    .HasDatabaseName("UQ_NotificationTemplates_Name_Channel");
                
                // Indexes
                entity.HasIndex(e => new { e.Category, e.IsActive })
                    .HasDatabaseName("IX_NotificationTemplates_Category");
                entity.HasIndex(e => new { e.IsActive, e.TemplateName })
                    .HasDatabaseName("IX_NotificationTemplates_Active");
            });

            // NotificationDelivery configuration
            modelBuilder.Entity<NotificationDelivery>(entity =>
            {
                entity.HasKey(e => e.NotificationDeliveryId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key to Notification with cascade delete
                entity.HasOne(nd => nd.Notification)
                    .WithMany(n => n.Deliveries)
                    .HasForeignKey(nd => nd.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes
                entity.HasIndex(e => new { e.Status, e.CreatedAt })
                    .HasDatabaseName("IX_NotificationDeliveries_Status");
                entity.HasIndex(e => e.NotificationId)
                    .HasDatabaseName("IX_NotificationDeliveries_Notification");
                entity.HasIndex(e => new { e.Channel, e.Status })
                    .HasDatabaseName("IX_NotificationDeliveries_Channel");
            });

            // NotificationPreference configuration
            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.HasKey(e => e.PreferenceId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key to Employee with cascade delete
                entity.HasOne(np => np.Employee)
                    .WithMany()
                    .HasForeignKey(np => np.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Unique constraint: One preference per employee+category
                entity.HasIndex(e => new { e.EmployeeId, e.Category })
                    .IsUnique()
                    .HasDatabaseName("UQ_NotificationPreferences_Employee_Category");
                
                // Indexes
                entity.HasIndex(e => e.EmployeeId)
                    .HasDatabaseName("IX_NotificationPreferences_Employee");
                entity.HasIndex(e => e.Category)
                    .HasDatabaseName("IX_NotificationPreferences_Category");
            });

            // NotificationChannelConfig configuration
            modelBuilder.Entity<NotificationChannelConfig>(entity =>
            {
                entity.HasKey(e => e.ConfigId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Unique constraint: One value per channel+key
                entity.HasIndex(e => new { e.Channel, e.ConfigKey })
                    .IsUnique()
                    .HasDatabaseName("UQ_NotificationChannelConfigs_Channel_Key");
                
                // Indexes
                entity.HasIndex(e => new { e.Channel, e.IsActive })
                    .HasDatabaseName("IX_NotificationChannelConfigs_Channel");
            });

            // ========================================
            // Media Management Configuration
            // ========================================

            // MediaCollection configuration
            modelBuilder.Entity<MediaCollection>(entity =>
            {
                entity.HasKey(e => e.CollectionId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Relationships
                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByPayroll)
                    .HasPrincipalKey(emp => emp.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedByPayroll)
                    .HasPrincipalKey(emp => emp.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Unique constraint on collection name
                entity.HasIndex(e => e.CollectionName)
                    .IsUnique()
                    .HasDatabaseName("UQ_MediaCollections_Name");
                
                // Index for module queries
                entity.HasIndex(e => e.ModuleName)
                    .HasDatabaseName("IX_MediaCollections_Module");
            });

            // MediaFile configuration
            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.HasKey(e => e.MediaId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Relationships
                entity.HasOne(e => e.Collection)
                    .WithMany(c => c.MediaFiles)
                    .HasForeignKey(e => e.CollectionId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.UploadedBy)
                    .WithMany()
                    .HasForeignKey(e => e.UploadedByPayroll)
                    .HasPrincipalKey(emp => emp.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedByPayroll)
                    .HasPrincipalKey(emp => emp.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ParentMedia)
                    .WithMany(e => e.ChildVersions)
                    .HasForeignKey(e => e.ParentMediaId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes
                entity.HasIndex(e => e.FileHash)
                    .HasDatabaseName("IX_MediaFiles_FileHash");
                
                entity.HasIndex(e => new { e.CollectionId, e.IsActive, e.CreatedAt })
                    .HasDatabaseName("IX_MediaFiles_Collection");
                
                entity.HasIndex(e => e.UploadedByPayroll)
                    .HasDatabaseName("IX_MediaFiles_UploadedBy");
                
                entity.HasIndex(e => new { e.ParentMediaId, e.VersionNumber })
                    .HasDatabaseName("IX_MediaFiles_Version");
            });

            // MediaAssociation configuration
            modelBuilder.Entity<MediaAssociation>(entity =>
            {
                entity.HasKey(e => e.AssociationId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Relationships
                entity.HasOne(e => e.Media)
                    .WithMany(m => m.Associations)
                    .HasForeignKey(e => e.MediaId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByPayroll)
                    .HasPrincipalKey(emp => emp.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedByPayroll)
                    .HasPrincipalKey(emp => emp.PayrollNo)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Composite indexes for polymorphic lookups
                entity.HasIndex(e => new { e.AssociatedTable, e.AssociatedRecordId, e.AssociationType })
                    .HasDatabaseName("IX_MediaAssociations_Entity");
                
                entity.HasIndex(e => new { e.MediaId, e.AssociationType })
                    .HasDatabaseName("IX_MediaAssociations_Media");
            });

            // MediaAccessLog configuration
            modelBuilder.Entity<MediaAccessLog>(entity =>
            {
                entity.HasKey(e => e.AccessLogId);
                entity.Property(e => e.AccessTimestamp).HasDefaultValueSql("GETUTCDATE()");
                
                // Indexes for audit queries
                entity.HasIndex(e => new { e.MediaId, e.AccessTimestamp })
                    .HasDatabaseName("IX_MediaAccessLogs_Media")
                    .IsDescending(false, true);
                
                entity.HasIndex(e => new { e.UserPayroll, e.AccessTimestamp })
                    .HasDatabaseName("IX_MediaAccessLogs_User")
                    .IsDescending(false, true);
            });

            // MediaConversionJob configuration
            modelBuilder.Entity<MediaConversionJob>(entity =>
            {
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Index for job status queries
                entity.HasIndex(e => new { e.JobStatus, e.CreatedAt })
                    .HasDatabaseName("IX_MediaConversionJobs_Status");
            });
        }
    }
}
