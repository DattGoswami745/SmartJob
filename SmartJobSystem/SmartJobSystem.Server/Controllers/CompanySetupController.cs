using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;
using System.Data;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/company/setup")]
    public class CompanySetupController : ControllerBase
    {
        private readonly DbHelper _db;

        public CompanySetupController(DbHelper db)
        {
            _db = db;
        }

        // ================= GET ALL EXISTING COMPANIES =================
        [HttpGet("list")]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _db.GetCompaniesAsync();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ================= SETUP COMPANY (CREATE OR JOIN) =================
        [HttpPost]
        public async Task<IActionResult> SetupCompany([FromBody] CompanySetupRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Company")
                return Unauthorized(new { message = "Only company users can perform company setup." });

            if (string.IsNullOrWhiteSpace(request.Action))
                return BadRequest(new { message = "Action is required (create or join)." });

            using var con = _db.GetConnection();
            await con.OpenAsync();

            try
            {
                int companyId;

                if (request.Action == "create")
                {
                    if (string.IsNullOrWhiteSpace(request.CompanyName))
                        return BadRequest(new { message = "Company name is required for registration." });

                    // 1. Create Company
                    var cmdCreate = new SqlCommand(@"
                        INSERT INTO Companies (CompanyName, Industry, Location, CreatedAt)
                        OUTPUT INSERTED.CompanyId
                        VALUES (@Name, @Ind, @Loc, GETDATE())
                    ", con);
                    cmdCreate.Parameters.AddWithValue("@Name", request.CompanyName);
                    cmdCreate.Parameters.AddWithValue("@Ind", request.Industry ?? "");
                    cmdCreate.Parameters.AddWithValue("@Loc", request.Location ?? "");
                    
                    companyId = (int)await cmdCreate.ExecuteScalarAsync();
                }
                else if (request.Action == "join")
                {
                    if (request.CompanyId == null || request.CompanyId <= 0)
                        return BadRequest(new { message = "Valid Company ID is required to join." });

                    companyId = request.CompanyId.Value;
                }
                else
                {
                    return BadRequest(new { message = "Invalid action." });
                }

                // 2. Link User to Company
                var cmdLink = new SqlCommand("UPDATE Users SET CompanyId = @CID WHERE UserId = @UID", con);
                cmdLink.Parameters.AddWithValue("@CID", companyId);
                cmdLink.Parameters.AddWithValue("@UID", userId);
                await cmdLink.ExecuteNonQueryAsync();

                // 3. Update Session
                HttpContext.Session.SetInt32("CompanyId", companyId);

                return Ok(new { message = "Company setup successful!", companyId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Setup failed: {ex.Message}" });
            }
        }
    }

    public class CompanySetupRequest
    {
        public string? Action { get; set; } // "create" or "join"
        public string? CompanyName { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public int? CompanyId { get; set; }
    }
}
