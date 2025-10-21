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
        }
    }
}
