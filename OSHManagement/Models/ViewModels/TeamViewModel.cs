using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying team information in lists
    /// </summary>
    public class TeamViewModel
    {
        public int TeamId { get; set; }

        [Display(Name = "Team Name")]
        public string TeamName { get; set; } = string.Empty;

        [Display(Name = "Team Type")]
        public string TeamType { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? TeamDescription { get; set; }

        [Display(Name = "Station")]
        public string StationName { get; set; } = string.Empty;

        public int StationId { get; set; }

        [Display(Name = "Status")]
        public string TeamStatus { get; set; } = string.Empty;

        [Display(Name = "Formation Date")]
        public DateTime FormationDate { get; set; }

        [Display(Name = "Disband Date")]
        public DateTime? DisbandDate { get; set; }

        [Display(Name = "Active Members")]
        public int ActiveMemberCount { get; set; }

        [Display(Name = "Maximum Members")]
        public int? MaxMemberCount { get; set; }

        [Display(Name = "Required Members")]
        public int? RequiredMemberCount { get; set; }

        [Display(Name = "Section Representation")]
        public bool RequiresSectionRepresentation { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new team via multi-step wizard
    /// </summary>
    public class CreateTeamViewModel
    {
        // ========== STEP 1: Basic Information ==========

        [Required(ErrorMessage = "Station is required")]
        [Display(Name = "Station")]
        public int StationId { get; set; }

        [Required(ErrorMessage = "Team type is required")]
        [Display(Name = "Team Type")]
        public int TeamTypeDefinitionId { get; set; }

        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Formation date is required")]
        [Display(Name = "Formation Date")]
        public DateTime FormationDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Team status is required")]
        [StringLength(20)]
        [Display(Name = "Team Status")]
        public string TeamStatus { get; set; } = "Forming";

        [Display(Name = "Disband Date")]
        public DateTime? DisbandDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? TeamDescription { get; set; }

        // ========== STEP 2: Type-Specific Configuration ==========

        // OSH Committee Config
        [Display(Name = "Committee Trained")]
        public bool IsCommitteeTrained { get; set; }

        [Display(Name = "Training Date")]
        public DateTime? TrainingDate { get; set; }

        [Display(Name = "Has Meeting Schedule")]
        public bool HasMeetingSchedule { get; set; }

        [Display(Name = "Inspection Frequency")]
        public string? InspectionFrequency { get; set; }

        [Display(Name = "Next Inspection Date")]
        public DateTime? NextInspectionDate { get; set; }

        // Risk Assessment Config
        [Display(Name = "Assessment Frequency")]
        public string? AssessmentFrequency { get; set; }

        [StringLength(500)]
        [Display(Name = "Team Qualifications")]
        public string? TeamQualifications { get; set; }

        [Display(Name = "Next Assessment Date")]
        public DateTime? NextAssessmentDate { get; set; }

        // Investigation Config
        [StringLength(500)]
        [Display(Name = "Investigation Scope")]
        public string? InvestigationScope { get; set; }

        [StringLength(500)]
        [Display(Name = "Team Expertise")]
        public string? TeamExpertise { get; set; }

        [Display(Name = "Response Time (Hours)")]
        public int ResponseTimeHours { get; set; } = 24;

        [Display(Name = "Escalation Threshold")]
        public string? EscalationThreshold { get; set; }

        // ========== STEP 3: Members ==========

        [Display(Name = "Team Members")]
        public List<AddTeamMemberDto> Members { get; set; } = new List<AddTeamMemberDto>();

        // For member payroll numbers from form submission
        public List<string> SelectedMemberPayrolls { get; set; } = new List<string>();

        // ========== Additional Properties ==========

        [Display(Name = "Save as Draft")]
        public bool SaveAsDraft { get; set; }
    }

    /// <summary>
    /// DTO for adding/updating a member to a team
    /// </summary>
    public class AddTeamMemberDto
    {
        public int MemberId { get; set; } // 0 for new, >0 for edit

        [Required(ErrorMessage = "Team is required")]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public string EmployeePayroll { get; set; } = string.Empty;

        [Required(ErrorMessage = "Member role is required")]
        public int TeamRoleDefinitionId { get; set; }

        public int? SectionId { get; set; }

        [StringLength(50)]
        public string? EducationLevel { get; set; }

        [StringLength(200)]
        public string? RelevantExperience { get; set; }

        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        public bool IsVotingMember { get; set; } = true;

        // For display purposes (not submitted)
        public string? EmployeeName { get; set; }
        public string? Designation { get; set; }
        public string? DepartmentName { get; set; }
        public string? SectionName { get; set; }
    }

    /// <summary>
    /// ViewModel for editing an existing team
    /// </summary>
    public class EditTeamViewModel
    {
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100)]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; } = string.Empty;

        [Display(Name = "Team Type Definition ID")]
        public int? TeamTypeDefinitionId { get; set; }

        [Display(Name = "Team Type")]
        public string TeamType { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? TeamDescription { get; set; }

        [Display(Name = "Station")]
        public int StationId { get; set; }

        [Display(Name = "Status")]
        public string TeamStatus { get; set; } = string.Empty;

        [Display(Name = "Formation Date")]
        public DateTime FormationDate { get; set; }

        [Display(Name = "Required Member Count")]
        public int? RequiredMemberCount { get; set; }

        [Display(Name = "Maximum Member Count")]
        public int? MaxMemberCount { get; set; }

        [Display(Name = "Requires Section Representation")]
        public bool RequiresSectionRepresentation { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for team details page
    /// </summary>
    public class TeamDetailsViewModel
    {
        public Team Team { get; set; } = null!;
        public List<TeamMemberDetailViewModel> Members { get; set; } = new List<TeamMemberDetailViewModel>();
        public CompositionStatusViewModel CompositionStatus { get; set; } = null!;
    }

    /// <summary>
    /// ViewModel for team member details
    /// </summary>
    public class TeamMemberDetailViewModel
    {
        public int MemberId { get; set; }
        public string EmployeePayroll { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public int TeamRoleDefinitionId { get; set; }
        public string MemberRole { get; set; } = string.Empty; // Display name from TeamRoleDefinition
        public string? SectionName { get; set; }
        public int? SectionId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public bool IsVotingMember { get; set; }
        public string? EducationLevel { get; set; }
        public string? RelevantExperience { get; set; }
    }

    /// <summary>
    /// ViewModel for team composition validation status
    /// </summary>
    public class CompositionStatusViewModel
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public Dictionary<string, int> RoleCounts { get; set; } = new Dictionary<string, int>();
        public int TotalMembers { get; set; }
        public int RequiredMembers { get; set; }
        public int? MaxMembers { get; set; }
        public bool HasSectionRepresentation { get; set; }
        public int SectionsRepresented { get; set; }
    }
}
