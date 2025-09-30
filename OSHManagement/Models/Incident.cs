using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Incident
    {
        [Key]
        public int IncidentId { get; set; }

        public int StationId { get; set; }
        public int? InvestigationTeamId { get; set; }

        [Required]
        [MaxLength(20)]
        public string IncidentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string IncidentSeverity { get; set; } = string.Empty;

        public DateTime IncidentDate { get; set; }
        public TimeSpan? IncidentTime { get; set; }

        [Required]
        [MaxLength(500)]
        public string LocationDescription { get; set; } = string.Empty;

        public int? SectionId { get; set; }

        [MaxLength(20)]
        public string? PersonAffectedPayroll { get; set; }

        [MaxLength(100)]
        public string? PersonAffectedName { get; set; }

        [MaxLength(10)]
        public string? PersonAffectedGender { get; set; }

        [Required]
        public string IncidentDescription { get; set; } = string.Empty;

        public string? InjuryDescription { get; set; }

        public bool RecordedOnSite { get; set; }
        public bool ReportedToDoshmis { get; set; }
        public bool ReportedToMajaniInsurance { get; set; }

        [Required]
        [MaxLength(20)]
        public string IncidentStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ReportedByPayroll { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station Station { get; set; } = null!;
        public Team? InvestigationTeam { get; set; }
        public Section? Section { get; set; }
        public IncidentCause? IncidentCause { get; set; }
        public IncidentInvestigation? Investigation { get; set; }
        public ICollection<ControlAction> ControlActions { get; set; } = new List<ControlAction>();
        public ICollection<LessonLearned> LessonsLearned { get; set; } = new List<LessonLearned>();
    }
}
