using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly DbHelper _db;

    public ApplicationsController(DbHelper db)
    {
        _db = db;
    }

    // 📝 APPLY JOB
    [HttpPost]
    public IActionResult Apply([FromBody] ApplyDto dto)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        using var con = _db.GetConnection();
        con.Open();

        // Prevent duplicate apply
        var check = new SqlCommand(
            "SELECT COUNT(*) FROM Applications WHERE JobId=@jid AND UserId=@uid",
            con
        );
        check.Parameters.AddWithValue("@jid", dto.JobId);
        check.Parameters.AddWithValue("@uid", userId.Value);

        if ((int)check.ExecuteScalar() > 0)
            return BadRequest(new { message = "Already applied" });

        var cmd = new SqlCommand(@"
            INSERT INTO Applications (JobId, UserId, ApplicationStatus, AppliedDate)
            VALUES (@jid, @uid, 'Pending', GETDATE())
        ", con);

        cmd.Parameters.AddWithValue("@jid", dto.JobId);
        cmd.Parameters.AddWithValue("@uid", userId.Value);
        cmd.ExecuteNonQuery();

        return Ok(new { message = "Application submitted" });
    }

    // 📄 GET MY APPLICATIONS
    [HttpGet]
    public IActionResult GetMyApplications()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var list = new List<object>();

        using var con = _db.GetConnection();
        con.Open();

        var cmd = new SqlCommand(@"
            SELECT 
                a.ApplicationId,
                a.ApplicationStatus,
                j.Title,
                j.LastDate,
                c.CompanyName
            FROM Applications a
            JOIN Jobs j ON a.JobId = j.JobId
            JOIN Companies c ON j.CompanyId = c.CompanyId
            WHERE a.UserId = @uid
            ORDER BY a.AppliedDate DESC
        ", con);

        cmd.Parameters.AddWithValue("@uid", userId.Value);

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new
            {
                applicationId = reader["ApplicationId"],
                title = reader["Title"].ToString(),
                lastDate = reader["LastDate"] == DBNull.Value ? null : reader["LastDate"],
                company = reader["CompanyName"].ToString(),
                status = reader["ApplicationStatus"].ToString()
            });
        }

        return Ok(list);
    }
}

public class ApplyDto
{
    public int JobId { get; set; }
}
