using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class SectionController : Controller
    {
        private readonly OshDbContext _context;

        public SectionController(OshDbContext context)
        {
            _context = context;
        }

        // GET: Section/Index
        public async Task<IActionResult> Index(string? search, string? status, int? stationId, int page = 1)
        {
            const int pageSize = 15; // Items per page

            // Start with base query
            var query = _context.Sections
                .Include(s => s.Station)
                .Include(s => s.TeamMembers)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s =>
                    s.SectionName.ToLower().Contains(search) ||
                    s.Station.StationName.ToLower().Contains(search)
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(s => s.IsActive);
                }
                else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(s => !s.IsActive);
                }
            }

            // Apply station filter
            if (stationId.HasValue && stationId.Value > 0)
            {
                query = query.Where(s => s.StationId == stationId.Value);
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Calculate statistics
            var allSections = await query
                .Select(s => new { s.IsActive, TeamMemberCount = s.TeamMembers.Count })
                .ToListAsync();

            ViewBag.TotalSections = totalItems;
            ViewBag.ActiveSections = allSections.Count(s => s.IsActive);
            ViewBag.InactiveSections = allSections.Count(s => !s.IsActive);
            ViewBag.TotalTeamMembers = allSections.Sum(s => s.TeamMemberCount);

            // Execute query with pagination
            var sections = await query
                .OrderBy(s => s.Station.StationName).ThenBy(s => s.SectionName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SectionViewModel
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName,
                    StationId = s.StationId,
                    StationName = s.Station.StationName,
                    SectionSupervisorPayroll = s.SectionSupervisorPayroll,
                    IsActive = s.IsActive,
                    TeamMemberCount = s.TeamMembers.Count,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            // Get supervisor names
            var supervisorPayrolls = sections
                .Where(s => !string.IsNullOrEmpty(s.SectionSupervisorPayroll))
                .Select(s => s.SectionSupervisorPayroll)
                .Distinct()
                .ToList();

            var supervisorNames = await _context.Employees
                .Where(e => supervisorPayrolls.Contains(e.PayrollNo))
                .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
                .ToDictionaryAsync(e => e.PayrollNo, e => e.FullName);

            // Populate supervisor names
            foreach (var section in sections)
            {
                if (!string.IsNullOrEmpty(section.SectionSupervisorPayroll) && 
                    supervisorNames.ContainsKey(section.SectionSupervisorPayroll))
                {
                    section.SupervisorFullName = supervisorNames[section.SectionSupervisorPayroll];
                }
            }

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Get stations for filter dropdown
            var stations = await _context.Stations
                .Where(st => st.IsActive)
                .OrderBy(st => st.StationName)
                .Select(st => new { st.StationId, st.StationName })
                .ToListAsync();

            ViewBag.Stations = stations;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStationId = stationId;

            return View(sections);
        }
    }
}
