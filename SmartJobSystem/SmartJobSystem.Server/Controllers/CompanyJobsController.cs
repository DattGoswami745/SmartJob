using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Models;
using Microsoft.AspNetCore.Http;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/company/jobs")]
    [ApiController]
    public class CompanyJobsController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IWebHostEnvironment _env;

        public CompanyJobsController(DbHelper db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
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
        public async Task<IActionResult> AddJob([FromForm] Job job, IFormFile? descriptionFile)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can add jobs.");

            // 🔍 Verification Check
            if (!await _db.IsCompanyVerifiedAsync(companyId.Value))
            {
                return BadRequest("Company verification pending. Please upload required documents and wait for admin approval.");
            }

            if (job == null)
                return BadRequest("Invalid job data");

            if (string.IsNullOrWhiteSpace(job.Title))
                return BadRequest("Job Title is required");

            // Handle File Upload
            if (descriptionFile != null && descriptionFile.Length > 0)
            {
                var ext = Path.GetExtension(descriptionFile.FileName).ToLower();
                if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
                    return BadRequest("Invalid file type. Only PDF and Word documents are allowed.");

                if (descriptionFile.Length > 5 * 1024 * 1024)
                    return BadRequest("File size exceeds 5MB limit.");

                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "job_descriptions");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{descriptionFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await descriptionFile.CopyToAsync(fileStream);
                }

                job.JobDescriptionFile = $"/uploads/job_descriptions/{uniqueFileName}";
            }

            if (!string.IsNullOrEmpty(job.JobDescriptionText) || !string.IsNullOrEmpty(job.JobDescriptionFile))
            {
                job.JobDescriptionUpdatedAt = DateTime.UtcNow;
            }

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
        public async Task<IActionResult> UpdateJob(int jobId, [FromForm] Job job, IFormFile? descriptionFile)
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Company" || companyId == null)
                return Unauthorized("Only company recruiters can update jobs.");

            if (job == null)
                return BadRequest("Invalid job data");

            // Verify the job belongs to this company first
            var existingJobs = await _db.GetJobsByCompanyAsync(companyId.Value);
            var existing = existingJobs.FirstOrDefault(j => j.JobId == jobId);
            if (existing == null)
            {
                return NotFound("Job not found or does not belong to your company.");
            }

            // Handle File Upload
            if (descriptionFile != null && descriptionFile.Length > 0)
            {
                var ext = Path.GetExtension(descriptionFile.FileName).ToLower();
                if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
                    return BadRequest("Invalid file type. Only PDF and Word documents are allowed.");

                if (descriptionFile.Length > 5 * 1024 * 1024)
                    return BadRequest("File size exceeds 5MB limit.");

                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "job_descriptions");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{descriptionFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await descriptionFile.CopyToAsync(fileStream);
                }

                job.JobDescriptionFile = $"/uploads/job_descriptions/{uniqueFileName}";
            }
            else
            {
                // Preserve existing file if no new one is uploaded
                job.JobDescriptionFile = existing.JobDescriptionFile;
            }

            if (!string.IsNullOrEmpty(job.JobDescriptionText) || !string.IsNullOrEmpty(job.JobDescriptionFile))
            {
                job.JobDescriptionUpdatedAt = DateTime.UtcNow;
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
