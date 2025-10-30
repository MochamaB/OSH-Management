namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel for OSH Committee Details Dashboard
    /// </summary>
    public class OshCommitteeViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string TeamStatus { get; set; } = string.Empty;
        public DateTime FormationDate { get; set; }
        public bool IsActivated { get; set; }
        
        public OshCommitteeConfigViewModel? Config { get; set; }
        public CommitteeMetrics Metrics { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for OSH Committee Configuration
    /// </summary>
    public class OshCommitteeConfigViewModel
    {
        public bool IsCommitteeTrained { get; set; }
        public DateTime? TrainingDate { get; set; }
        public bool HasMeetingSchedule { get; set; }
        public string InspectionFrequency { get; set; } = string.Empty;
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
    }

    /// <summary>
    /// Committee performance metrics
    /// </summary>
    public class CommitteeMetrics
    {
        public int TotalMembers { get; set; }
        public int OpenIssues { get; set; }
        public int PendingRecommendations { get; set; }
        public int ActiveActions { get; set; }
        public int OverdueActions { get; set; }
        public DateTime? NextInspectionDate { get; set; }
    }
}
