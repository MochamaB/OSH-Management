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

        // Changed from int to string for polymorphic support
        [Required]
        [MaxLength(100)]
        public string AssociatedRecordId { get; set; } = string.Empty;

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

        // Audit fields
        [MaxLength(20)]
        public string? CreatedByPayroll { get; set; }

        // Soft delete support
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(20)]
        public string? DeletedByPayroll { get; set; }

        // Navigation properties
        public MediaFile Media { get; set; } = null!;
        public Employee? CreatedBy { get; set; }
        public Employee? DeletedBy { get; set; }
    }
}
