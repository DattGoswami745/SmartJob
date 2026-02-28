using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DbHelper _db;

    public DashboardController(DbHelper db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetDashboard()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        using var con = _db.GetConnection();
        con.Open();

        // 1️⃣ Get user profile details
        var profileCmd = new SqlCommand(
            "SELECT Skills, Education, PreferredLocation FROM UserProfiles WHERE UserId=@uid",
            con);

        profileCmd.Parameters.AddWithValue("@uid", userId.Value);
        
        using var profileReader = profileCmd.ExecuteReader();
        string skills = "";
        string education = "";
        string location = "";
        bool profileExists = profileReader.Read();

        if (profileExists)
        {
            skills = profileReader["Skills"]?.ToString() ?? "";
            education = profileReader["Education"]?.ToString() ?? "";
            location = profileReader["PreferredLocation"]?.ToString() ?? "";
        }
        profileReader.Close();

        bool isProfileComplete = !string.IsNullOrWhiteSpace(skills) && 
                                 !string.IsNullOrWhiteSpace(education) && 
                                 !string.IsNullOrWhiteSpace(location);

        if (string.IsNullOrWhiteSpace(skills))
        {
            return Ok(new
            {
                totalJobs = GetTotalJobs(con),
                appliedJobs = GetAppliedJobs(con, userId.Value),
                skillMatch = 0,
                isProfileComplete = isProfileComplete
            });
        }

        var userSkills = skills
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLower())
            .ToList();

        // 2️⃣ Get Total & Applied Jobs
        int totalJobs = GetTotalJobs(con);
        int appliedJobs = GetAppliedJobs(con, userId.Value);

        // 3️⃣ Skill Matching Logic
        var jobCmd = new SqlCommand(
            "SELECT RequiredSkills FROM Jobs WHERE IsActive=1",
            con);

        var jobReader = jobCmd.ExecuteReader();

        int totalRequiredSkills = 0;
        int totalMatchedSkills = 0;

        while (jobReader.Read())
        {
            if (jobReader["RequiredSkills"] == DBNull.Value)
                continue;

            var requiredSkills = jobReader["RequiredSkills"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();

            totalRequiredSkills += requiredSkills.Count;

            totalMatchedSkills += requiredSkills
                .Count(skill => userSkills.Contains(skill));
        }

        jobReader.Close();

        double finalMatch = totalRequiredSkills == 0
            ? 0
            : ((double)totalMatchedSkills / totalRequiredSkills) * 100;

        return Ok(new
        {
            totalJobs = totalJobs,
            appliedJobs = appliedJobs,
            skillMatch = Math.Round(finalMatch, 2),
            isProfileComplete = isProfileComplete
        });
    }

    private int GetTotalJobs(SqlConnection con)
    {
        var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM Jobs WHERE IsActive=1", con);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private int GetAppliedJobs(SqlConnection con, int userId)
    {
        var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM Applications WHERE UserId=@uid", con);

        cmd.Parameters.AddWithValue("@uid", userId);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

}
