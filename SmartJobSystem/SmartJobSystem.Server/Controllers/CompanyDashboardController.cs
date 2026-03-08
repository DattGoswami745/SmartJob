using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/company/dashboard")]
    public class CompanyDashboardController : ControllerBase
    {
        private readonly DbHelper _db;

        public CompanyDashboardController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetDashboardData()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");
            int? companyId = HttpContext.Session.GetInt32("CompanyId");

            if (userId == null || role != "Company" || companyId == null)
                return Unauthorized(new { message = "Only company users can access this dashboard." });

            using var con = _db.GetConnection();
            con.Open();

            var response = new
            {
                stats = new
                {
                    totalJobs = GetCompanyJobsCount(con, companyId.Value),
                    totalApplications = GetCompanyApplicationsCount(con, companyId.Value),
                    totalPlaced = GetCompanyPlacementsCount(con, companyId.Value)
                },
                recentApplications = GetRecentCompanyApplications(con, companyId.Value),
                activeJobs = GetActiveCompanyJobs(con, companyId.Value),
                dailyChart = GetCompanyDailyChartData(con, companyId.Value)
            };

            return Ok(response);
        }

        private int GetCompanyJobsCount(SqlConnection con, int companyId)
        {
            return Convert.ToInt32(new SqlCommand(
                $"SELECT COUNT(*) FROM Jobs WHERE CompanyId = {companyId} AND IsActive = 1", con).ExecuteScalar());
        }

        private int GetCompanyApplicationsCount(SqlConnection con, int companyId)
        {
            return Convert.ToInt32(new SqlCommand(@"
                SELECT COUNT(*) 
                FROM Applications A
                INNER JOIN Jobs J ON A.JobId = J.JobId
                WHERE J.CompanyId = @CompanyId
            ", con) { Parameters = { new SqlParameter("@CompanyId", companyId) } }.ExecuteScalar());
        }

        private int GetCompanyPlacementsCount(SqlConnection con, int companyId)
        {
            return Convert.ToInt32(new SqlCommand(@"
                SELECT COUNT(*) 
                FROM Applications A
                INNER JOIN Jobs J ON A.JobId = J.JobId
                WHERE J.CompanyId = @CompanyId AND A.ApplicationStatus = 'Placed'
            ", con) { Parameters = { new SqlParameter("@CompanyId", companyId) } }.ExecuteScalar());
        }

        private List<object> GetRecentCompanyApplications(SqlConnection con, int companyId)
        {
            var list = new List<object>();

            var cmd = new SqlCommand(@"
                SELECT TOP 5
                    A.ApplicationId,
                    U.FullName,
                    U.Email,
                    J.Title,
                    A.ApplicationStatus,
                    A.AppliedDate
                FROM Applications A
                INNER JOIN Users U ON A.UserId = U.UserId
                INNER JOIN Jobs J ON A.JobId = J.JobId
                WHERE J.CompanyId = @CompanyId
                ORDER BY A.AppliedDate DESC
            ", con);
            cmd.Parameters.AddWithValue("@CompanyId", companyId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new
                {
                    applicationId = reader["ApplicationId"],
                    userName = reader["FullName"],
                    email = reader["Email"],
                    jobTitle = reader["Title"],
                    status = reader["ApplicationStatus"],
                    appliedDate = reader["AppliedDate"]
                });
            }

            return list;
        }

        private List<object> GetCompanyDailyChartData(SqlConnection con, int companyId)
        {
            var dict = new Dictionary<string, dynamic>();
            var startDate = DateTime.Today.AddDays(-30);

            // Initialize dictionary with all 30 days to ensure continuous chart
            for (int i = 0; i <= 30; i++)
            {
                var day = startDate.AddDays(i).ToString("yyyy-MM-dd");
                dict[day] = new { day, totalApplications = 0, totalPlaced = 0, totalJobs = 0 };
            }

            // Applications & Placements for the company
            var cmdApps = new SqlCommand(@"
                SELECT 
                    FORMAT(AppliedDate, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Apps,
                    SUM(CASE WHEN ApplicationStatus = 'Placed' THEN 1 ELSE 0 END) AS Placed
                FROM Applications A
                INNER JOIN Jobs J ON A.JobId = J.JobId
                WHERE J.CompanyId = @CompanyId AND A.AppliedDate >= @StartDate
                GROUP BY FORMAT(AppliedDate, 'yyyy-MM-dd')
            ", con);
            cmdApps.Parameters.AddWithValue("@CompanyId", companyId);
            cmdApps.Parameters.AddWithValue("@StartDate", startDate);
            
            using (var r = cmdApps.ExecuteReader())
            {
                while (r.Read())
                {
                    var day = r["Day"].ToString();
                    if (dict.ContainsKey(day))
                    {
                        var existing = dict[day];
                        dict[day] = new { day, totalApplications = r["Apps"], totalPlaced = r["Placed"], totalJobs = existing.totalJobs };
                    }
                }
            }

            // Calculate Initial Active Jobs count (before the 30-day window)
            var cmdInitialJobs = new SqlCommand(@"
                SELECT COUNT(*) FROM Jobs 
                WHERE CompanyId = @CompanyId AND IsActive = 1 AND PostedDate < @StartDate
            ", con);
            cmdInitialJobs.Parameters.AddWithValue("@CompanyId", companyId);
            cmdInitialJobs.Parameters.AddWithValue("@StartDate", startDate);
            int runningJobsTotal = Convert.ToInt32(cmdInitialJobs.ExecuteScalar());

            // Get Job Postings within the window
            var cmdJobs = new SqlCommand(@"
                SELECT 
                    FORMAT(PostedDate, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Count
                FROM Jobs
                WHERE CompanyId = @CompanyId AND PostedDate >= @StartDate AND IsActive = 1
                GROUP BY FORMAT(PostedDate, 'yyyy-MM-dd')
                ORDER BY Day
            ", con);
            cmdJobs.Parameters.AddWithValue("@CompanyId", companyId);
            cmdJobs.Parameters.AddWithValue("@StartDate", startDate);

            var dailyNewJobs = new Dictionary<string, int>();
            using (var r = cmdJobs.ExecuteReader())
            {
                while (r.Read())
                {
                    dailyNewJobs[r["Day"].ToString()] = Convert.ToInt32(r["Count"]);
                }
            }

            // Add cumulative total to each day
            var sortedDays = dict.Keys.OrderBy(x => x).ToList();
            var finalData = new List<object>();

            foreach (var day in sortedDays)
            {
                if (dailyNewJobs.ContainsKey(day))
                {
                    runningJobsTotal += dailyNewJobs[day];
                }
                
                var existing = dict[day];
                finalData.Add(new { 
                    day = existing.day, 
                    totalApplications = existing.totalApplications, 
                    totalPlaced = existing.totalPlaced, 
                    totalJobs = runningJobsTotal 
                });
            }

            return finalData;
        }

        private List<object> GetActiveCompanyJobs(SqlConnection con, int companyId)
        {
            var list = new List<object>();
            var cmd = new SqlCommand(@"
                SELECT TOP 5
                    JobId,
                    Title,
                    JobType,
                    SalaryRange,
                    PostedDate,
                    LastDate
                FROM Jobs
                WHERE CompanyId = @CompanyId AND IsActive = 1
                ORDER BY PostedDate DESC
            ", con);
            cmd.Parameters.AddWithValue("@CompanyId", companyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new
                {
                    jobId = reader["JobId"],
                    title = reader["Title"],
                    jobType = reader["JobType"],
                    salaryRange = reader["SalaryRange"],
                    postedDate = reader["PostedDate"],
                    lastDate = reader["LastDate"] == DBNull.Value ? null : reader["LastDate"]
                });
            }
            return list;
        }
    }
}
