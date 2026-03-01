using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobAPI.Helpers;

namespace SmartJobAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            using SqlConnection con =
                new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT UserId, FullName, Role, PasswordHash
                FROM Users
                WHERE Email=@Email AND IsActive=1
            ", con);

            cmd.Parameters.AddWithValue("@Email", dto.Email);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return Unauthorized(new { message = "Invalid credentials" });

            string enteredHash = PasswordHelper.HashPassword(dto.Password);
            string dbHash = reader["PasswordHash"].ToString();

            if (enteredHash != dbHash)
                return Unauthorized(new { message = "Invalid credentials" });

            int userId = (int)reader["UserId"];
            string userNameResult = reader["FullName"].ToString();
            string roleResult = reader["Role"].ToString();

            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("UserName", userNameResult);
            HttpContext.Session.SetString("Role", roleResult);

            reader.Close(); // Close the reader before executing another command on the same connection

            // --- LOG ACTIVITY ---
            var logCmd = new SqlCommand(@"
                INSERT INTO dbo.UserActivityLogs (UserId, Action, ActionDate)
                VALUES (@UId, 'Logged in to system', GETDATE())
            ", con);
            logCmd.Parameters.AddWithValue("@UId", userId);
            logCmd.ExecuteNonQuery();

            return Ok(new
            {
                userId = userId,
                name = userNameResult,
                role = roleResult
            });
        }

        // ================= SIGNUP (FIXED) =================
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupDto dto)
        {
            string hash = PasswordHelper.HashPassword(dto.Password);

            using SqlConnection con =
                new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // 🔍 Check email
            var check = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Email=@Email", con);
            check.Parameters.AddWithValue("@Email", dto.Email);

            if ((int)check.ExecuteScalar() > 0)
                return BadRequest(new { message = "Email already exists" });

            // 🧑 Insert into Users + GET UserId
            var insertUser = new SqlCommand(@"
                INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, IsEmailVerified, EmailOTP, EmailOTPExpiry)
                OUTPUT INSERTED.UserId
                VALUES (@Name, @Email, @Password, 'User', 1, 0, '111111', DATEADD(MINUTE, 10, GETDATE()))
            ", con);

            insertUser.Parameters.AddWithValue("@Name", dto.FullName);
            insertUser.Parameters.AddWithValue("@Email", dto.Email);
            insertUser.Parameters.AddWithValue("@Password", hash);

            int userId = (int)insertUser.ExecuteScalar();

            // ✅ VERY IMPORTANT: create empty UserProfile
            var insertProfile = new SqlCommand(@"
                INSERT INTO UserProfiles (UserId)
                VALUES (@UserId)
            ", con);

            insertProfile.Parameters.AddWithValue("@UserId", userId);
            insertProfile.ExecuteNonQuery();

            return Ok(new
            {
                message = "Signup successful",
                userId = userId
            });
        }

        // ================= LOGOUT =================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok();
        }

        // ================= SESSION CHECK =================
        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized(new { message = "Session expired" });

            return Ok(new
            {
                userId = userId,
                userName = HttpContext.Session.GetString("UserName"),
                role = HttpContext.Session.GetString("Role")
            });
        }

        // ================= VERIFY EMAIL =================
        [HttpPost("verify-email")]
        public IActionResult VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            using SqlConnection con = new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand("SELECT UserId, EmailOTP, EmailOTPExpiry FROM Users WHERE Email = @Email AND IsActive = 1", con);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound(new { message = "User not found" });

            var dbOtp = reader["EmailOTP"]?.ToString();
            var expiryObj = reader["EmailOTPExpiry"];
            
            if (string.IsNullOrEmpty(dbOtp) || dbOtp != dto.Otp)
                return BadRequest(new { message = "Invalid OTP" });

            if (expiryObj != DBNull.Value && (DateTime)expiryObj < DateTime.UtcNow)
                return BadRequest(new { message = "OTP has expired" });

            reader.Close(); 

            var updateCmd = new SqlCommand(@"
                UPDATE Users 
                SET IsEmailVerified = 1, EmailOTP = NULL, EmailOTPExpiry = NULL 
                WHERE Email = @Email
            ", con);
            updateCmd.Parameters.AddWithValue("@Email", dto.Email);
            updateCmd.ExecuteNonQuery();

            return Ok(new { message = "Email verified successfully" });
        }

        // ================= RESEND OTP =================
        [HttpPost("resend-otp")]
        public IActionResult ResendOtp([FromBody] ResendOtpDto dto)
        {
            using SqlConnection con = new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand("SELECT UserId FROM Users WHERE Email = @Email AND IsActive = 1", con);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound(new { message = "User not found" });
            
            reader.Close();

            var updateCmd = new SqlCommand(@"
                UPDATE Users 
                SET EmailOTP = '111111', EmailOTPExpiry = DATEADD(MINUTE, 10, GETDATE()) 
                WHERE Email = @Email
            ", con);
            updateCmd.Parameters.AddWithValue("@Email", dto.Email);
            updateCmd.ExecuteNonQuery();

            return Ok(new { message = "OTP resent successfully" });
        }
    }

    // ================= DTOs =================
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class SignupDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class VerifyEmailDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }

    public class ResendOtpDto
    {
        public string Email { get; set; }
    }
}
