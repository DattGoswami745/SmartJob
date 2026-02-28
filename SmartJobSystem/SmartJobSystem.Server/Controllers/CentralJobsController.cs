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
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _db.GetJobsAsync();
            return Ok(jobs);
        }

        // ============================================
        // ADD JOB
        // ============================================
        [HttpPost("add")]
        public async Task<IActionResult> AddJob([FromBody] Job job)
        {
            if (job == null)
                return BadRequest("Invalid job data");

            if (string.IsNullOrWhiteSpace(job.Title))
                return BadRequest("Job Title is required");

            if (job.CompanyId <= 0)
                return BadRequest("CompanyId is required");

            var newId = await _db.AddJobAsync(job);

            return Ok(new
            {
                message = "Job Created Successfully",
                jobId = newId
            });
        }

        // ============================================
        // UPDATE JOB
        // ============================================
        [HttpPut("update/{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, [FromBody] Job job)
        {
            if (job == null)
                return BadRequest("Invalid job data");

            if (string.IsNullOrWhiteSpace(job.Title))
                return BadRequest("Job Title is required");

            if (job.CompanyId <= 0)
                return BadRequest("CompanyId is required");

            bool updated = await _db.UpdateJobAsync(jobId, job);

            if (!updated)
                return NotFound("Job not found or could not be updated");

            return Ok(new
            {
                message = "Job Updated Successfully"
            });
        }
    }
}