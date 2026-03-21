using System.Text.Json.Serialization;

namespace SmartJobSystem.Server.Models
{
    public class Job
    {
        public int JobId { get; set; }
        public int CompanyId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? RequiredSkills { get; set; }
        public string? JobType { get; set; }
        public string? SalaryRange { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? LastDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }

        // Job Description Feature
        public string? JobDescriptionFile { get; set; }
        public string? JobDescriptionText { get; set; }
        public DateTime? JobDescriptionUpdatedAt { get; set; }

        // Optional property for UI display
        public string? CompanyName { get; set; }
    }
}