using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Models;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/companies")]
    [ApiController]
    public class CentralCompaniesController : ControllerBase
    {
        private readonly DbHelper _db;

        public CentralCompaniesController(DbHelper db)
        {
            _db = db;
        }

        /* ===================== GET ALL COMPANIES ===================== */

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _db.GetCompaniesAsync();
            return Ok(companies);
        }

        /* ===================== ADD COMPANY ===================== */

        [HttpPost]
        public async Task<IActionResult> AddCompany([FromBody] Company company)
        {
            // Extra safety check
            if (company == null)
                return BadRequest("Invalid company data.");

            if (string.IsNullOrWhiteSpace(company.CompanyName))
                return BadRequest("Company name is required.");

            // Ensure optional fields never break DB
            company.Industry ??= string.Empty;
            company.Location ??= string.Empty;

            var id = await _db.AddCompanyAsync(company);

            return Ok(new
            {
                companyId = id,
                companyName = company.CompanyName,
                industry = company.Industry,
                location = company.Location
            });
        }
    }
}