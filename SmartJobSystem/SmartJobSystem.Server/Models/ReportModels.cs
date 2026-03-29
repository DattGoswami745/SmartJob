using System;
using System.Collections.Generic;

namespace SmartJobSystem.Server.Models
{
    public class ReportConfiguration
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BaseTable { get; set; } = string.Empty;
        public string SelectedFields { get; set; } = string.Empty; // JSON
        public string? Filters { get; set; } // JSON
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ReportGenerationLog
    {
        public int LogId { get; set; }
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string Format { get; set; } = "Web";
        public string? FilterValues { get; set; } // JSON
    }

    public class FieldDefinition
    {
        public string id { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string type { get; set; } = "string";
    }
}
