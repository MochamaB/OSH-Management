using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class StationViewModel
    {
        public int StationId { get; set; }

        [Required(ErrorMessage = "Station code is required")]
        [StringLength(20, ErrorMessage = "Station code cannot exceed 20 characters")]
        [Display(Name = "Station Code")]
        public string StationCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Station name is required")]
        [StringLength(100, ErrorMessage = "Station name cannot exceed 100 characters")]
        [Display(Name = "Station Name")]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization category is required")]
        [Display(Name = "Organization Category")]
        public int OrgCategoryId { get; set; }

        [Display(Name = "Parent Station")]
        public int? ParentStationId { get; set; }

        [StringLength(50, ErrorMessage = "Legacy mapping cannot exceed 50 characters")]
        [Display(Name = "Legacy Station Mapping")]
        public string? LegacyStationMapping { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display
        public string CategoryName { get; set; } = string.Empty;
        public string? ParentStationName { get; set; }
        public int DepartmentCount { get; set; }
        public int SectionCount { get; set; }
        public int TeamCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper properties for UI
        public string StatusClass => IsActive ? "success" : "danger";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("MMM dd, yyyy") ?? "Never";
    }
}
