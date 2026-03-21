using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Models;
using System.Data;

namespace SmartJobSystem.Server.Data
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        /* ===================== CONNECTION ===================== */

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /* ===================== JOBS ===================== */

        // 🔹 Get all active jobs
        // 🔹 Get jobs by status (Pending, Approved, Rejected)
        public async Task<List<Job>> GetJobsAsync(string status = "All")
        {
            var jobs = new List<Job>();
            string filter = "WHERE 1=1";

            if (status == "Pending")
                filter = "WHERE j.IsApproved = 0 AND j.IsActive = 1";
            else if (status == "Approved")
                filter = "WHERE j.IsApproved = 1 AND j.IsActive = 1";
            else if (status == "Rejected")
                filter = "WHERE j.IsActive = 0";

            using var con = GetConnection();
            using var cmd = new SqlCommand($@"
        SELECT 
            j.JobId,
            j.CompanyId,
            j.Title,
            j.Description,
            j.RequiredSkills,
            j.JobType,
            j.SalaryRange,
            j.PostedDate,
            j.LastDate,
            j.IsActive,
            j.IsApproved,
            j.JobDescriptionFile,
            j.JobDescriptionText,
            j.JobDescriptionUpdatedAt,
            c.CompanyName
        FROM Jobs j
        LEFT JOIN Companies c ON j.CompanyId = c.CompanyId
        {filter}
        ORDER BY j.PostedDate DESC
    ", con);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                jobs.Add(new Job
                {
                    JobId = reader.GetInt32(0),
                    CompanyId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    RequiredSkills = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    JobType = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    SalaryRange = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    PostedDate = reader.GetDateTime(7),
                    LastDate = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                    IsActive = reader.GetBoolean(9),
                    IsApproved = reader.GetBoolean(10),
                    JobDescriptionFile = reader.IsDBNull(11) ? null : reader.GetString(11),
                    JobDescriptionText = reader.IsDBNull(12) ? null : reader.GetString(12),
                    JobDescriptionUpdatedAt = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
                    CompanyName = reader.IsDBNull(14) ? null : reader.GetString(14)
                });
            }

            return jobs;
        }

        // 🔹 Get jobs by company
        public async Task<List<Job>> GetJobsByCompanyAsync(int companyId)
        {
            var jobs = new List<Job>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT 
                    JobId,
                    CompanyId,
                    Title,
                    Description,
                    RequiredSkills,
                    JobType,
                    SalaryRange,
                    PostedDate,
                    LastDate,
                    IsActive,
                    IsApproved,
                    JobDescriptionFile,
                    JobDescriptionText,
                    JobDescriptionUpdatedAt
                FROM Jobs
                WHERE CompanyId = @CompanyId AND IsActive = 1
                ORDER BY PostedDate DESC
            ", con);

            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                jobs.Add(new Job
                {
                    JobId = reader.GetInt32(0),
                    CompanyId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    RequiredSkills = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    JobType = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    SalaryRange = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    PostedDate = reader.GetDateTime(7),
                    LastDate = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                    IsActive = reader.GetBoolean(9),
                    IsApproved = reader.GetBoolean(10),
                    JobDescriptionFile = reader.IsDBNull(11) ? null : reader.GetString(11),
                    JobDescriptionText = reader.IsDBNull(12) ? null : reader.GetString(12),
                    JobDescriptionUpdatedAt = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13)
                });
            }

            return jobs;
        }

        // 🔹 Approve a job
        public async Task<bool> ApproveJobAsync(int jobId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("UPDATE Jobs SET IsApproved = 1 WHERE JobId = @JobId", con);
            cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // 🔹 Reject (Soft Delete) a job
        public async Task<bool> RejectJobAsync(int jobId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("UPDATE Jobs SET IsActive = 0 WHERE JobId = @JobId", con);
            cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // 🔹 Restore a rejected job
        public async Task<bool> RestoreJobAsync(int jobId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("UPDATE Jobs SET IsActive = 1 WHERE JobId = @JobId", con);
            cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // 🔹 Count active jobs
        public async Task<int> GetActiveJobsCountAsync()
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Jobs WHERE IsActive = 1",
                con
            );

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /* ===================== APPLICATIONS ===================== */

        // 🔹 Apply for Job (prevents duplicate)
        public async Task<bool> ApplyForJobAsync(int userId, int jobId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (
                    SELECT 1 FROM UserApplications 
                    WHERE UserId = @UserId AND JobId = @JobId
                )
                BEGIN
                    INSERT INTO UserApplications (UserId, JobId)
                    VALUES (@UserId, @JobId)
                END
            ", con);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return true;
        }

        // 🔹 Get applied job IDs
        public async Task<List<int>> GetAppliedJobIdsAsync(int userId)
        {
            var jobIds = new List<int>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(
                "SELECT JobId FROM UserApplications WHERE UserId = @UserId",
                con
            );

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                jobIds.Add(reader.GetInt32(0));
            }

            return jobIds;
        }

        // 🔹 Get ALL Applications (For Admin/Central)
        public async Task<List<object>> GetAllApplicationsAsync()
        {
            var applications = new List<object>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT 
                    a.ApplicationId,
                    a.AppliedDate,
                    u.UserId,
                    u.FullName,
                    u.Email,
                    j.Title AS JobTitle,
                    c.CompanyName,
                    a.ApplicationStatus
                FROM Applications a
                JOIN Users u ON a.UserId = u.UserId
                JOIN Jobs j ON a.JobId = j.JobId
                JOIN Companies c ON j.CompanyId = c.CompanyId
                ORDER BY a.AppliedDate DESC
            ", con);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                applications.Add(new
                {
                    ApplicationId = reader.GetInt32(0),
                    AppliedDate = reader.GetDateTime(1),
                    UserId = reader.GetInt32(2),
                    FullName = reader.GetString(3),
                    Email = reader.GetString(4),
                    JobTitle = reader.GetString(5),
                    CompanyName = reader.GetString(6),
                    ApplicationStatus = reader.GetString(7)
                });
            }

            return applications;
        }

        // 🔹 Delete Application (For Admin/Central)
        public async Task<bool> DeleteApplicationAsync(int applicationId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(
                "DELETE FROM Applications WHERE ApplicationId = @ApplicationId", 
                con
            );
            cmd.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = applicationId;

            await con.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        // 🔹 Get Applications for a specific Company
        public async Task<List<object>> GetApplicationsByCompanyAsync(int companyId)
        {
            var applications = new List<object>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT 
                    a.ApplicationId,
                    a.AppliedDate,
                    u.UserId,
                    u.FullName,
                    u.Email,
                    j.Title AS JobTitle,
                    j.JobId,
                    j.JobType,
                    c.CompanyName,
                    a.ApplicationStatus
                FROM Applications a
                JOIN Users u ON a.UserId = u.UserId
                JOIN Jobs j ON a.JobId = j.JobId
                JOIN Companies c ON j.CompanyId = c.CompanyId
                WHERE j.CompanyId = @CompanyId
                ORDER BY a.AppliedDate DESC
            ", con);

            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                applications.Add(new
                {
                    ApplicationId = reader.GetInt32(0),
                    AppliedDate = reader.GetDateTime(1),
                    UserId = reader.GetInt32(2),
                    FullName = reader.GetString(3),
                    Email = reader.GetString(4),
                    JobTitle = reader.GetString(5),
                    JobId = reader.GetInt32(6),
                    JobType = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    CompanyName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    ApplicationStatus = reader.GetString(9)
                });
            }

            return applications;
        }

        // 🔹 Delete Application for a specific Company (Security Check)
        public async Task<bool> DeleteCompanyApplicationAsync(int companyId, int applicationId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                DELETE a 
                FROM Applications a
                JOIN Jobs j ON a.JobId = j.JobId
                WHERE a.ApplicationId = @ApplicationId AND j.CompanyId = @CompanyId
            ", con);

            cmd.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = applicationId;
            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;

            await con.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        // 🔹 Mark Candidate as Placed
        public async Task<bool> MarkApplicationAsPlacedAsync(int companyId, int applicationId)
        {
            using var con = GetConnection();
            await con.OpenAsync();

            // 1. Get Info, Verify Ownership, and find an Admin for notification
            int userId = 0;
            int? adminId = null;
            string jobTitle = "";

            using (var cmdInfo = new SqlCommand(@"
                SELECT TOP 1 
                    a.UserId, 
                    j.Title,
                    (SELECT TOP 1 UserId FROM Users WHERE Role = 'SuperAdmin' AND IsActive = 1) as AdminId
                FROM Applications a
                INNER JOIN Jobs j ON a.JobId = j.JobId
                WHERE a.ApplicationId = @AppId AND j.CompanyId = @CompanyId
            ", con))
            {
                cmdInfo.Parameters.AddWithValue("@AppId", applicationId);
                cmdInfo.Parameters.AddWithValue("@CompanyId", companyId);

                using var reader = await cmdInfo.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userId = reader.GetInt32(0);
                    jobTitle = reader.GetString(1);
                    adminId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
                }
                else
                {
                    return false; // Not found or doesn't belong to company
                }
            }

            using var transaction = con.BeginTransaction();
            try
            {
                // 2. Update status
                using var cmdUpdate = new SqlCommand(@"
                    UPDATE Applications
                    SET ApplicationStatus = 'Placed'
                    WHERE ApplicationId = @AppId
                ", con, transaction);
                cmdUpdate.Parameters.AddWithValue("@AppId", applicationId);
                await cmdUpdate.ExecuteNonQueryAsync();

                // 2.5 Auto-remove from all other applications
                using var cmdRemoveOthers = new SqlCommand(@"
                    DELETE FROM Applications 
                    WHERE UserId = @UserId AND ApplicationId != @AppId
                ", con, transaction);
                cmdRemoveOthers.Parameters.AddWithValue("@UserId", userId);
                cmdRemoveOthers.Parameters.AddWithValue("@AppId", applicationId);
                await cmdRemoveOthers.ExecuteNonQueryAsync();

                // 3. Log Activity for Candidate
                using var cmdLogCandidate = new SqlCommand(@"
                    INSERT INTO UserActivityLogs (UserId, Action, ActionDate)
                    VALUES (@UId, @Action, GETDATE())
                ", con, transaction);
                cmdLogCandidate.Parameters.AddWithValue("@UId", userId);
                cmdLogCandidate.Parameters.AddWithValue("@Action", $"Congratulations! You have been marked as Placed for: {jobTitle}");
                await cmdLogCandidate.ExecuteNonQueryAsync();

                // 4. Log Activity for Central User (Admin)
                if (adminId.HasValue)
                {
                    using var cmdLogCentral = new SqlCommand(@"
                        INSERT INTO UserActivityLogs (UserId, Action, ActionDate)
                        VALUES (@AdminId, @Action, GETDATE())
                    ", con, transaction);
                    cmdLogCentral.Parameters.AddWithValue("@AdminId", adminId.Value);
                    cmdLogCentral.Parameters.AddWithValue("@Action", $"Placement Alert: Candidate (ID: {userId}) has been placed for {jobTitle}");
                    await cmdLogCentral.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // 🔹 Get User Profile (For Admin/Central)
        public async Task<object> GetUserProfileAsync(int userId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT 
                    ProfileId, 
                    UserId, 
                    Skills, 
                    ExperienceYears, 
                    Education, 
                    PreferredLocation, 
                    ResumePath
                FROM UserProfiles
                WHERE UserId = @UserId
            ", con);
            
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new
                {
                    ProfileId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Skills = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ExperienceYears = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    Education = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PreferredLocation = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ResumePath = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }

            return null;
        }

        /* ===================== CENTRAL USERS ===================== */

        public async Task<List<object>> GetAllUsersAsync()
        {
            var users = new List<object>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT 
                    UserId, 
                    FullName, 
                    Email, 
                    Role, 
                    CreatedAt, 
                    IsActive 
                FROM Users
                ORDER BY CreatedAt DESC
            ", con);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new
                {
                    UserId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    IsActive = reader.GetBoolean(5)
                });
            }

            return users;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var con = GetConnection();
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();

            try
            {
                // 1. Delete Activity Logs
                using (var cmd = new SqlCommand("DELETE FROM UserActivityLogs WHERE UserId = @UserId", con, transaction))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. Delete AI Recommendations
                using (var cmd = new SqlCommand("DELETE FROM AI_JobRecommendations WHERE UserId = @UserId", con, transaction))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Delete Applications
                using (var cmd = new SqlCommand("DELETE FROM Applications WHERE UserId = @UserId", con, transaction))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 4. Delete User Profile
                using (var cmd = new SqlCommand("DELETE FROM UserProfiles WHERE UserId = @UserId", con, transaction))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5. Finally, Delete the User
                using (var cmd = new SqlCommand("DELETE FROM Users WHERE UserId = @UserId", con, transaction))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    
                    transaction.Commit();
                    return rowsAffected > 0;
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<object>> GetUserActivityLogsAsync(int userId)
        {
            var logs = new List<object>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT LogId, Action, ActionDate 
                FROM UserActivityLogs 
                WHERE UserId = @UserId 
                ORDER BY ActionDate DESC
            ", con);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(new
                {
                    LogId = reader.GetInt32(0),
                    Action = reader.GetString(1),
                    ActionDate = reader.GetDateTime(2)
                });
            }

            return logs;
        }

        /* ===================== CENTRAL REPORTS ===================== */

        public async Task<object> GetCentralMultiFilterReportAsync(int? companyId, int? jobId)
        {
            using var con = GetConnection();
            await con.OpenAsync();

            var applicants = new List<object>();
            string condition = "WHERE (j.IsApproved = 1 OR j.IsApproved = 0)"; // Approved or Pending

            if (companyId.HasValue && companyId > 0)
                condition += " AND j.CompanyId = @CompanyId";
            if (jobId.HasValue && jobId > 0)
                condition += " AND j.JobId = @JobId";

            using (var cmd = new SqlCommand($@"
                SELECT 
                    u.FullName,
                    u.Email,
                    a.AppliedDate,
                    a.ApplicationStatus,
                    p.Skills,
                    p.ExperienceYears,
                    p.Education,
                    p.PreferredLocation,
                    j.Title as JobTitle,
                    c.CompanyName
                FROM Applications a
                JOIN Users u ON a.UserId = u.UserId
                JOIN Jobs j ON a.JobId = j.JobId
                LEFT JOIN Companies c ON j.CompanyId = c.CompanyId
                LEFT JOIN UserProfiles p ON u.UserId = p.UserId
                {condition}
                ORDER BY a.AppliedDate DESC
            ", con))
            {
                if (companyId.HasValue && companyId > 0)
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId.Value;
                if (jobId.HasValue && jobId > 0)
                    cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId.Value;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    applicants.Add(new
                    {
                        FullName = reader.GetString(0),
                        Email = reader.GetString(1),
                        AppliedDate = reader.GetDateTime(2),
                        Status = reader.IsDBNull(3) ? "Pending" : reader.GetString(3),
                        Skills = reader.IsDBNull(4) ? "N/A" : reader.GetString(4),
                        ExperienceValue = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        Education = reader.IsDBNull(6) ? "N/A" : reader.GetString(6),
                        Location = reader.IsDBNull(7) ? "N/A" : reader.GetString(7),
                        JobTitle = reader.GetString(8),
                        CompanyName = reader.IsDBNull(9) ? "N/A" : reader.GetString(9)
                    });
                }
            }

            return applicants;
        }

        public async Task<object> GetJobApplicantsReportAsync(int jobId)
        {
            using var con = GetConnection();
            await con.OpenAsync();

            // 1. Get Job and Company Details
            string jobTitle = "Unknown Job";
            string companyName = "Unknown Company";

            using (var cmdJob = new SqlCommand(@"
                SELECT j.Title, c.CompanyName 
                FROM Jobs j 
                LEFT JOIN Companies c ON j.CompanyId = c.CompanyId 
                WHERE j.JobId = @JobId
            ", con))
            {
                cmdJob.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;
                using var readerJob = await cmdJob.ExecuteReaderAsync();
                if (await readerJob.ReadAsync())
                {
                    jobTitle = readerJob.GetString(0);
                    companyName = readerJob.IsDBNull(1) ? "Unknown Company" : readerJob.GetString(1);
                }
            }

            // 2. Get Applicants
            var applicants = new List<object>();
            using (var cmdApp = new SqlCommand(@"
                SELECT 
                    u.FullName,
                    u.Email,
                    a.AppliedDate,
                    a.ApplicationStatus,
                    p.Skills,
                    p.ExperienceYears,
                    p.Education,
                    p.PreferredLocation
                FROM Applications a
                JOIN Users u ON a.UserId = u.UserId
                LEFT JOIN UserProfiles p ON u.UserId = p.UserId
                WHERE a.JobId = @JobId
                ORDER BY a.AppliedDate DESC
            ", con))
            {
                cmdApp.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;
                using var readerApp = await cmdApp.ExecuteReaderAsync();

                while (await readerApp.ReadAsync())
                {
                    applicants.Add(new
                    {
                        FullName = readerApp.GetString(0),
                        Email = readerApp.GetString(1),
                        AppliedDate = readerApp.GetDateTime(2),
                        Status = readerApp.IsDBNull(3) ? "Pending" : readerApp.GetString(3),
                        Skills = readerApp.IsDBNull(4) ? "N/A" : readerApp.GetString(4),
                        ExperienceValue = readerApp.IsDBNull(5) ? 0 : readerApp.GetInt32(5),
                        Education = readerApp.IsDBNull(6) ? "N/A" : readerApp.GetString(6),
                        Location = readerApp.IsDBNull(7) ? "N/A" : readerApp.GetString(7),
                        JobTitle = jobTitle,
                        CompanyName = companyName
                    });
                }
            }

            return new
            {
                JobTitle = jobTitle,
                CompanyName = companyName,
                Applicants = applicants
            };
        }

        /* ===================== ADD JOB ===================== */

        public async Task<int> AddJobAsync(Job job)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Jobs 
                (CompanyId, Title, Description, RequiredSkills, JobType, SalaryRange, PostedDate, LastDate, IsActive, IsApproved, JobDescriptionFile, JobDescriptionText, JobDescriptionUpdatedAt)
                VALUES
                (@CompanyId, @Title, @Description, @RequiredSkills, @JobType, @SalaryRange, @PostedDate, @LastDate, @IsActive, @IsApproved, @JobDescriptionFile, @JobDescriptionText, @JobDescriptionUpdatedAt);
                SELECT SCOPE_IDENTITY();
            ", con);

            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = job.CompanyId;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = job.Title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = job.Description ?? "";
            cmd.Parameters.Add("@RequiredSkills", SqlDbType.NVarChar).Value = job.RequiredSkills ?? "";
            cmd.Parameters.Add("@JobType", SqlDbType.NVarChar, 100).Value = job.JobType ?? "";
            cmd.Parameters.Add("@SalaryRange", SqlDbType.NVarChar, 100).Value = job.SalaryRange ?? "";
            cmd.Parameters.Add("@PostedDate", SqlDbType.DateTime2).Value = DateTime.UtcNow;
            cmd.Parameters.Add("@LastDate", SqlDbType.DateTime2).Value = (object)job.LastDate ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = true;
            cmd.Parameters.Add("@IsApproved", SqlDbType.Bit).Value = job.IsApproved;
            cmd.Parameters.Add("@JobDescriptionFile", SqlDbType.NVarChar).Value = (object)job.JobDescriptionFile ?? DBNull.Value;
            cmd.Parameters.Add("@JobDescriptionText", SqlDbType.NVarChar).Value = (object)job.JobDescriptionText ?? DBNull.Value;
            cmd.Parameters.Add("@JobDescriptionUpdatedAt", SqlDbType.DateTime).Value = (object)job.JobDescriptionUpdatedAt ?? DBNull.Value;

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        /* ===================== UPDATE JOB ===================== */

        public async Task<bool> UpdateJobAsync(int jobId, Job job)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE Jobs 
                SET CompanyId = @CompanyId,
                    Title = @Title,
                    Description = @Description,
                    RequiredSkills = @RequiredSkills,
                    JobType = @JobType,
                    SalaryRange = @SalaryRange,
                    LastDate = @LastDate,
                    IsApproved = @IsApproved,
                    JobDescriptionFile = @JobDescriptionFile,
                    JobDescriptionText = @JobDescriptionText,
                    JobDescriptionUpdatedAt = @JobDescriptionUpdatedAt
                WHERE JobId = @JobId AND IsActive = 1
            ", con);

            cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;
            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = job.CompanyId;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = job.Title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = job.Description ?? "";
            cmd.Parameters.Add("@RequiredSkills", SqlDbType.NVarChar).Value = job.RequiredSkills ?? "";
            cmd.Parameters.Add("@JobType", SqlDbType.NVarChar, 100).Value = job.JobType ?? "";
            cmd.Parameters.Add("@SalaryRange", SqlDbType.NVarChar, 100).Value = job.SalaryRange ?? "";
            cmd.Parameters.Add("@LastDate", SqlDbType.DateTime2).Value = (object)job.LastDate ?? DBNull.Value;
            cmd.Parameters.Add("@IsApproved", SqlDbType.Bit).Value = job.IsApproved;
            cmd.Parameters.Add("@JobDescriptionFile", SqlDbType.NVarChar).Value = (object)job.JobDescriptionFile ?? DBNull.Value;
            cmd.Parameters.Add("@JobDescriptionText", SqlDbType.NVarChar).Value = (object)job.JobDescriptionText ?? DBNull.Value;
            cmd.Parameters.Add("@JobDescriptionUpdatedAt", SqlDbType.DateTime).Value = (object)job.JobDescriptionUpdatedAt ?? DBNull.Value;

            await con.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        /* ===================== COMPANIES ===================== */

        public async Task<List<Company>> GetCompaniesAsync()
        {
            var companies = new List<Company>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT CompanyId, CompanyName, Industry, Location, IsCompanyVerified
                FROM Companies
                ORDER BY CompanyName
            ", con);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                companies.Add(new Company
                {
                    CompanyId = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    Industry = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Location = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    IsCompanyVerified = reader.IsDBNull(4) ? false : reader.GetBoolean(4)
                });
            }

            return companies;
        }

        public async Task<int> AddCompanyAsync(Company company)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Companies (CompanyName, Industry, Location, CreatedAt)
                VALUES (@CompanyName, @Industry, @Location, @CreatedAt);
                SELECT SCOPE_IDENTITY();
            ", con);

            cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 200).Value = company.CompanyName;
            cmd.Parameters.Add("@Industry", SqlDbType.NVarChar, 200).Value = company.Industry ?? "";
            cmd.Parameters.Add("@Location", SqlDbType.NVarChar, 200).Value = company.Location ?? "";
            cmd.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        /* ===================== COMPANY VERIFICATION ===================== */

        public async Task<bool> UploadVerificationDocumentsAsync(List<CompanyVerificationDocument> docs)
        {
            using var con = GetConnection();
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();

            try
            {
                foreach (var doc in docs)
                {
                    using var cmd = new SqlCommand(@"
                        INSERT INTO dbo.CompanyVerificationDocuments 
                        (nCompanyId, vDocumentType, vFileName, vFilePath, nRecordedBy, dRecordedOnUTC)
                        VALUES (@CompanyId, @Type, @FileName, @FilePath, @RecordedBy, GETUTCDATE())
                    ", con, transaction);

                    cmd.Parameters.Add("@CompanyId", SqlDbType.BigInt).Value = doc.CompanyId;
                    cmd.Parameters.Add("@Type", SqlDbType.VarChar, 100).Value = doc.DocumentType;
                    cmd.Parameters.Add("@FileName", SqlDbType.VarChar, 500).Value = doc.FileName;
                    cmd.Parameters.Add("@FilePath", SqlDbType.VarChar, 1000).Value = doc.FilePath;
                    cmd.Parameters.Add("@RecordedBy", SqlDbType.BigInt).Value = (object)doc.RecordedBy ?? DBNull.Value;

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;    
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CompanyVerificationDocument>> GetCompanyDocumentsAsync(int companyId)
        {
            var docs = new List<CompanyVerificationDocument>();
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT nDocumentId, nCompanyId, vDocumentType, vFileName, vFilePath, 
                       IsVerified, nVerifiedBy, dVerifiedOnUTC, IsRejected, vRejectReason
                FROM dbo.CompanyVerificationDocuments
                WHERE nCompanyId = @CompanyId
            ", con);
            cmd.Parameters.Add("@CompanyId", SqlDbType.BigInt).Value = companyId;

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                docs.Add(new CompanyVerificationDocument
                {
                    DocumentId = reader.GetInt64(0),
                    CompanyId = reader.GetInt64(1),
                    DocumentType = reader.GetString(2),
                    FileName = reader.GetString(3),
                    FilePath = reader.GetString(4),
                    IsVerified = reader.GetBoolean(5),
                    VerifiedBy = reader.IsDBNull(6) ? (long?)null : reader.GetInt64(6),
                    VerifiedOnUTC = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                    IsRejected = reader.GetBoolean(8),
                    RejectReason = reader.IsDBNull(9) ? "" : reader.GetString(9)
                });
            }
            return docs;
        }

        public async Task<bool> VerifyCompanyAsync(int companyId, bool isApproved, string reason, int adminId)
        {
            using var con = GetConnection();
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();

            try
            {
                var updateDocCmd = new SqlCommand(@"
                    UPDATE dbo.CompanyVerificationDocuments
                    SET IsVerified = @IsVerified, 
                        IsRejected = @IsRejected,
                        vRejectReason = @Reason,
                        nVerifiedBy = @AdminId,
                        dVerifiedOnUTC = GETUTCDATE()
                    WHERE nCompanyId = @CompanyId
                ", con, transaction);

                updateDocCmd.Parameters.Add("@IsVerified", SqlDbType.Bit).Value = isApproved;
                updateDocCmd.Parameters.Add("@IsRejected", SqlDbType.Bit).Value = !isApproved;
                updateDocCmd.Parameters.Add("@Reason", SqlDbType.VarChar, 500).Value = (object)reason ?? DBNull.Value;
                updateDocCmd.Parameters.Add("@AdminId", SqlDbType.BigInt).Value = adminId;
                updateDocCmd.Parameters.Add("@CompanyId", SqlDbType.BigInt).Value = companyId;

                await updateDocCmd.ExecuteNonQueryAsync();

                if (isApproved)
                {
                    var updateCompanyCmd = new SqlCommand(@"
                        UPDATE Companies SET IsCompanyVerified = 1 WHERE CompanyId = @CompanyId
                    ", con, transaction);
                    updateCompanyCmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;
                    await updateCompanyCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCompanyDocumentsAsync(int companyId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("DELETE FROM dbo.CompanyVerificationDocuments WHERE nCompanyId = @CompanyId", con);
            cmd.Parameters.Add("@CompanyId", SqlDbType.BigInt).Value = companyId;
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsCompanyVerifiedAsync(int companyId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("SELECT IsCompanyVerified FROM Companies WHERE CompanyId = @CompanyId", con);
            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null && Convert.ToBoolean(result);
        }
        /* ===================== DYNAMIC REPORTS ===================== */

        public async Task<List<ReportConfiguration>> GetReportConfigurationsAsync()
        {
            var reports = new List<ReportConfiguration>();
            using var con = GetConnection();
            using var cmd = new SqlCommand("SELECT * FROM ReportConfigurations WHERE IsActive = 1 ORDER BY CreatedAt DESC", con);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reports.Add(new ReportConfiguration
                {
                    ReportId = reader.GetInt32(0),
                    ReportName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    BaseTable = reader.GetString(3),
                    SelectedFields = reader.GetString(4),
                    Filters = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsActive = reader.GetBoolean(6),
                    CreatedBy = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.GetDateTime(9)
                });
            }
            return reports;
        }

        public async Task<ReportConfiguration?> GetReportConfigurationByIdAsync(int reportId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("SELECT * FROM ReportConfigurations WHERE ReportId = @Id", con);
            cmd.Parameters.AddWithValue("@Id", reportId);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ReportConfiguration
                {
                    ReportId = reader.GetInt32(0),
                    ReportName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    BaseTable = reader.GetString(3),
                    SelectedFields = reader.GetString(4),
                    Filters = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsActive = reader.GetBoolean(6),
                    CreatedBy = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.GetDateTime(9)
                };
            }
            return null;
        }

        public async Task<int> AddReportConfigurationAsync(ReportConfiguration config)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO ReportConfigurations (ReportName, Description, BaseTable, SelectedFields, Filters, CreatedBy)
                VALUES (@Name, @Desc, @Table, @Fields, @Filters, @UserId);
                SELECT SCOPE_IDENTITY();", con);
            cmd.Parameters.AddWithValue("@Name", config.ReportName);
            cmd.Parameters.AddWithValue("@Desc", (object)config.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Table", config.BaseTable);
            cmd.Parameters.AddWithValue("@Fields", config.SelectedFields);
            cmd.Parameters.AddWithValue("@Filters", (object)config.Filters ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object)config.CreatedBy ?? DBNull.Value);
            await con.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateReportConfigurationAsync(ReportConfiguration config)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE ReportConfigurations 
                SET ReportName = @Name, Description = @Desc, SelectedFields = @Fields, Filters = @Filters, UpdatedAt = GETUTCDATE()
                WHERE ReportId = @Id", con);
            cmd.Parameters.AddWithValue("@Id", config.ReportId);
            cmd.Parameters.AddWithValue("@Name", config.ReportName);
            cmd.Parameters.AddWithValue("@Desc", (object)config.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fields", config.SelectedFields);
            cmd.Parameters.AddWithValue("@Filters", (object)config.Filters ?? DBNull.Value);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<IDictionary<string, object>>> GetDynamicReportDataAsync(string baseTable, string[] selectedFields, string? filterClause, Dictionary<string, object> parameters)
        {
            var data = new List<IDictionary<string, object>>();
            using var con = GetConnection();
            
            // Basic validation to prevent SQL injection for table and column names
            // In a production app, we should use a whitelist of allowed tables and columns
            string columns = string.Join(", ", selectedFields);
            string sql = $"SELECT {columns} FROM {baseTable}";
            if (!string.IsNullOrEmpty(filterClause))
            {
                sql += $" WHERE {filterClause}";
            }

            using var cmd = new SqlCommand(sql, con);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                data.Add(row);
            }
            return data;
        }

        public async Task LogReportGenerationAsync(ReportGenerationLog log)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO ReportGenerationLogs (ReportId, UserId, Format, FilterValues)
                VALUES (@ReportId, @UserId, @Format, @FilterValues)", con);
            cmd.Parameters.AddWithValue("@ReportId", log.ReportId);
            cmd.Parameters.AddWithValue("@UserId", log.UserId);
            cmd.Parameters.AddWithValue("@Format", log.Format);
            cmd.Parameters.AddWithValue("@FilterValues", (object)log.FilterValues ?? DBNull.Value);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}