namespace OSHManagement.Data.Seeds
{
    /// <summary>
    /// Main database seeder - coordinates all seed operations
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeds all required data for the application
        /// Call this from Program.cs after database migrations
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger for tracking seed operations</param>
        public static void SeedAll(OshDbContext context, ILogger? logger = null)
        {
            logger?.LogInformation("Starting database seeding...");

            try
            {
                // Seed TeamRoleDefinitions
                logger?.LogInformation("Seeding TeamRoleDefinitions...");
                TeamRoleDefinitionSeeder.SeedTeamRoleDefinitions(context);
                logger?.LogInformation("TeamRoleDefinitions seeded successfully.");

                // Add other seeders here as needed
                // Example: RoleSeeder.SeedRoles(context);
                // Example: PermissionSeeder.SeedPermissions(context);

                logger?.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }
    }
}
