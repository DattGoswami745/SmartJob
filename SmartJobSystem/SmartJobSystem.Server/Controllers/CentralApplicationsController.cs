using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using System.Threading.Tasks;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/applications")]
    [ApiController]
    public class CentralApplicationsController : ControllerBase
    {
        private readonly DbHelper _db;

        public CentralApplicationsController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            var apps = await _db.GetAllApplicationsAsync();
            return Ok(apps);
        }

        [HttpDelete("{appId}")]
        public async Task<IActionResult> DeleteApplication(int appId)
        {
            bool deleted = await _db.DeleteApplicationAsync(appId);
            if (!deleted)
                return NotFound(new { message = "Application not found or already deleted." });

            return Ok(new { message = "Application deleted successfully." });
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfileForAdmin(int userId)
        {
            var profile = await _db.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFound(new { message = "Profile not found." });

            return Ok(profile);
        }
    }
}
