using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class SectionViewModel
    {
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Section name is required")]
        [StringLength(100, ErrorMessage = "Section name cannot exceed 100 characters")]
        [Display(Name = "Section Name")]
        public string SectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Station is required")]
        [Display(Name = "Station")]
        public int StationId { get; set; }

        [StringLength(20, ErrorMessage = "Supervisor payroll cannot exceed 20 characters")]
        [Display(Name = "Section Supervisor")]
        public string? SectionSupervisorPayroll { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display
        public string StationName { get; set; } = string.Empty;
        public string? SupervisorFullName { get; set; }
        public int TeamMemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper properties for UI
        public string StatusClass => IsActive ? "success" : "danger";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("MMM dd, yyyy") ?? "Never";
    }
}
