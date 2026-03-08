using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Models;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/jobs")]
    [ApiController]
    public class CentralJobsController : ControllerBase
    {
        private readonly DbHelper _db;

        public CentralJobsController(DbHelper db)
        {
            _db = db;
        }

        // ============================================
        // GET ALL JOBS (THIS FIXES YOUR 404 ERROR)
        // ============================================
        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] string status = "All")
        {
            var jobs = await _db.GetJobsAsync(status);
            return Ok(jobs);
        }



        [HttpPost("approve/{jobId}")]
        public async Task<IActionResult> ApproveJob(int jobId)
        {
            bool approved = await _db.ApproveJobAsync(jobId);
            if (!approved) return BadRequest("Approval Failed");
            return Ok(new { message = "Job Approved Successfully" });
        }

        [HttpPost("reject/{jobId}")]
        public async Task<IActionResult> RejectJob(int jobId)
        {
            bool rejected = await _db.RejectJobAsync(jobId);
            if (!rejected) return BadRequest("Rejection Failed");
            return Ok(new { message = "Job Rejected Successfully" });
        }

        [HttpPost("restore/{jobId}")]
        public async Task<IActionResult> RestoreJob(int jobId)
        {
            bool restored = await _db.RestoreJobAsync(jobId);
            if (!restored) return BadRequest("Restoration Failed");
            return Ok(new { message = "Job Restored Successfully" });
        }
    }
}