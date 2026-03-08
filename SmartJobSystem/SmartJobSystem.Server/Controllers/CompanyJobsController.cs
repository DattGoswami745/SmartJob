using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Models;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/company/jobs")]
    [ApiController]
    public class CompanyJobsController : ControllerBase
    {
        private readonly DbHelper _db;

        public CompanyJobsController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can access this.");

            var jobs = await _db.GetJobsByCompanyAsync(companyId.Value);
            return Ok(jobs);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddJob([FromBody] Job job)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can add jobs.");

            if (job == null)
                return BadRequest("Invalid job data");

            if (string.IsNullOrWhiteSpace(job.Title))
                return BadRequest("Job Title is required");

            // Override companyId from session for security
            job.CompanyId = companyId.Value;
            job.IsActive = true;
            job.IsApproved = false; // Requires central approval

            var newId = await _db.AddJobAsync(job);

            return Ok(new
            {
                message = "Job Created Successfully",
                jobId = newId
            });
        }

        [HttpPut("update/{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, [FromBody] Job job)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can update jobs.");

            if (job == null)
                return BadRequest("Invalid job data");

            // Verify the job belongs to this company first
            var existingJobs = await _db.GetJobsByCompanyAsync(companyId.Value);
            if (!existingJobs.Any(j => j.JobId == jobId))
            {
                return NotFound("Job not found or does not belong to your company.");
            }

            job.CompanyId = companyId.Value;
            job.IsApproved = false; // Reset approval on update
            bool updated = await _db.UpdateJobAsync(jobId, job);

            if (!updated)
                return BadRequest("Could not update job.");

            return Ok(new { message = "Job Updated Successfully" });
        }
    }
}
