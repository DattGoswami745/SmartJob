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

            var builder = new StringBuilder();

            // Create an HTML-based Excel file so we can support Bold formatting
            builder.AppendLine("<html>");
            builder.AppendLine("<head><meta charset='utf-8'></head>");
            builder.AppendLine("<body>");

            builder.AppendLine("<table>");

            // --- Metadata Headers (Bold) ---
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td colspan='2'><b>Report Name:</b></td><td colspan='6'>{EscapeHtml(jobTitle)} Applicant Report</td>");
            builder.AppendLine("</tr>");
            
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td colspan='2'><b>Company Name:</b></td><td colspan='6'>{EscapeHtml(companyName)}</td>");
            builder.AppendLine("</tr>");

            builder.AppendLine("<tr>");
            builder.AppendLine($"<td colspan='2'><b>Downloaded By:</b></td><td colspan='6'>System Administrator</td>");
            builder.AppendLine("</tr>");

            builder.AppendLine("<tr>");
            builder.AppendLine($"<td colspan='2'><b>Downloaded (UTC):</b></td><td colspan='6'>{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}</td>");
            builder.AppendLine("</tr>");

            // Empty row separator
            builder.AppendLine("<tr><td colspan='8'></td></tr>");

            // --- Table Headers (Bold/Background) ---
            builder.AppendLine("<tr style='background-color: #f1f5f9; font-weight: bold;'>");
            builder.AppendLine("<td><b>Full Name</b></td>");
            builder.AppendLine("<td><b>Email</b></td>");
            builder.AppendLine("<td><b>Applied Date</b></td>");
            builder.AppendLine("<td><b>Status</b></td>");
            builder.AppendLine("<td><b>Experience (Yrs)</b></td>");
            builder.AppendLine("<td><b>Education</b></td>");
            builder.AppendLine("<td><b>Location</b></td>");
            builder.AppendLine("<td><b>Skills</b></td>");
            builder.AppendLine("</tr>");

            // --- Data Rows ---
            foreach (var app in applicants)
            {
                // We use dynamic to easily read the anonymous object returned by DbHelper
                var d = (dynamic)app;

                var fullName = EscapeHtml(d.FullName);
                var email = EscapeHtml(d.Email);
                var appliedDate = d.AppliedDate.ToString("yyyy-MM-dd HH:mm");
                var status = EscapeHtml(d.Status);
                var experience = d.Experience.ToString();
                var education = EscapeHtml(d.Education);
                var location = EscapeHtml(d.Location);
                var skills = EscapeHtml(d.Skills);

                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{fullName}</td>");
                builder.AppendLine($"<td>{email}</td>");
                builder.AppendLine($"<td>{appliedDate}</td>");
                builder.AppendLine($"<td>{status}</td>");
                builder.AppendLine($"<td>{experience}</td>");
                builder.AppendLine($"<td>{education}</td>");
                builder.AppendLine($"<td>{location}</td>");
                builder.AppendLine($"<td>{skills}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</table>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            // Return the Excel File
            var fileBytes = Encoding.UTF8.GetBytes(builder.ToString());
            
            return File(fileBytes, "application/vnd.ms-excel", $"JobReport_Company_{jobId}.xls");
        }

        // Helper to escape HTML characters
        private string EscapeHtml(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return System.Net.WebUtility.HtmlEncode(field);
        }
    }
}
