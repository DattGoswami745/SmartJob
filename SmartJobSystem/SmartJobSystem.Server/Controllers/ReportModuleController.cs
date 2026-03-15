using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Helpers;
using SmartJobSystem.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportModuleController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IReportExportService _exportService;

        public ReportModuleController(DbHelper db, IReportExportService exportService)
        {
            _db = db;
            _exportService = exportService;
        }

        // ... existing methods ...

        [HttpGet("export/{id}/{format}")]
        public async Task<IActionResult> ExportReport(int id, string format, [FromQuery] string? filterJson)
        {
            var config = await _db.GetReportConfigurationByIdAsync(id);
            if (config == null) return NotFound();

            var fields = JsonSerializer.Deserialize<List<FieldDefinition>>(config.SelectedFields);
            if (fields == null) return BadRequest();

            var filters = string.IsNullOrEmpty(filterJson) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(filterJson) ?? new Dictionary<string, object>();

            // Fetch data (reuse logic or call it)
            string[] fieldNames = fields.Select(f => f.id).ToArray();
            var parameters = new Dictionary<string, object>();
            string filterClause = ""; // Logic to build filter clause same as in GenerateReport

            // TODO: Extract filter building logic to a helper method to keep it DRY
            if (filters.Count > 0)
            {
                var filterParts = new List<string>();
                foreach (var filter in filters)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(filter.Key, @"^[a-zA-Z0-9_]+$")) continue;
                    string paramName = $"@p{parameters.Count}";
                    filterParts.Add($"{filter.Key} = {paramName}");
                    parameters.Add(paramName, filter.Value);
                }
                filterClause = string.Join(" AND ", filterParts);
            }

            var data = await _db.GetDynamicReportDataAsync(config.BaseTable, fieldNames, filterClause, parameters);
            string userName = HttpContext.Session.GetString("UserName") ?? "System";

            if (format.ToLower() == "excel")
            {
                var bytes = _exportService.GenerateExcel(config.ReportName, fields, data, userName);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{config.ReportName}.xlsx");
            }
            else if (format.ToLower() == "pdf")
            {
                var bytes = _exportService.GeneratePdf(config.ReportName, fields, data, userName);
                return File(bytes, "application/pdf", $"{config.ReportName}.pdf");
            }

            return BadRequest("Invalid format.");
        }

        // ================= CONFIGURATION =================

        [HttpGet("config")]
        public async Task<IActionResult> GetReportConfigs()
        {
            var configs = await _db.GetReportConfigurationsAsync();
            return Ok(configs);
        }

        [HttpGet("config/{id}")]
        public async Task<IActionResult> GetReportConfig(int id)
        {
            var config = await _db.GetReportConfigurationByIdAsync(id);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpPost("config")]
        public async Task<IActionResult> CreateReportConfig([FromBody] ReportConfiguration config)
        {
            // Simple validation
            if (string.IsNullOrEmpty(config.ReportName) || string.IsNullOrEmpty(config.BaseTable))
                return BadRequest("Report Name and Base Table are required.");

            int id = await _db.AddReportConfigurationAsync(config);
            return Ok(new { ReportId = id, Message = "Report configuration saved successfully." });
        }

        [HttpPut("config/{id}")]
        public async Task<IActionResult> UpdateReportConfig(int id, [FromBody] ReportConfiguration config)
        {
            config.ReportId = id;
            bool success = await _db.UpdateReportConfigurationAsync(config);
            if (!success) return NotFound();
            return Ok(new { Message = "Report configuration updated successfully." });
        }

        // ================= GENERATION =================

        [HttpPost("generate/{id}")]
        public async Task<IActionResult> GenerateReport(int id, [FromBody] ReportGenerationRequest request)
        {
            var config = await _db.GetReportConfigurationByIdAsync(id);
            if (config == null) return NotFound("Report configuration not found.");

            // 1. Parse fields
            var fields = JsonSerializer.Deserialize<List<FieldDefinition>>(config.SelectedFields);
            if (fields == null || fields.Count == 0) return BadRequest("No fields defined for this report.");

            string[] fieldNames = fields.Select(f => f.id).ToArray();

            // 2. Build Filter Clause (Simplified for now)
            string filterClause = "";
            var parameters = new Dictionary<string, object>();
            
            if (request.Filters != null && request.Filters.Count > 0)
            {
                var filterParts = new List<string>();
                foreach (var filter in request.Filters)
                {
                    // Basic sanity check on filter key to prevent SQL injection
                    if (!System.Text.RegularExpressions.Regex.IsMatch(filter.Key, @"^[a-zA-Z0-9_]+$")) continue;

                    string paramName = $"@p{parameters.Count}";
                    filterParts.Add($"{filter.Key} = {paramName}");
                    parameters.Add(paramName, filter.Value);
                }
                if (filterParts.Count > 0)
                {
                    filterClause = string.Join(" AND ", filterParts);
                }
            }

            // 3. Fetch Data
            var data = await _db.GetDynamicReportDataAsync(config.BaseTable, fieldNames, filterClause, parameters);

            // 4. Log generation
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                await _db.LogReportGenerationAsync(new ReportGenerationLog
                {
                    ReportId = id,
                    UserId = userId.Value,
                    Format = "Web",
                    FilterValues = JsonSerializer.Serialize(request.Filters)
                });
            }

            return Ok(new
            {
                Title = config.ReportName,
                Headers = fields,
                Data = data,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = HttpContext.Session.GetString("UserName") ?? "System"
            });
        }
    }

    public class ReportGenerationRequest
    {
        public Dictionary<string, object>? Filters { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }

    public class FieldDefinition
    {
        public string id { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string type { get; set; } = "string";
    }
}
