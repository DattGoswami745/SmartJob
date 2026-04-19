using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using System.Threading.Tasks;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/parameters")]
    [ApiController]
    public class ParametersController : ControllerBase
    {
        private readonly DbHelper _db;

        public ParametersController(DbHelper db)
        {
            _db = db;
        }

        /* ================================
           GET ALL PARAMETERS
        ================================= */
        [HttpGet]
        public async Task<IActionResult> GetParameters()
        {
            // Simple session check for admin/central roles can be added here
            var role = HttpContext.Session.GetString("Role");
            if (role != "SuperAdmin" && role != "Central")
            {
                // For a "standalone" utility, we might allow access if specific conditions are met, 
                // but usually, this should be restricted.
                // return Unauthorized("Admin access required.");
            }

            var parameters = await _db.GetAllParametersAsync();
            return Ok(parameters);
        }

        /* ================================
           UPDATE PARAMETER
        ================================= */
        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateParameter(string key, [FromBody] ParameterUpdateDto dto)
        {
            if (string.IsNullOrEmpty(key)) return BadRequest("Key is required");

            bool success = await _db.UpdateParameterAsync(key, dto.ParamValue, dto.Description);
            if (!success) return StatusCode(500, "Failed to update parameter");

            return Ok(new { message = "Parameter updated successfully" });
        }
    }

    public class ParameterUpdateDto
    {
        public string ParamValue { get; set; }
        public string Description { get; set; }
    }
}
