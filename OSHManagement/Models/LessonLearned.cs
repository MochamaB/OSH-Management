using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class LessonLearned
    {
        [Key]
        public int LessonId { get; set; }

        public int IncidentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string LessonTitle { get; set; } = string.Empty;

        [Required]
        public string LessonDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LessonCategory { get; set; } = string.Empty;

        public string? ApplicableToStations { get; set; }
        public string? ApplicableToSections { get; set; }

        [Required]
        [MaxLength(10)]
        public string LessonPriority { get; set; } = string.Empty;

        public DateTime SharedDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ImplementationStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string SharedByPayroll { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Incident Incident { get; set; } = null!;
    }
}
