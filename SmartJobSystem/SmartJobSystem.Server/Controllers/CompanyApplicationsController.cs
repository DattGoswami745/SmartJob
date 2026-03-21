using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SmartJobSystem.Server.Helpers;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/company/applications")]
    [ApiController]
    public class CompanyApplicationsController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IConfiguration _config;

        public CompanyApplicationsController(DbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyApplications()
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can access this.");

            var apps = await _db.GetApplicationsByCompanyAsync(companyId.Value);
            return Ok(apps);
        }

        [HttpGet("report")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string? search, [FromQuery] int? jobId)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can access this.");

            var allApps = await _db.GetApplicationsByCompanyAsync(companyId.Value);
            
            // Filter the data in-memory for the report
            var query = allApps.Cast<dynamic>();

            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                query = query.Where(a => ((string)a.FullName).ToLower().Contains(s));
            }

            if (jobId.HasValue && jobId.Value > 0)
            {
                query = query.Where(a => (int)a.JobId == jobId.Value);
            }

            var filteredApps = query.ToList();

            using (var ms = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylesPart.Stylesheet = new Stylesheet(
                        new Fonts(
                            new Font(), // Default
                            new Font(new Bold()) // Bold
                        ),
                        new Fills(new Fill()),
                        new Borders(new Border()),
                        new CellFormats(
                            new CellFormat(), // Default
                            new CellFormat { FontId = 1, ApplyFont = true } // Bold
                        )
                    );

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Applications" };
                    sheets.Append(sheet);

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // 1. Metadata Rows
                    string downloadedBy = HttpContext.Session.GetString("UserName") ?? "Recruiter";
                    string downloadTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

                    var metaRow1 = new Row();
                    metaRow1.Append(
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Downloaded By:"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue(downloadedBy) }
                    );
                    sheetData.AppendChild(metaRow1);

                    var metaRow2 = new Row();
                    metaRow2.Append(
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Download Time:"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue(downloadTime) }
                    );
                    sheetData.AppendChild(metaRow2);

                    sheetData.AppendChild(new Row()); // Spacer

                    // 2. Header row
                    var headerRow = new Row();
                    headerRow.Append(
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Applicant Name"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Email"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Job Title"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Job Type"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Company Name"), StyleIndex = 1 },
                        new Cell() { DataType = CellValues.String, CellValue = new CellValue("Applied Date"), StyleIndex = 1 }
                    );
                    sheetData.AppendChild(headerRow);

                    // 3. Data rows
                    foreach (var app in filteredApps)
                    {
                        var row = new Row();
                        row.Append(
                            new Cell() { DataType = CellValues.String, CellValue = new CellValue((string)app.FullName) },
                            new Cell() { DataType = CellValues.String, CellValue = new CellValue((string)app.Email) },
                            new Cell() { DataType = CellValues.String, CellValue = new CellValue((string)app.JobTitle) },
                            new Cell() { DataType = CellValues.String, CellValue = new CellValue((string)app.JobType) },
                            new Cell() { DataType = CellValues.String, CellValue = new CellValue((string)app.CompanyName) },
                            new Cell() { DataType = CellValues.String, CellValue = new CellValue(((DateTime)app.AppliedDate).ToShortDateString()) }
                        );
                        sheetData.AppendChild(row);
                    }

                    workbookPart.Workbook.Save();
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Applications_Report.xlsx");
            }
        }

        [HttpDelete("{appId}")]
        public async Task<IActionResult> DeleteApplication(int appId)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can access this.");

            bool deleted = await _db.DeleteCompanyApplicationAsync(companyId.Value, appId);
            if (!deleted)
                return NotFound(new { message = "Application not found or does not belong to your company." });

            return Ok(new { message = "Application removed successfully." });
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfileForCompany(int userId)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can access this.");

            var profile = await _db.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFound(new { message = "Profile not found." });

            var encryptionKey = _config["SecuritySettings:EncryptionKey"] ?? "";
            var profileData = profile as dynamic;
            string resumePath = profileData?.ResumePath ?? "";

            // Decrypt ResumePath if it looks encrypted
            if (!string.IsNullOrEmpty(resumePath) && !resumePath.StartsWith("/api/"))
            {
                try {
                    resumePath = SecurityHelper.Decrypt(resumePath, encryptionKey);
                } catch { /* Skip if not encrypted */ }
            }

            return Ok(new {
                profileId = profileData.ProfileId,
                userId = profileData.UserId,
                skills = profileData.Skills,
                experienceYears = profileData.ExperienceYears,
                education = profileData.Education,
                preferredLocation = profileData.PreferredLocation,
                resumePath = resumePath
            });
        }

        [HttpPost("mark-placed/{appId}")]
        public async Task<IActionResult> MarkPlaced(int appId)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can perform this action.");

            // 🔍 Verification Check
            if (!await _db.IsCompanyVerifiedAsync(companyId.Value))
            {
                return BadRequest("Company verification pending. Please upload required documents and wait for admin approval.");
            }

            bool success = await _db.MarkApplicationAsPlacedAsync(companyId.Value, appId);
            if (!success)
                return NotFound(new { message = "Application not found or does not belong to your company." });

            return Ok(new { message = "Candidate marked as Placed successfully!" });
        }
    }
}
