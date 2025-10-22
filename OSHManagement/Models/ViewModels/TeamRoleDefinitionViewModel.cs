using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class TeamRoleDefinitionViewModel
    {
        public int TeamRoleDefinitionId { get; set; }

        [Required(ErrorMessage = "Team type is required")]
        [Display(Name = "Team Type")]
        public string TeamType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Requires Voting Rights")]
        public bool RequiresVotingRights { get; set; } = true;

        [Display(Name = "Max Occurrences")]
        [Range(1, 100, ErrorMessage = "Max occurrences must be between 1 and 100")]
        public int? MaxOccurrences { get; set; }

        [Display(Name = "Min Occurrences")]
        [Range(1, 100, ErrorMessage = "Min occurrences must be between 1 and 100")]
        public int? MinOccurrences { get; set; }

        [StringLength(500, ErrorMessage = "Required qualifications cannot exceed 500 characters")]
        [Display(Name = "Required Qualifications")]
        public string? RequiredQualifications { get; set; }

        [Required]
        [Display(Name = "Display Order")]
        [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display
        [Display(Name = "Current Usage")]
        public int UsageCount { get; set; }
    }
}
