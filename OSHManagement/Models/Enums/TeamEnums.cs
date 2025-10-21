using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.Enums
{
    /// <summary>
    /// Team types for OSH management
    /// </summary>
    public enum TeamType
    {
        [Display(Name = "OSH Committee")]
        OshCommittee = 1,

        [Display(Name = "Risk Assessment")]
        RiskAssessment = 2,

        [Display(Name = "Investigation")]
        Investigation = 3,

        [Display(Name = "Custom")]
        Custom = 4
    }

    /// <summary>
    /// Team member roles
    /// </summary>
    public enum TeamMemberRole
    {
        [Display(Name = "Chairperson")]
        Chairperson = 1,

        [Display(Name = "Secretary")]
        Secretary = 2,

        [Display(Name = "Member")]
        Member = 3,

        [Display(Name = "Management Representative")]
        ManagementRepresentative = 4,

        [Display(Name = "Employee Representative")]
        EmployeeRepresentative = 5,

        [Display(Name = "Safety Officer")]
        SafetyOfficer = 6,

        [Display(Name = "Team Leader")]
        TeamLeader = 7,

        [Display(Name = "Technical Expert")]
        TechnicalExpert = 8,

        [Display(Name = "Investigator")]
        Investigator = 9,

        [Display(Name = "Fire Marshal")]
        FireMarshal = 10
    }

    /// <summary>
    /// Frequency options for inspections and assessments
    /// </summary>
    public enum Frequency
    {
        [Display(Name = "Daily")]
        Daily = 1,

        [Display(Name = "Weekly")]
        Weekly = 2,

        [Display(Name = "Monthly")]
        Monthly = 3,

        [Display(Name = "Quarterly")]
        Quarterly = 4,

        [Display(Name = "Semi-Annually")]
        SemiAnnually = 5,

        [Display(Name = "Annually")]
        Annually = 6
    }

    /// <summary>
    /// Education levels for team members
    /// </summary>
    public enum EducationLevel
    {
        [Display(Name = "Certificate")]
        Certificate = 1,

        [Display(Name = "Diploma")]
        Diploma = 2,

        [Display(Name = "Degree")]
        Degree = 3,

        [Display(Name = "Masters")]
        Masters = 4,

        [Display(Name = "PhD")]
        PhD = 5,

        [Display(Name = "Professional Certification")]
        ProfessionalCertification = 6
    }

    /// <summary>
    /// Helper class to get display names from enums
    /// </summary>
    public static class TeamEnumExtensions
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            return displayAttribute?.Length > 0 ? displayAttribute[0].Name ?? enumValue.ToString() : enumValue.ToString();
        }

        public static List<string> GetTeamTypeValues()
        {
            return System.Enum.GetValues(typeof(TeamType))
                .Cast<TeamType>()
                .Select(t => t.GetDisplayName())
                .ToList();
        }

        public static List<string> GetMemberRoleValues()
        {
            return System.Enum.GetValues(typeof(TeamMemberRole))
                .Cast<TeamMemberRole>()
                .Select(r => r.GetDisplayName())
                .ToList();
        }

        public static List<string> GetFrequencyValues()
        {
            return System.Enum.GetValues(typeof(Frequency))
                .Cast<Frequency>()
                .Select(f => f.GetDisplayName())
                .ToList();
        }

        public static List<string> GetEducationLevelValues()
        {
            return System.Enum.GetValues(typeof(EducationLevel))
                .Cast<EducationLevel>()
                .Select(e => e.GetDisplayName())
                .ToList();
        }

        /// <summary>
        /// Get TeamType as dictionary for dropdowns (Value = DisplayName, Key = EnumValue)
        /// </summary>
        public static Dictionary<string, string> GetTeamTypeDictionary()
        {
            return System.Enum.GetValues(typeof(TeamType))
                .Cast<TeamType>()
                .ToDictionary(
                    t => t.ToString(),  // Key: Enum name (e.g., "OshCommittee")
                    t => t.GetDisplayName()  // Value: Display name (e.g., "OSH Committee")
                );
        }

        /// <summary>
        /// Parse display name back to enum
        /// </summary>
        public static TeamType? ParseTeamType(string displayName)
        {
            foreach (TeamType type in System.Enum.GetValues(typeof(TeamType)))
            {
                if (type.GetDisplayName().Equals(displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }
            return null;
        }
    }
}
