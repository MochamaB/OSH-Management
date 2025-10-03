namespace OSHManagement.Models.ViewModels
{
    public class OrgCategoryViewModel
    {
        public int OrgCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int StationCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper properties for UI
        public string StatusClass => IsActive ? "success" : "danger";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("MMM dd, yyyy") ?? "Never";
    }
}
