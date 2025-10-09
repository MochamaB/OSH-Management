using OSHManagement.Models.Authorization;

namespace OSHManagement.Services
{
    /// <summary>
    /// Service for applying scope-based filtering to queries
    /// Ensures users only see data they have access to based on their scope level
    /// </summary>
    public interface IScopeFilterService
    {
        /// <summary>
        /// Applies scope filtering to a queryable based on current user's scope
        /// </summary>
        /// <typeparam name="T">The entity type to filter</typeparam>
        /// <param name="query">The base query to filter</param>
        /// <param name="userScope">Optional specific user scope (if null, uses current user)</param>
        /// <returns>Filtered query based on scope</returns>
        IQueryable<T> ApplyScope<T>(IQueryable<T> query, UserScope? userScope = null) where T : class;

        /// <summary>
        /// Gets the current user's scope from claims
        /// </summary>
        UserScope? GetCurrentUserScope();
    }
}
