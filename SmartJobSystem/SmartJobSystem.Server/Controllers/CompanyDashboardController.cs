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

            // Applications & Placements for the company
            var cmdApps = new SqlCommand(@"
                SELECT 
                    FORMAT(AppliedDate, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Apps,
                    SUM(CASE WHEN ApplicationStatus = 'Placed' THEN 1 ELSE 0 END) AS Placed
                FROM Applications A
                INNER JOIN Jobs J ON A.JobId = J.JobId
                WHERE J.CompanyId = @CompanyId AND A.AppliedDate >= DATEADD(day, -30, GETDATE())
                GROUP BY FORMAT(AppliedDate, 'yyyy-MM-dd')
            ", con);
            cmdApps.Parameters.AddWithValue("@CompanyId", companyId);
            
            using (var r = cmdApps.ExecuteReader())
            {
                while (r.Read())
                {
                    var day = r["Day"].ToString();
                    dict[day] = new { day, totalApplications = r["Apps"], totalPlaced = r["Placed"], totalJobs = 0 };
                }
            }

            // Job Postings for the company
            var cmdJobs = new SqlCommand(@"
                SELECT 
                    FORMAT(PostedDate, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Count
                FROM Jobs
                WHERE CompanyId = @CompanyId AND PostedDate >= DATEADD(day, -30, GETDATE())
                GROUP BY FORMAT(PostedDate, 'yyyy-MM-dd')
            ", con);
            cmdJobs.Parameters.AddWithValue("@CompanyId", companyId);

            using (var r = cmdJobs.ExecuteReader())
            {
                while (r.Read())
                {
                    var day = r["Day"].ToString();
                    if (dict.ContainsKey(day))
                    {
                        var existing = dict[day];
                        dict[day] = new { day, totalApplications = existing.totalApplications, totalPlaced = existing.totalPlaced, totalJobs = r["Count"] };
                    }
                    else
                    {
                        dict[day] = new { day, totalApplications = 0, totalPlaced = 0, totalJobs = r["Count"] };
                    }
                }
            }

            return dict.Values.OrderBy(x => x.day).Cast<object>().ToList();
        }
    }
}
