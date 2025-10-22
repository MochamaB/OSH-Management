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
        }
    }
}
