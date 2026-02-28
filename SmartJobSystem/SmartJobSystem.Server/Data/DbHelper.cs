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
        public async Task<List<Job>> GetJobsAsync()
        {
            var jobs = new List<Job>();

            using var con = GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT 
            j.JobId,
            j.CompanyId,
            j.Title,
            j.Description,
            j.RequiredSkills,
            j.JobType,
            j.SalaryRange,
            j.PostedDate,
            j.IsActive,
            c.CompanyName
        FROM Jobs j
        LEFT JOIN Companies c ON j.CompanyId = c.CompanyId
        WHERE j.IsActive = 1
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
                    IsActive = reader.GetBoolean(8),
                    CompanyName = reader.IsDBNull(9) ? null : reader.GetString(9)
                });
            }

            return jobs;
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
                    c.CompanyName
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
                    CompanyName = reader.GetString(6)
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
                        Experience = readerApp.IsDBNull(5) ? 0 : readerApp.GetInt32(5),
                        Education = readerApp.IsDBNull(6) ? "N/A" : readerApp.GetString(6),
                        Location = readerApp.IsDBNull(7) ? "N/A" : readerApp.GetString(7)
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
                (CompanyId, Title, Description, RequiredSkills, JobType, SalaryRange, PostedDate, IsActive)
                VALUES
                (@CompanyId, @Title, @Description, @RequiredSkills, @JobType, @SalaryRange, @PostedDate, @IsActive);
                SELECT SCOPE_IDENTITY();
            ", con);

            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = job.CompanyId;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = job.Title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = job.Description ?? "";
            cmd.Parameters.Add("@RequiredSkills", SqlDbType.NVarChar).Value = job.RequiredSkills ?? "";
            cmd.Parameters.Add("@JobType", SqlDbType.NVarChar, 100).Value = job.JobType ?? "";
            cmd.Parameters.Add("@SalaryRange", SqlDbType.NVarChar, 100).Value = job.SalaryRange ?? "";
            cmd.Parameters.Add("@PostedDate", SqlDbType.DateTime2).Value = DateTime.UtcNow;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = true;

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
                    SalaryRange = @SalaryRange
                WHERE JobId = @JobId AND IsActive = 1
            ", con);

            cmd.Parameters.Add("@JobId", SqlDbType.Int).Value = jobId;
            cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = job.CompanyId;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = job.Title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = job.Description ?? "";
            cmd.Parameters.Add("@RequiredSkills", SqlDbType.NVarChar).Value = job.RequiredSkills ?? "";
            cmd.Parameters.Add("@JobType", SqlDbType.NVarChar, 100).Value = job.JobType ?? "";
            cmd.Parameters.Add("@SalaryRange", SqlDbType.NVarChar, 100).Value = job.SalaryRange ?? "";

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
                SELECT CompanyId, CompanyName, Industry, Location
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
                    Location = reader.IsDBNull(3) ? "" : reader.GetString(3)
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
    }
}