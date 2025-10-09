using OSHManagement.Models.Authorization;

namespace OSHManagement.Services
{
    /// <summary>
    /// Service for determining and managing user data access scope
    /// </summary>
    public interface IUserScopeService
    {
        /// <summary>
        /// Gets the scope for the currently authenticated user
        /// Returns null if no user is authenticated
        /// </summary>
        UserScope? GetCurrentUserScope();

        /// <summary>
        /// Gets the scope for a specific user by ID
        /// </summary>
        Task<UserScope?> GetUserScopeAsync(int userId);

        /// <summary>
        /// Determines the scope level based on employee's roles and position
        /// </summary>
        Task<ScopeLevel> DetermineScopeLevelAsync(int userId);
    }
}
