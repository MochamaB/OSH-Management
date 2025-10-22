using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class TeamTypeDefinitionViewModel
    {
        public int TeamTypeDefinitionId { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        [StringLength(50, ErrorMessage = "Type name cannot exceed 50 characters")]
        [Display(Name = "Type Name")]
        public string TypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type code is required")]
        [StringLength(30, ErrorMessage = "Type code cannot exceed 30 characters")]
        [Display(Name = "Type Code")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Type code must contain only letters (no spaces or special characters)")]
        public string TypeCode { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Team Limits
        [Display(Name = "Max Teams Per Station")]
        [Range(1, 100, ErrorMessage = "Max teams per station must be between 1 and 100")]
        public int? MaxTeamsPerStation { get; set; }

        // Statutory Compliance
        [Display(Name = "Requires Statutory Compliance")]
        public bool RequiresStatutoryCompliance { get; set; }

        [Display(Name = "Required Employee Rep Ratio (%)")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal? RequiredEmployeeRepRatioPercent { get; set; }

        [Display(Name = "Required Employer Rep Ratio (%)")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal? RequiredEmployerRepRatioPercent { get; set; }

        [Display(Name = "Minimum Female Ratio (%)")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal? MinFemaleRatioPercent { get; set; }

        [Display(Name = "Minimum Male Ratio (%)")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal? MinMaleRatioPercent { get; set; }

        // Meeting Requirements
        [Display(Name = "Minimum Meetings Per Year")]
        [Range(1, 365, ErrorMessage = "Meetings per year must be between 1 and 365")]
        public int? MinMeetingsPerYear { get; set; }

        [Display(Name = "Quorum Percentage (%)")]
        [Range(0, 100, ErrorMessage = "Quorum percentage must be between 0 and 100")]
        public decimal? QuorumPercentageValue { get; set; }

        // Member Constraints
        [Display(Name = "Minimum Member Count")]
        [Range(1, 1000, ErrorMessage = "Minimum member count must be between 1 and 1000")]
        public int? MinMemberCount { get; set; }

        [Display(Name = "Maximum Member Count")]
        [Range(1, 1000, ErrorMessage = "Maximum member count must be between 1 and 1000")]
        public int? MaxMemberCount { get; set; }

        [Display(Name = "Requires Section Representation")]
        public bool RequiresSectionRepresentation { get; set; }

        // Term Management
        [Display(Name = "Default Term (Months)")]
        [Range(1, 120, ErrorMessage = "Term months must be between 1 and 120")]
        public int? DefaultTermMonths { get; set; }

        [Display(Name = "Max Consecutive Terms")]
        [Range(1, 10, ErrorMessage = "Max consecutive terms must be between 1 and 10")]
        public int? MaxConsecutiveTerms { get; set; }

        // System Flags
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "System Type")]
        public bool IsSystemType { get; set; }

        // Display properties
        public int RoleCount { get; set; }
        public int TeamCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
