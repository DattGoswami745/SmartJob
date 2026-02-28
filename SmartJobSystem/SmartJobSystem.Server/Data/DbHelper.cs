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
            JobId,
            CompanyId,
            Title,
            Description,
            RequiredSkills,
            JobType,
            SalaryRange,
            PostedDate,
            IsActive
        FROM Jobs
        WHERE IsActive = 1
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
                    IsActive = reader.GetBoolean(8)
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