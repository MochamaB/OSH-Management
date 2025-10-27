using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class OshPolicyViewModel
    {
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Station is required")]
        [Display(Name = "Station")]
        public int StationId { get; set; }

        // Signature Section
        [Display(Name = "Policy Signed by Top Management")]
        public bool HasTopManagementSignature { get; set; }

        [Display(Name = "Date Signed")]
        public DateTime? DateSigned { get; set; }

        [StringLength(20)]
        [Display(Name = "Signed By (Payroll)")]
        public string? SignedByPayroll { get; set; }

        // Review Section
        [Display(Name = "Last Reviewed Date")]
        public DateTime? LastReviewedDate { get; set; }

        // Implementation Section
        [Display(Name = "Policy is Being Implemented")]
        public bool IsPolicyImplemented { get; set; }

        // Communication Section
        [StringLength(2000)]
        [Display(Name = "Communication Methods")]
        public string? CommunicationMethods { get; set; }

        // Responsibilities Section
        [Display(Name = "Management Responsibilities")]
        public string? ManagementResponsibilities { get; set; }

        [Display(Name = "Supervisor Responsibilities")]
        public string? SupervisorResponsibilities { get; set; }

        [Display(Name = "Outsourced Labour Responsibilities")]
        public string? OutsourcedResponsibilities { get; set; }

        [Display(Name = "Contractor Responsibilities")]
        public string? ContractorResponsibilities { get; set; }

        // Contractor Charter
        [Display(Name = "Contractor Charter Exists")]
        public bool ContractorCharterExists { get; set; }

        // Policy Status
        [Required]
        [StringLength(20)]
        [Display(Name = "Policy Status")]
        public string PolicyStatus { get; set; } = "Draft";

        // Audit Fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Read-only properties for display
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string? OrgCategoryName { get; set; }
        public string? SignedByName { get; set; }

        // Helper properties for UI
        public string FormattedDateSigned => DateSigned?.ToString("MMM dd, yyyy") ?? "—";
        public string FormattedLastReviewed => LastReviewedDate?.ToString("MMM dd, yyyy") ?? "—";
        public DateTime? NextReviewDue => LastReviewedDate?.AddYears(1);
        public string FormattedNextReviewDue => NextReviewDue?.ToString("MMM dd, yyyy") ?? "—";
        public int? DaysSinceReview => LastReviewedDate.HasValue ? (DateTime.Today - LastReviewedDate.Value).Days : null;
        public int? DaysUntilReviewDue => NextReviewDue.HasValue ? (NextReviewDue.Value - DateTime.Today).Days : null;
        public bool IsReviewOverdue => DaysUntilReviewDue.HasValue && DaysUntilReviewDue.Value < 0;

        public string PolicyStatusBadge => PolicyStatus switch
        {
            "Active" => "bg-success-transparent",
            "Draft" => "bg-secondary-transparent",
            "UnderReview" => "bg-warning-transparent",
            "Expired" => "bg-danger-transparent",
            "Archived" => "bg-light text-muted",
            _ => "bg-secondary-transparent"
        };

        public string ComplianceStatusBadge
        {
            get
            {
                if (PolicyStatus != "Active") return "bg-danger-transparent";
                if (!HasTopManagementSignature) return "bg-danger-transparent";
                if (!IsPolicyImplemented) return "bg-danger-transparent";
                if (string.IsNullOrWhiteSpace(ManagementResponsibilities) ||
                    string.IsNullOrWhiteSpace(SupervisorResponsibilities)) return "bg-warning-transparent";
                if (!LastReviewedDate.HasValue) return "bg-warning-transparent";
                if (IsReviewOverdue) return "bg-warning-transparent";
                return "bg-success-transparent";
            }
        }

        public string ComplianceStatusText
        {
            get
            {
                if (PolicyStatus != "Active") return "Critical";
                if (!HasTopManagementSignature) return "Critical";
                if (!IsPolicyImplemented) return "Critical";
                if (string.IsNullOrWhiteSpace(ManagementResponsibilities) ||
                    string.IsNullOrWhiteSpace(SupervisorResponsibilities)) return "Needs Attention";
                if (!LastReviewedDate.HasValue) return "Needs Attention";
                if (IsReviewOverdue) return "Needs Attention";
                return "Compliant";
            }
        }
    }
}
