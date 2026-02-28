using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/central/dashboard")]
    public class CentralDashboardController : ControllerBase
    {
        private readonly DbHelper _db;

        public CentralDashboardController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetCentralDashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Central")
                return Unauthorized();

            using var con = _db.GetConnection();
            con.Open();

            var response = new
            {
                stats = new
                {
                    totalUsers = GetTotalActiveUsers(con),
                    totalJobs = GetTotalActiveJobs(con),
                    totalApplications = GetTotalApplications(con),
                    totalPlaced = GetTotalPlaced(con)
                },
                recentApplications = GetRecentApplications(con),
                dailyChart = GetDailyChartData(con)
            };

            return Ok(response);
        }

        private int GetTotalActiveUsers(SqlConnection con)
        {
            return Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE IsActive = 1", con).ExecuteScalar());
        }

        private int GetTotalActiveJobs(SqlConnection con)
        {
            return Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM Jobs WHERE IsActive = 1", con).ExecuteScalar());
        }

        private int GetTotalApplications(SqlConnection con)
        {
            return Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM Applications", con).ExecuteScalar());
        }

        private int GetTotalPlaced(SqlConnection con)
        {
            return Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM Applications WHERE ApplicationStatus = 'Placed'", con).ExecuteScalar());
        }

        // ✅ LIMIT TO 5
        private List<object> GetRecentApplications(SqlConnection con)
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
                ORDER BY A.AppliedDate DESC
            ", con);

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

        // ✅ ENHANCED DAILY CHART DATA
        private List<object> GetDailyChartData(SqlConnection con)
        {
            var dict = new Dictionary<string, dynamic>();

            // 1. Get Applications & Placements
            var cmdApps = new SqlCommand(@"
                SELECT 
                    FORMAT(AppliedDate, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Apps,
                    SUM(CASE WHEN ApplicationStatus = 'Placed' THEN 1 ELSE 0 END) AS Placed
                FROM Applications
                WHERE AppliedDate >= DATEADD(day, -30, GETDATE())
                GROUP BY FORMAT(AppliedDate, 'yyyy-MM-dd')
            ", con);
            
            using (var r = cmdApps.ExecuteReader())
            {
                while (r.Read())
                {
                    var day = r["Day"].ToString();
                    dict[day] = new { day, totalApplications = r["Apps"], totalPlaced = r["Placed"], totalUsers = 0, totalJobs = 0 };
                }
            }

            // 2. Get User Registrations
            var cmdUsers = new SqlCommand(@"
                SELECT 
                    FORMAT(CreatedAt, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Count
                FROM Users
                WHERE CreatedAt >= DATEADD(day, -30, GETDATE())
                GROUP BY FORMAT(CreatedAt, 'yyyy-MM-dd')
            ", con);

            using (var r = cmdUsers.ExecuteReader())
            {
                while (r.Read())
                {
                    var day = r["Day"].ToString();
                    if (dict.ContainsKey(day))
                    {
                        var existing = dict[day];
                        dict[day] = new { day, totalApplications = existing.totalApplications, totalPlaced = existing.totalPlaced, totalUsers = r["Count"], totalJobs = existing.totalJobs };
                    }
                    else
                    {
                        dict[day] = new { day, totalApplications = 0, totalPlaced = 0, totalUsers = r["Count"], totalJobs = 0 };
                    }
                }
            }

            // 3. Get Job Postings
            var cmdJobs = new SqlCommand(@"
                SELECT 
                    FORMAT(PostedDate, 'yyyy-MM-dd') AS Day,
                    COUNT(*) AS Count
                FROM Jobs
                WHERE PostedDate >= DATEADD(day, -30, GETDATE())
                GROUP BY FORMAT(PostedDate, 'yyyy-MM-dd')
            ", con);

            using (var r = cmdJobs.ExecuteReader())
            {
                while (r.Read())
                {
                    var day = r["Day"].ToString();
                    if (dict.ContainsKey(day))
                    {
                        var existing = dict[day];
                        dict[day] = new { day, totalApplications = existing.totalApplications, totalPlaced = existing.totalPlaced, totalUsers = existing.totalUsers, totalJobs = r["Count"] };
                    }
                    else
                    {
                        dict[day] = new { day, totalApplications = 0, totalPlaced = 0, totalUsers = 0, totalJobs = r["Count"] };
                    }
                }
            }

            return dict.Values.OrderBy(x => x.day).Cast<object>().ToList();
        }
    }
}