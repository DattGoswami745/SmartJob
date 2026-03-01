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

        // --- LOG ACTIVITY ---
        var logCmd = new SqlCommand(@"
            INSERT INTO dbo.UserActivityLogs (UserId, Action, ActionDate)
            VALUES (@UId, CONCAT('Applied for Job ID: ', @JId), GETDATE())
        ", con);
        logCmd.Parameters.AddWithValue("@UId", userId.Value);
        logCmd.Parameters.AddWithValue("@JId", dto.JobId);
        logCmd.ExecuteNonQuery();

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

    // 🎉 MARK ASP PLACED
    [HttpPost("mark-placed")]
    public IActionResult MarkPlaced([FromBody] ApplyDto dto)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        using var con = _db.GetConnection();
        con.Open();

        using var transaction = con.BeginTransaction();

        try
        {
            // 1. Delete all other applications for this user
            var deleteCmd = new SqlCommand(
                "DELETE FROM Applications WHERE UserId = @uid AND JobId != @jid",
                con, transaction
            );
            deleteCmd.Parameters.AddWithValue("@uid", userId.Value);
            deleteCmd.Parameters.AddWithValue("@jid", dto.JobId);
            deleteCmd.ExecuteNonQuery();

            // 2. Check if the specific application exists
            var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Applications WHERE UserId = @uid AND JobId = @jid",
                con, transaction
            );
            checkCmd.Parameters.AddWithValue("@uid", userId.Value);
            checkCmd.Parameters.AddWithValue("@jid", dto.JobId);
            
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists > 0)
            {
                // Update existing application
                var updateCmd = new SqlCommand(
                    "UPDATE Applications SET ApplicationStatus = 'Placed' WHERE UserId = @uid AND JobId = @jid",
                    con, transaction
                );
                updateCmd.Parameters.AddWithValue("@uid", userId.Value);
                updateCmd.Parameters.AddWithValue("@jid", dto.JobId);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                // Insert new application
                var insertCmd = new SqlCommand(@"
                    INSERT INTO Applications (JobId, UserId, ApplicationStatus, AppliedDate)
                    VALUES (@jid, @uid, 'Placed', GETDATE())
                ", con, transaction);
                insertCmd.Parameters.AddWithValue("@uid", userId.Value);
                insertCmd.Parameters.AddWithValue("@jid", dto.JobId);
                insertCmd.ExecuteNonQuery();
            }

            // 3. Log Activity
            var logCmd = new SqlCommand(@"
                INSERT INTO dbo.UserActivityLogs (UserId, Action, ActionDate)
                VALUES (@uid, CONCAT('Marked as Placed in Job ID: ', @jid), GETDATE())
            ", con, transaction);
            logCmd.Parameters.AddWithValue("@uid", userId.Value);
            logCmd.Parameters.AddWithValue("@jid", dto.JobId);
            logCmd.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Marked as placed successfully!" });
        }
        catch (Exception)
        {
            transaction.Rollback();
            return StatusCode(500, new { message = "An error occurred while marking as placed." });
        }
    }
}

public class ApplyDto
{
    public int JobId { get; set; }
}
