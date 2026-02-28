using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using System.Threading.Tasks;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/users")]
    [ApiController]
    public class CentralUsersController : ControllerBase
    {
        private readonly DbHelper _db;

        public CentralUsersController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            bool deleted = await _db.DeleteUserAsync(userId);
            if (!deleted)
                return NotFound(new { message = "User not found or already deleted." });

            return Ok(new { message = "User deleted successfully." });
        }

        [HttpGet("{userId}/activity")]
        public async Task<IActionResult> GetUserActivityLogs(int userId)
        {
            var logs = await _db.GetUserActivityLogsAsync(userId);
            return Ok(logs);
        }
    }
}
