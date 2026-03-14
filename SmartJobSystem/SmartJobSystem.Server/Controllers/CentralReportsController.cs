using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using System.Text;
using System.Threading.Tasks;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/reports")]
    [ApiController]
    public class CentralReportsController : ControllerBase
    {
        private readonly DbHelper _db;

        public CentralReportsController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> DownloadJobReport(int jobId)
        {
            var reportData = await _db.GetJobApplicantsReportAsync(jobId);
            var dReport = (dynamic)reportData;

            var jobTitle = dReport.JobTitle;
            var companyName = dReport.CompanyName;
            var applicants = dReport.Applicants;

            if (applicants == null || applicants.Count == 0)
            {
                return NotFound("No applicants found for this job.");
            }

            return GenerateExcelReport(jobTitle, companyName, applicants, $"JobReport_{jobId}.xls");
        }

        [HttpGet("multi")]
        public async Task<IActionResult> DownloadMultiFilterReport([FromQuery] int? companyId, [FromQuery] int? jobId)
        {
            var applicants = (List<object>)await _db.GetCentralMultiFilterReportAsync(companyId, jobId);

            if (applicants == null || applicants.Count == 0)
            {
                return NotFound("No applicants found for the selected filters.");
            }

            string reportTitle = "Consolidated";
            if (companyId.HasValue && companyId > 0) reportTitle += "_Company_" + companyId;
            if (jobId.HasValue && jobId > 0) reportTitle += "_Job_" + jobId;

            return GenerateExcelReport("Consolidated Report", "Multiple Companies/Jobs", applicants, $"{reportTitle}_Report.xls");
        }

        private IActionResult GenerateExcelReport(string title, string company, List<object> applicants, string fileName)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("<head><meta charset='utf-8'></head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<table>");

            // Metadata
            builder.AppendLine($"<tr><td colspan='2'><b>Report Title:</b></td><td colspan='7'>{EscapeHtml(title)}</td></tr>");
            builder.AppendLine($"<tr><td colspan='2'><b>Scope:</b></td><td colspan='7'>{EscapeHtml(company)}</td></tr>");
            builder.AppendLine($"<tr><td colspan='2'><b>Date:</b></td><td colspan='7'>{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</td></tr>");
            builder.AppendLine("<tr><td colspan='9'></td></tr>");

            // Headers
            builder.AppendLine("<tr style='background-color: #f1f5f9; font-weight: bold;'>");
            builder.AppendLine("<td><b>Full Name</b></td>");
            builder.AppendLine("<td><b>Email</b></td>");
            builder.AppendLine("<td><b>Job Title</b></td>");
            builder.AppendLine("<td><b>Company</b></td>");
            builder.AppendLine("<td><b>Applied Date</b></td>");
            builder.AppendLine("<td><b>Status</b></td>");
            builder.AppendLine("<td><b>Exp (Yrs)</b></td>");
            builder.AppendLine("<td><b>Education</b></td>");
            builder.AppendLine("<td><b>Location</b></td>");
            builder.AppendLine("</tr>");

            foreach (var app in applicants)
            {
                var d = (dynamic)app;
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{EscapeHtml(d.FullName)}</td>");
                builder.AppendLine($"<td>{EscapeHtml(d.Email)}</td>");
                builder.AppendLine($"<td>{EscapeHtml(d.JobTitle)}</td>");
                builder.AppendLine($"<td>{EscapeHtml(d.CompanyName)}</td>");
                builder.AppendLine($"<td>{d.AppliedDate:yyyy-MM-dd}</td>");
                builder.AppendLine($"<td>{EscapeHtml(d.Status)}</td>");
                builder.AppendLine($"<td>{d.ExperienceValue}</td>");
                builder.AppendLine($"<td>{EscapeHtml(d.Education)}</td>");
                builder.AppendLine($"<td>{EscapeHtml(d.Location)}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</table></body></html>");

            var fileBytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(fileBytes, "application/vnd.ms-excel", fileName);
        }

        // Helper to escape HTML characters
        private string EscapeHtml(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return System.Net.WebUtility.HtmlEncode(field);
        }
    }
}
