using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly DbHelper _db;

        public JobsController(DbHelper db)
        {
            _db = db;
        }

        // ✅ JOB LIST WITH COMPANY INFO + APPLIED STATUS
        [HttpGet]
        public IActionResult GetJobs()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var jobs = new List<object>();

            using var con = _db.GetConnection();
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT 
                    j.JobId,
                    j.Title,
                    j.Description,
                    j.RequiredSkills,
                    j.JobType,
                    j.SalaryRange,
                    j.PostedDate,
                    j.LastDate,

                    c.CompanyName,
                    c.Industry,
                    c.Location,

                    CASE 
                        WHEN a.ApplicationId IS NULL THEN 0 
                        ELSE 1 
                    END AS Applied
                FROM Jobs j
                INNER JOIN Companies c 
                    ON j.CompanyId = c.CompanyId
                LEFT JOIN Applications a 
                    ON j.JobId = a.JobId AND a.UserId = @uid
                WHERE j.IsActive = 1
                ORDER BY j.PostedDate DESC
            ", con);

            cmd.Parameters.AddWithValue("@uid", userId.Value);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                jobs.Add(new
                {
                    jobId = reader["JobId"],
                    title = reader["Title"].ToString(),
                    description = reader["Description"].ToString(),
                    requiredSkills = reader["RequiredSkills"].ToString(),
                    jobType = reader["JobType"].ToString(),
                    salaryRange = reader["SalaryRange"].ToString(),
                    postedDate = reader["PostedDate"],
                    lastDate = reader["LastDate"] == DBNull.Value ? null : reader["LastDate"],

                    companyName = reader["CompanyName"].ToString(),
                    industry = reader["Industry"].ToString(),
                    location = reader["Location"].ToString(),

                    applied = Convert.ToInt32(reader["Applied"]) == 1
                });
            }

            return Ok(jobs);
        }
    }
}
