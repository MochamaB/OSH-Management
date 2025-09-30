using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaAssociation
    {
        [Key]
        public int AssociationId { get; set; }

        public int MediaId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssociatedTable { get; set; } = string.Empty;

        public int AssociatedRecordId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssociationType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AssociationLabel { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsRequired { get; set; }
        public int? MaxFilesAllowed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public MediaFile Media { get; set; } = null!;
    }
}
