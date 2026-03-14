using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminVerificationController : ControllerBase
    {
        private readonly DbHelper _db;

        public AdminVerificationController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet("company-documents/{companyId}")]
        public async Task<IActionResult> GetDocuments(int companyId)
        {
            var role = HttpContext.Session.GetString("Role");
            var userCompanyId = HttpContext.Session.GetInt32("CompanyId");

            // Allow Central Admins OR the Company themselves to see these docs
            bool isCentral = role == "Central";
            bool isOwnCompany = (role == "Company" && userCompanyId == companyId);

            if (!isCentral && !isOwnCompany) 
                return StatusCode(403, new { message = "You do not have permission to view these documents." });

            var docs = await _db.GetCompanyDocumentsAsync(companyId);
            return Ok(docs);
        }

        [HttpPost("verify-company")]
        public async Task<IActionResult> VerifyCompany([FromBody] VerifyActionDto dto)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Central") 
                return StatusCode(403, new { message = "Only central admins can verify companies." });

            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _db.VerifyCompanyAsync(dto.CompanyId, dto.IsApproved, dto.Reason, adminId);

            return Ok(new { message = dto.IsApproved ? "Company verified successfully" : "Company verification rejected" });
        }

        public class VerifyActionDto
        {
            public int CompanyId { get; set; }
            public bool IsApproved { get; set; }
            public string Reason { get; set; }
        }
    }
}
