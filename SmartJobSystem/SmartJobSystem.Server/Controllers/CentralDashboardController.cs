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
                monthlyChart = GetMonthlyChartData(con)
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

        // ✅ MONTHLY CHART DATA
        private List<object> GetMonthlyChartData(SqlConnection con)
        {
            var list = new List<object>();

            var cmd = new SqlCommand(@"
                SELECT 
                    FORMAT(A.AppliedDate, 'yyyy-MM') AS Month,
                    COUNT(*) AS TotalApplications,
                    SUM(CASE WHEN A.ApplicationStatus = 'Placed' THEN 1 ELSE 0 END) AS TotalPlaced
                FROM Applications A
                GROUP BY FORMAT(A.AppliedDate, 'yyyy-MM')
                ORDER BY Month
            ", con);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new
                {
                    month = reader["Month"],
                    totalApplications = reader["TotalApplications"],
                    totalPlaced = reader["TotalPlaced"]
                });
            }

            return list;
        }
    }
}