using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Helpers;
using SmartJobSystem.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/reports")]
    [ApiController]
    public class CentralReportsController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IReportExportService _exportService;

        public CentralReportsController(DbHelper db, IReportExportService exportService)
        {
            _db = db;
            _exportService = exportService;
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> DownloadJobReport(int jobId)
        {
            var reportData = await _db.GetJobApplicantsReportAsync(jobId);
            var dReport = (dynamic)reportData;

            var jobTitle = dReport.JobTitle;
            var applicants = (List<object>)dReport.Applicants;

            if (applicants == null || applicants.Count == 0)
            {
                return NotFound("No applicants found for this job.");
            }

            var headers = GetApplicantHeaders();
            var data = MapApplicantsToData(applicants);
            string userName = HttpContext.Session.GetString("UserName") ?? "System";

            var bytes = _exportService.GenerateExcel($"{jobTitle} - Applicants", headers, data, userName);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"JobReport_{jobId}.xlsx");
        }

        [HttpGet("multi")]
        public async Task<IActionResult> DownloadMultiFilterReport([FromQuery] int? companyId, [FromQuery] int? jobId)
        {
            var applicants = (List<object>)await _db.GetCentralMultiFilterReportAsync(companyId, jobId);

            if (applicants == null || applicants.Count == 0)
            {
                return NotFound("No applicants found for the selected filters.");
            }

            string reportTitle = "Consolidated Report";
            string fileName = "Consolidated_Report.xlsx";

            var headers = GetApplicantHeaders();
            var data = MapApplicantsToData(applicants);
            string userName = HttpContext.Session.GetString("UserName") ?? "System";

            var bytes = _exportService.GenerateExcel(reportTitle, headers, data, userName);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private List<FieldDefinition> GetApplicantHeaders()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { id = "FullName", label = "Full Name" },
                new FieldDefinition { id = "Email", label = "Email" },
                new FieldDefinition { id = "JobTitle", label = "Job Title" },
                new FieldDefinition { id = "CompanyName", label = "Company" },
                new FieldDefinition { id = "AppliedDate", label = "Applied Date" },
                new FieldDefinition { id = "Status", label = "Status" },
                new FieldDefinition { id = "Experience", label = "Exp (Yrs)" },
                new FieldDefinition { id = "Education", label = "Education" },
                new FieldDefinition { id = "Location", label = "Location" }
            };
        }

        private List<IDictionary<string, object>> MapApplicantsToData(List<object> applicants)
        {
            var data = new List<IDictionary<string, object>>();
            foreach (var app in applicants)
            {
                var d = (dynamic)app;
                var dict = new Dictionary<string, object>
                {
                    ["FullName"] = d.FullName,
                    ["Email"] = d.Email,
                    ["JobTitle"] = d.JobTitle,
                    ["CompanyName"] = d.CompanyName,
                    ["AppliedDate"] = d.AppliedDate?.ToString("yyyy-MM-dd") ?? "",
                    ["Status"] = d.Status,
                    ["Experience"] = d.ExperienceValue,
                    ["Education"] = d.Education,
                    ["Location"] = d.Location
                };
                data.Add(dict);
            }
            return data;
        }
    }
}
