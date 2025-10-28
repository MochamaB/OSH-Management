using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;
using OSHManagement.Services.Media;
using OSHManagement.Extensions;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class OshPolicyController : ScopedController
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationalHierarchyService _orgHierarchyService;
        private readonly IScopeFilterService _scopeFilterService;
        private readonly IMediaService _mediaService;

        public OshPolicyController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            IOrganizationService organizationService,
            IOrganizationalHierarchyService orgHierarchyService,
            IMediaService mediaService,
            ILogger<OshPolicyController> logger)
            : base(context, scopeFilter, logger)
        {
            _organizationService = organizationService;
            _orgHierarchyService = orgHierarchyService;
            _scopeFilterService = scopeFilter;
            _mediaService = mediaService;
        }

        // GET: OshPolicy/Index
        public async Task<IActionResult> Index(string? search, int? categoryId, int? stationId, string? status, int page = 1)
        {
            const int pageSize = 15;

            // Start with base query
            var query = _context.OshPolicies.AsQueryable();

            // ⚠️ CRITICAL: Apply scope FIRST (security takes precedence)
            query = _scopeFilterService.ApplyScope(query, CurrentScope);

            // THEN apply includes
            query = query
                .Include(p => p.Station)
                    .ThenInclude(s => s.OrgCategory);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Station.StationName.ToLower().Contains(search) ||
                    p.Station.StationCode.ToLower().Contains(search) ||
                    (p.ManagementResponsibilities != null && p.ManagementResponsibilities.ToLower().Contains(search)) ||
                    (p.SupervisorResponsibilities != null && p.SupervisorResponsibilities.ToLower().Contains(search))
                );
            }

            // Apply category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.Station.OrgCategoryId == categoryId.Value);
            }

            // Apply station filter
            if (stationId.HasValue && stationId.Value > 0)
            {
                query = query.Where(p => p.StationId == stationId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.PolicyStatus == status);
            }

            // Get total count
            var totalItems = await query.CountAsync();

            // Calculate statistics
            var allPolicies = await query
                .Select(p => new { p.PolicyStatus, p.HasTopManagementSignature, p.IsPolicyImplemented, p.LastReviewedDate })
                .ToListAsync();

            ViewBag.TotalPolicies = totalItems;
            ViewBag.ActivePolicies = allPolicies.Count(p => p.PolicyStatus == "Active");
            ViewBag.CompliantPolicies = allPolicies.Count(p =>
                p.PolicyStatus == "Active" &&
                p.HasTopManagementSignature &&
                p.IsPolicyImplemented &&
                p.LastReviewedDate.HasValue);
            ViewBag.NeedsAttention = allPolicies.Count(p =>
                p.PolicyStatus != "Active" ||
                !p.HasTopManagementSignature ||
                !p.IsPolicyImplemented);

            // Execute query with pagination
            var policies = await query
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new OshPolicyViewModel
                {
                    PolicyId = p.PolicyId,
                    StationId = p.StationId,
                    StationName = p.Station != null ? p.Station.StationName : "",
                    StationCode = p.Station != null ? p.Station.StationCode : "",
                    OrgCategoryName = p.Station != null && p.Station.OrgCategory != null ? p.Station.OrgCategory.CategoryName : null,
                    HasTopManagementSignature = p.HasTopManagementSignature,
                    DateSigned = p.DateSigned,
                    SignedByPayroll = p.SignedByPayroll,
                    LastReviewedDate = p.LastReviewedDate,
                    IsPolicyImplemented = p.IsPolicyImplemented,
                    ContractorCharterExists = p.ContractorCharterExists,
                    PolicyStatus = p.PolicyStatus,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            // Get signer names for policies
            var signerPayrolls = policies
                .Where(p => !string.IsNullOrEmpty(p.SignedByPayroll))
                .Select(p => p.SignedByPayroll!)
                .Distinct()
                .ToList();

            Dictionary<string, string> signerNames = new();
            if (signerPayrolls.Any())
            {
                signerNames = await _context.Employees
                    .Where(e => signerPayrolls.Contains(e.PayrollNo))
                    .ToDictionaryAsync(e => e.PayrollNo, e => $"{e.FirstName} {e.LastName}");
            }

            // Populate signer names
            foreach (var policy in policies)
            {
                if (!string.IsNullOrEmpty(policy.SignedByPayroll) && signerNames.ContainsKey(policy.SignedByPayroll))
                {
                    policy.SignedByName = signerNames[policy.SignedByPayroll];
                }
            }

            // Get document counts for policies
            var policyIds = policies.Select(p => p.PolicyId).ToList();
            var policyIdStrings = policyIds.Select(id => id.ToString()).ToList();
            var documentCounts = await _context.MediaAssociations
                .Where(ma => ma.AssociatedTable == "OshPolicies" && policyIdStrings.Contains(ma.AssociatedRecordId) && ma.IsActive)
                .GroupBy(ma => new { ma.AssociatedRecordId, ma.AssociationType })
                .Select(g => new
                {
                    PolicyIdString = g.Key.AssociatedRecordId,
                    AssociationType = g.Key.AssociationType,
                    Count = g.Count()
                })
                .ToListAsync();

            // Populate document counts
            foreach (var policy in policies)
            {
                var policyIdString = policy.PolicyId.ToString();
                policy.PolicyDocumentCount = documentCounts
                    .Where(dc => dc.PolicyIdString == policyIdString && dc.AssociationType == "policy_document")
                    .Sum(dc => dc.Count);
                policy.CharterDocumentCount = documentCounts
                    .Where(dc => dc.PolicyIdString == policyIdString && dc.AssociationType == "charter_document")
                    .Sum(dc => dc.Count);
            }

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Get filter dropdown data
            ViewBag.Categories = await _organizationService.GetActiveCategoriesAsync();
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);

            // Pass filter values to view
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentStationId = stationId;
            ViewBag.CurrentStatus = status;

            return View(policies);
        }

        // GET: OshPolicy/Create
        public async Task<IActionResult> Create(int? stationId)
        {
            await PopulateDropdowns();

            // Pre-populate station if provided
            var model = new OshPolicyViewModel
            {
                PolicyStatus = "Draft"
            };

            if (stationId.HasValue)
            {
                // Verify user has access to this station
                var station = await _context.Stations
                    .Where(s => s.StationId == stationId.Value)
                    .FirstOrDefaultAsync();

                if (station != null)
                {
                    var stationQuery = _context.Stations.Where(s => s.StationId == stationId.Value);
                    stationQuery = _scopeFilterService.ApplyScope(stationQuery, CurrentScope);

                    if (await stationQuery.AnyAsync())
                    {
                        model.StationId = stationId.Value;
                    }
                }
            }

            return View(model);
        }

        // POST: OshPolicy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OshPolicyViewModel model, IFormFile? PolicyDocumentFile, IFormFile? CharterDocumentFile)
        {
            // Additional validation
            if (model.HasTopManagementSignature)
            {
                if (!model.DateSigned.HasValue)
                {
                    ModelState.AddModelError(nameof(model.DateSigned), "Date signed is required when policy is signed by management");
                }
                if (string.IsNullOrWhiteSpace(model.SignedByPayroll))
                {
                    ModelState.AddModelError(nameof(model.SignedByPayroll), "Signatory payroll is required when policy is signed by management");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if policy already exists for this station
                    var existingPolicy = await _context.OshPolicies
                        .FirstOrDefaultAsync(p => p.StationId == model.StationId);

                    if (existingPolicy != null)
                    {
                        TempData["Error"] = "A policy already exists for this station. Please edit the existing policy.";
                        await PopulateDropdowns();
                        return View(model);
                    }

                    // Verify user has access to this station
                    var stationQuery = _context.Stations.Where(s => s.StationId == model.StationId);
                    stationQuery = _scopeFilterService.ApplyScope(stationQuery, CurrentScope);
                    if (!await stationQuery.AnyAsync())
                    {
                        TempData["Error"] = "Access denied to the selected station.";
                        await PopulateDropdowns();
                        return View(model);
                    }

                    // Create new policy
                    var policy = new OshPolicy
                    {
                        StationId = model.StationId,
                        HasTopManagementSignature = model.HasTopManagementSignature,
                        DateSigned = model.DateSigned,
                        SignedByPayroll = model.SignedByPayroll,
                        LastReviewedDate = model.LastReviewedDate,
                        IsPolicyImplemented = model.IsPolicyImplemented,
                        CommunicationMethods = model.CommunicationMethods,
                        ManagementResponsibilities = model.ManagementResponsibilities,
                        SupervisorResponsibilities = model.SupervisorResponsibilities,
                        OutsourcedResponsibilities = model.OutsourcedResponsibilities,
                        ContractorResponsibilities = model.ContractorResponsibilities,
                        ContractorCharterExists = model.ContractorCharterExists,
                        PolicyStatus = model.PolicyStatus,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.OshPolicies.Add(policy);
                    await _context.SaveChangesAsync();

                    // Upload and associate media files with the policy
                    await UploadAndAssociateFilesAsync(policy.PolicyId, model.StationId, PolicyDocumentFile, CharterDocumentFile);

                    // Update OrgMetadata
                    await UpdateOrgMetadataAsync(policy.StationId);

                    var station = await _context.Stations.FindAsync(policy.StationId);
                    TempData["Success"] = $"Policy for '{station?.StationName}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating OSH policy for station {StationId}", model.StationId);
                    TempData["Error"] = "An error occurred while creating the policy. Please try again.";
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // GET: OshPolicy/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _context.OshPolicies
                .Include(p => p.Station)
                    .ThenInclude(s => s.OrgCategory)
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy == null)
            {
                TempData["Error"] = "Policy not found.";
                return RedirectToAction(nameof(Index));
            }

            // Verify user has access to this station
            var stationQuery = _context.Stations.Where(s => s.StationId == policy.StationId);
            stationQuery = _scopeFilterService.ApplyScope(stationQuery, CurrentScope);
            if (!await stationQuery.AnyAsync())
            {
                TempData["Error"] = "Access denied to this policy.";
                return RedirectToAction(nameof(Index));
            }

            var model = new OshPolicyViewModel
            {
                PolicyId = policy.PolicyId,
                StationId = policy.StationId,
                StationName = policy.Station?.StationName ?? "",
                StationCode = policy.Station?.StationCode ?? "",
                OrgCategoryName = policy.Station?.OrgCategory?.CategoryName,
                HasTopManagementSignature = policy.HasTopManagementSignature,
                DateSigned = policy.DateSigned,
                SignedByPayroll = policy.SignedByPayroll,
                LastReviewedDate = policy.LastReviewedDate,
                IsPolicyImplemented = policy.IsPolicyImplemented,
                CommunicationMethods = policy.CommunicationMethods,
                ManagementResponsibilities = policy.ManagementResponsibilities,
                SupervisorResponsibilities = policy.SupervisorResponsibilities,
                OutsourcedResponsibilities = policy.OutsourcedResponsibilities,
                ContractorResponsibilities = policy.ContractorResponsibilities,
                ContractorCharterExists = policy.ContractorCharterExists,
                PolicyStatus = policy.PolicyStatus,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };

            // Get signer name
            if (!string.IsNullOrEmpty(policy.SignedByPayroll))
            {
                var signer = await _context.Employees
                    .Where(e => e.PayrollNo == policy.SignedByPayroll)
                    .FirstOrDefaultAsync();

                if (signer != null)
                {
                    model.SignedByName = $"{signer.FirstName} {signer.LastName}";
                }
            }

            await PopulateDropdowns();

            // Filter employees to only show those from the policy's station
            var stationEmployeesQuery = _context.Employees
                .Where(e => e.EmploymentStatus == "Active" && e.StationId == policy.StationId)
                .AsQueryable();

            // Apply scope filtering for security
            stationEmployeesQuery = _scopeFilterService.ApplyScope(stationEmployeesQuery, CurrentScope);

            var stationEmployees = await stationEmployeesQuery
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
                .ToListAsync();

            ViewBag.Employees = stationEmployees;

            return View(model);
        }

        // POST: OshPolicy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OshPolicyViewModel model)
        {
            if (id != model.PolicyId)
            {
                TempData["Error"] = "Invalid policy ID.";
                return RedirectToAction(nameof(Index));
            }

            // Additional validation
            if (model.HasTopManagementSignature)
            {
                if (!model.DateSigned.HasValue)
                {
                    ModelState.AddModelError(nameof(model.DateSigned), "Date signed is required when policy is signed by management");
                }
                if (string.IsNullOrWhiteSpace(model.SignedByPayroll))
                {
                    ModelState.AddModelError(nameof(model.SignedByPayroll), "Signatory payroll is required when policy is signed by management");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var policy = await _context.OshPolicies.FindAsync(id);

                    if (policy == null)
                    {
                        TempData["Error"] = "Policy not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Verify user has access to this station
                    var stationQuery = _context.Stations.Where(s => s.StationId == policy.StationId);
                    stationQuery = _scopeFilterService.ApplyScope(stationQuery, CurrentScope);
                    if (!await stationQuery.AnyAsync())
                    {
                        TempData["Error"] = "Access denied to this policy.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update policy
                    policy.HasTopManagementSignature = model.HasTopManagementSignature;
                    policy.DateSigned = model.DateSigned;
                    policy.SignedByPayroll = model.SignedByPayroll;
                    policy.LastReviewedDate = model.LastReviewedDate;
                    policy.IsPolicyImplemented = model.IsPolicyImplemented;
                    policy.CommunicationMethods = model.CommunicationMethods;
                    policy.ManagementResponsibilities = model.ManagementResponsibilities;
                    policy.SupervisorResponsibilities = model.SupervisorResponsibilities;
                    policy.OutsourcedResponsibilities = model.OutsourcedResponsibilities;
                    policy.ContractorResponsibilities = model.ContractorResponsibilities;
                    policy.ContractorCharterExists = model.ContractorCharterExists;
                    policy.PolicyStatus = model.PolicyStatus;
                    policy.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Update OrgMetadata
                    await UpdateOrgMetadataAsync(policy.StationId);

                    var station = await _context.Stations.FindAsync(policy.StationId);
                    TempData["Success"] = $"Policy for '{station?.StationName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PolicyExists(model.PolicyId))
                    {
                        TempData["Error"] = "Policy not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating OSH policy {PolicyId}", id);
                    TempData["Error"] = "An error occurred while updating the policy. Please try again.";
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // GET: OshPolicy/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var policy = await _context.OshPolicies
                .Include(p => p.Station)
                    .ThenInclude(s => s.OrgCategory)
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy == null)
            {
                TempData["Error"] = "Policy not found.";
                return RedirectToAction(nameof(Index));
            }

            // Verify user has access to this station
            var stationQuery = _context.Stations.Where(s => s.StationId == policy.StationId);
            stationQuery = _scopeFilterService.ApplyScope(stationQuery, CurrentScope);
            if (!await stationQuery.AnyAsync())
            {
                TempData["Error"] = "Access denied to this policy.";
                return RedirectToAction(nameof(Index));
            }

            var model = new OshPolicyViewModel
            {
                PolicyId = policy.PolicyId,
                StationId = policy.StationId,
                StationName = policy.Station?.StationName ?? "",
                StationCode = policy.Station?.StationCode ?? "",
                OrgCategoryName = policy.Station?.OrgCategory?.CategoryName,
                HasTopManagementSignature = policy.HasTopManagementSignature,
                DateSigned = policy.DateSigned,
                SignedByPayroll = policy.SignedByPayroll,
                LastReviewedDate = policy.LastReviewedDate,
                IsPolicyImplemented = policy.IsPolicyImplemented,
                CommunicationMethods = policy.CommunicationMethods,
                ManagementResponsibilities = policy.ManagementResponsibilities,
                SupervisorResponsibilities = policy.SupervisorResponsibilities,
                OutsourcedResponsibilities = policy.OutsourcedResponsibilities,
                ContractorResponsibilities = policy.ContractorResponsibilities,
                ContractorCharterExists = policy.ContractorCharterExists,
                PolicyStatus = policy.PolicyStatus,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };

            // Get signer name
            if (!string.IsNullOrEmpty(policy.SignedByPayroll))
            {
                var signer = await _context.Employees
                    .Where(e => e.PayrollNo == policy.SignedByPayroll)
                    .FirstOrDefaultAsync();

                if (signer != null)
                {
                    model.SignedByName = $"{signer.FirstName} {signer.LastName}";
                }
            }

            // Calculate metrics and populate ViewBag data for tabs
            await PopulateDetailsViewBag(policy);

            return View(model);
        }

        private async Task PopulateDetailsViewBag(OshPolicy policy)
        {
            // 1. Calculate Compliance Score
            ViewBag.ComplianceScore = CalculateComplianceScore(policy);

            // 2. Get Document Counts
            var documentCounts = await GetDocumentCounts(policy.PolicyId);
            ViewBag.PolicyDocumentCount = documentCounts.PolicyCount;
            ViewBag.CharterDocumentCount = documentCounts.CharterCount;
            ViewBag.TotalDocumentCount = documentCounts.TotalCount;

            // 3. Calculate Days Until Review
            if (policy.LastReviewedDate.HasValue)
            {
                var nextReviewDue = policy.LastReviewedDate.Value.AddYears(1);
                var daysUntil = (nextReviewDue - DateTime.UtcNow).Days;
                ViewBag.DaysUntilReview = daysUntil;
                ViewBag.NextReviewDue = nextReviewDue;
                ViewBag.IsReviewOverdue = daysUntil < 0;
                ViewBag.IsReviewDueSoon = daysUntil > 0 && daysUntil <= 60;
            }
            else
            {
                ViewBag.DaysUntilReview = null;
                ViewBag.NextReviewDue = null;
                ViewBag.IsReviewOverdue = false;
                ViewBag.IsReviewDueSoon = false;
            }

            // 4. Days Since Last Review
            ViewBag.DaysSinceReview = policy.LastReviewedDate.HasValue 
                ? (DateTime.UtcNow - policy.LastReviewedDate.Value).Days 
                : (DateTime.UtcNow - policy.CreatedAt).Days;

            // 5. Count Defined Stakeholders
            var stakeholderCount = 0;
            if (!string.IsNullOrWhiteSpace(policy.ManagementResponsibilities)) stakeholderCount++;
            if (!string.IsNullOrWhiteSpace(policy.SupervisorResponsibilities)) stakeholderCount++;
            if (!string.IsNullOrWhiteSpace(policy.OutsourcedResponsibilities)) stakeholderCount++;
            if (!string.IsNullOrWhiteSpace(policy.ContractorResponsibilities)) stakeholderCount++;
            ViewBag.StakeholderCount = stakeholderCount;

            // 6. Compliance Checklist Items
            ViewBag.ComplianceChecklist = new List<dynamic>
            {
                new { Item = "Policy Signed by Management", IsCompliant = policy.HasTopManagementSignature, Icon = "ri-quill-pen-line" },
                new { Item = "Policy Being Implemented", IsCompliant = policy.IsPolicyImplemented, Icon = "ri-checkbox-circle-line" },
                new { Item = "Management Responsibilities Defined", IsCompliant = !string.IsNullOrWhiteSpace(policy.ManagementResponsibilities), Icon = "ri-user-star-line" },
                new { Item = "Supervisor Responsibilities Defined", IsCompliant = !string.IsNullOrWhiteSpace(policy.SupervisorResponsibilities), Icon = "ri-user-settings-line" },
                new { Item = "Contractor Charter Exists", IsCompliant = policy.ContractorCharterExists, Icon = "ri-file-text-line" },
                new { Item = "Policy Documents Uploaded", IsCompliant = documentCounts.TotalCount > 0, Icon = "ri-file-list-line" },
                new { Item = "Reviewed Within 12 Months", IsCompliant = policy.LastReviewedDate.HasValue && (DateTime.UtcNow - policy.LastReviewedDate.Value).Days <= 365, Icon = "ri-calendar-check-line" }
            };

            // 7. Implementation Progress (simplified for Phase 1)
            var implementationProgress = 0;
            if (policy.HasTopManagementSignature) implementationProgress += 20;
            if (policy.IsPolicyImplemented) implementationProgress += 30;
            if (!string.IsNullOrWhiteSpace(policy.CommunicationMethods)) implementationProgress += 20;
            if (policy.LastReviewedDate.HasValue) implementationProgress += 15;
            if (documentCounts.TotalCount > 0) implementationProgress += 15;
            ViewBag.ImplementationProgress = implementationProgress;
        }

        private int CalculateComplianceScore(OshPolicy policy)
        {
            int score = 0;
            int maxScore = 100;

            // Signature (20 points)
            if (policy.HasTopManagementSignature && policy.DateSigned.HasValue && !string.IsNullOrEmpty(policy.SignedByPayroll))
                score += 20;

            // Implementation (15 points)
            if (policy.IsPolicyImplemented)
                score += 15;

            // Responsibilities defined (25 points - 5 each + 5 bonus for all)
            int responsibilityCount = 0;
            if (!string.IsNullOrWhiteSpace(policy.ManagementResponsibilities)) { score += 5; responsibilityCount++; }
            if (!string.IsNullOrWhiteSpace(policy.SupervisorResponsibilities)) { score += 5; responsibilityCount++; }
            if (!string.IsNullOrWhiteSpace(policy.OutsourcedResponsibilities)) { score += 5; responsibilityCount++; }
            if (!string.IsNullOrWhiteSpace(policy.ContractorResponsibilities)) { score += 5; responsibilityCount++; }
            if (responsibilityCount == 4) score += 5; // Bonus for completing all

            // Communication methods defined (10 points)
            if (!string.IsNullOrWhiteSpace(policy.CommunicationMethods))
                score += 10;

            // Review status (20 points)
            if (policy.LastReviewedDate.HasValue)
            {
                var daysSinceReview = (DateTime.UtcNow - policy.LastReviewedDate.Value).Days;
                if (daysSinceReview <= 365)
                    score += 20; // Full points if reviewed within a year
                else if (daysSinceReview <= 730)
                    score += 10; // Half points if reviewed within 2 years
            }

            // Documents (10 points) - will be checked separately
            // This will be added in the PopulateDetailsViewBag after document count is fetched

            return score;
        }

        private async Task<(int PolicyCount, int CharterCount, int TotalCount)> GetDocumentCounts(int policyId)
        {
            var policyIdString = policyId.ToString();
            
            var associations = await _context.MediaAssociations
                .Where(ma => ma.AssociatedTable == "OshPolicies" 
                    && ma.AssociatedRecordId == policyIdString 
                    && ma.IsActive)
                .ToListAsync();

            var policyCount = associations.Count(a => a.AssociationType == "policy_document");
            var charterCount = associations.Count(a => a.AssociationType == "charter_document");

            return (policyCount, charterCount, associations.Count);
        }

        private async Task<bool> PolicyExists(int id)
        {
            return await _context.OshPolicies.AnyAsync(p => p.PolicyId == id);
        }

        private async Task UpdateOrgMetadataAsync(int stationId)
        {
            try
            {
                var metadata = await _context.OrgMetadata
                    .FirstOrDefaultAsync(m => m.StationId == stationId);

                if (metadata == null)
                {
                    metadata = new OrgMetadata
                    {
                        StationId = stationId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OrgMetadata.Add(metadata);
                }

                var policy = await _context.OshPolicies
                    .FirstOrDefaultAsync(p => p.StationId == stationId);

                metadata.OshPolicyExists = policy != null;

                if (policy != null)
                {
                    // Determine compliance status
                    if (policy.PolicyStatus == "Active" &&
                        policy.HasTopManagementSignature &&
                        policy.IsPolicyImplemented &&
                        policy.LastReviewedDate.HasValue &&
                        !string.IsNullOrWhiteSpace(policy.ManagementResponsibilities) &&
                        !string.IsNullOrWhiteSpace(policy.SupervisorResponsibilities))
                    {
                        metadata.ComplianceStatus = "Compliant";
                    }
                    else if (policy.PolicyStatus != "Active" ||
                             !policy.HasTopManagementSignature ||
                             !policy.IsPolicyImplemented)
                    {
                        metadata.ComplianceStatus = "NonCompliant";
                    }
                    else
                    {
                        metadata.ComplianceStatus = "PartiallyCompliant";
                    }
                }
                else
                {
                    metadata.ComplianceStatus = "NotAssessed";
                }

                metadata.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OrgMetadata for station {StationId}", stationId);
            }
        }

        // AJAX: Get employees by station for signer dropdown
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByStation(int stationId)
        {
            try
            {
                var employeesQuery = _context.Employees
                    .Where(e => e.EmploymentStatus == "Active" && e.StationId == stationId)
                    .AsQueryable();

                // Apply scope filtering
                employeesQuery = _scopeFilterService.ApplyScope(employeesQuery, CurrentScope);

                var employees = await employeesQuery
                    .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                    .Select(e => new
                    {
                        payrollNo = e.PayrollNo,
                        fullName = e.FirstName + " " + e.LastName,
                        designation = e.Designation
                    })
                    .ToListAsync();

                return Json(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees for station {StationId}", stationId);
                return Json(new List<object>());
            }
        }

        // GET: OshPolicy/GetPolicyDocuments
        [HttpGet]
        public async Task<IActionResult> GetPolicyDocuments(int policyId)
        {
            try
            {
                // Verify policy exists and is accessible
                var policyQuery = _context.OshPolicies
                    .Where(p => p.PolicyId == policyId)
                    .AsQueryable();

                // Apply scope filtering
                policyQuery = _scopeFilterService.ApplyScope(policyQuery, CurrentScope);

                var policy = await policyQuery.FirstOrDefaultAsync();

                if (policy == null)
                {
                    return Json(new { success = false, message = "Policy not found" });
                }

                // Get all media associations for this policy
                var documents = await _context.MediaAssociations
                    .Where(ma => ma.AssociatedTable == "OshPolicies"
                               && ma.AssociatedRecordId == policyId.ToString()
                               && ma.IsActive)
                    .Include(ma => ma.Media)
                    .Select(ma => new
                    {
                        mediaId = ma.Media.MediaId,
                        originalFileName = ma.Media.OriginalFilename,
                        fileExtension = ma.Media.FileExtension,
                        fileSize = ma.Media.FileSizeBytes,
                        title = ma.Media.Title,
                        description = ma.Media.Description,
                        fileUrl = ma.Media.FilePath,
                        uploadedAt = ma.Media.CreatedAt,
                        associationType = ma.AssociationType,
                        displayOrder = ma.DisplayOrder
                    })
                    .OrderBy(d => d.displayOrder)
                    .ThenBy(d => d.uploadedAt)
                    .ToListAsync();

                return Json(new { success = true, documents });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching documents for policy {PolicyId}", policyId);
                return Json(new { success = false, message = "Error loading documents" });
            }
        }

        private async Task UploadAndAssociateFilesAsync(int policyId, int stationId, IFormFile? policyDocument, IFormFile? charterDocument)
        {
            try
            {
                // Upload policy document
                if (policyDocument != null && policyDocument.Length > 0)
                {
                    _logger.LogInformation($"Uploading policy document for policy {policyId}: {policyDocument.FileName}");

                    var policyMediaFile = await _mediaService.UploadAsync(
                        policyDocument,
                        "osh_policy_documents",
                        new MediaUploadOptions
                        {
                            Title = $"OSH Policy Document - {policyDocument.FileName}",
                            Description = "Official OSH policy document",
                            UploadedByPayroll = CurrentPayrollNo,
                            StationId = stationId
                        }
                    );

                    // Associate with policy
                    await _mediaService.AssociateAsync(
                        policyMediaFile.MediaId,
                        "OshPolicies",
                        policyId.ToString(),
                        "policy_document",
                        new MediaAssociationOptions
                        {
                            AssociationLabel = "OSH Policy Document",
                            IsPrimary = true,
                            DisplayOrder = 1
                        }
                    );

                    _logger.LogInformation($"Successfully uploaded and associated policy document {policyMediaFile.MediaId} with policy {policyId}");
                }

                // Upload charter document
                if (charterDocument != null && charterDocument.Length > 0)
                {
                    _logger.LogInformation($"Uploading charter document for policy {policyId}: {charterDocument.FileName}");

                    var charterMediaFile = await _mediaService.UploadAsync(
                        charterDocument,
                        "contractor_safety_charters",
                        new MediaUploadOptions
                        {
                            Title = $"Contractor Safety Charter - {charterDocument.FileName}",
                            Description = "Safety charter document for contractors",
                            UploadedByPayroll = CurrentPayrollNo,
                            StationId = stationId
                        }
                    );

                    // Associate with policy
                    await _mediaService.AssociateAsync(
                        charterMediaFile.MediaId,
                        "OshPolicies",
                        policyId.ToString(),
                        "charter_document",
                        new MediaAssociationOptions
                        {
                            AssociationLabel = "Contractor Safety Charter",
                            IsPrimary = false,
                            DisplayOrder = 2
                        }
                    );

                    _logger.LogInformation($"Successfully uploaded and associated charter document {charterMediaFile.MediaId} with policy {policyId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading and associating media files with policy {PolicyId}", policyId);
                // Don't throw - media upload failure shouldn't prevent policy creation
                // But log the error so it can be investigated
            }
        }

        private async Task PopulateDropdowns()
        {
            // Get categories and stations with scope
            ViewBag.OrgCategories = await _organizationService.GetActiveCategoriesAsync();
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);

            // Get employees for signer lookup
            var employeesQuery = _context.Employees
                .Where(e => e.EmploymentStatus == "Active")
                .AsQueryable();
            employeesQuery = _scopeFilterService.ApplyScope(employeesQuery, CurrentScope);

            ViewBag.Employees = await employeesQuery
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
                .ToListAsync();

            // Policy status options
            ViewBag.PolicyStatuses = new List<dynamic>
            {
                new { Value = "Draft", Text = "Draft" },
                new { Value = "Active", Text = "Active" },
                new { Value = "UnderReview", Text = "Under Review" },
                new { Value = "Expired", Text = "Expired" },
                new { Value = "Archived", Text = "Archived" }
            };
        }
    }
}
