using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobAPI.Helpers;
using SmartJobSystem.Server.Helpers;

namespace SmartJobAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly string _encryptionKey;

        public AuthController(IConfiguration config, IEmailService emailService)
        {
            _config = config;
            _emailService = emailService;
            _encryptionKey = _config["SecuritySettings:EncryptionKey"] ?? throw new InvalidOperationException("EncryptionKey is not configured.");
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

            string enteredHash = PasswordHelper.HashPassword(dto.Password ?? "");
            string dbHash = reader["PasswordHash"]?.ToString() ?? "";

            if (enteredHash != dbHash)
                return Unauthorized(new { message = "Invalid credentials" });

            int userId = (int)(reader["UserId"] ?? 0);
            string userNameResult = reader["FullName"]?.ToString() ?? "";
            string roleResult = reader["Role"]?.ToString() ?? "";

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
        public async Task<IActionResult> Signup([FromBody] SignupDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password) || string.IsNullOrEmpty(dto.FullName))
            {
                return BadRequest(new { message = "All fields (FullName, Email, Password) are required." });
            }

            try
            {
                using SqlConnection con = new(_config.GetConnectionString("DefaultConnection"));
                await con.OpenAsync();

                // 🔍 Check email
                var check = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@Email", con);
                check.Parameters.AddWithValue("@Email", dto.Email ?? "");
                
                var existingCount = await check.ExecuteScalarAsync();
                if (existingCount != null && Convert.ToInt32(existingCount) > 0)
                {
                    return Conflict(new { message = "This email is already registered. Please login or use a different email." });
                }

                // 🎲 Generate OTP
                string otp = new Random().Next(100000, 999999).ToString();
                string encryptedOtp = SecurityHelper.Encrypt(otp, _encryptionKey);
                DateTime expiry = DateTime.UtcNow.AddMinutes(5);

                // 🧑 Insert into Users + GET UserId
                var insertUser = new SqlCommand(@"
                    INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, IsEmailVerified, EmailOTP, EmailOTPExpiry, CreatedAt)
                    OUTPUT INSERTED.UserId
                    VALUES (@Name, @Email, @Password, 'User', 1, 0, @OTP, @Expiry, GETDATE())
                ", con);

                insertUser.Parameters.AddWithValue("@Name", dto.FullName ?? "");
                insertUser.Parameters.AddWithValue("@Email", dto.Email ?? "");
                insertUser.Parameters.AddWithValue("@Password", PasswordHelper.HashPassword(dto.Password ?? ""));
                insertUser.Parameters.AddWithValue("@OTP", encryptedOtp);
                insertUser.Parameters.AddWithValue("@Expiry", expiry);

                object? result = await insertUser.ExecuteScalarAsync();
                if (result == null)
                {
                    throw new Exception("Failed to insert user and retrieve UserId.");
                }
                int userId = Convert.ToInt32(result);

                // ✅ Create empty UserProfile
                try
                {
                    var insertProfile = new SqlCommand("INSERT INTO UserProfiles (UserId) VALUES (@UserId)", con);
                    insertProfile.Parameters.AddWithValue("@UserId", userId);
                    await insertProfile.ExecuteNonQueryAsync();
                }
                catch (Exception profEx)
                {
                    // Log but maybe don't fail signup completely? 
                    // Or actually, it's better to know why it failed.
                    Console.WriteLine($"UserProfile creation failed for UserId {userId}: {profEx.Message}");
                    // We'll continue for now so they can at least verify email
                }

                // 📧 Send Email
                try
                {
                    string emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                            <h2 style='color: #4CAF50; text-align: center;'>Welcome to SmartJob!</h2>
                            <p>Hello <strong>{dto.FullName}</strong>,</p>
                            <p>Thank you for signing up. To complete your registration, please use the following One-Time Password (OTP) to verify your email address:</p>
                            <div style='background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #333; margin: 20px 0; border-radius: 5px;'>
                                {otp}
                            </div>
                            <p style='color: #666; font-size: 14px;'>This OTP is valid for <strong>5 minutes</strong>. If you did not request this email, please ignore it.</p>
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                            <p style='text-align: center; color: #999; font-size: 12px;'>&copy; {DateTime.Now.Year} SmartJob System. All rights reserved.</p>
                        </div>";

                    await _emailService.SendEmailAsync(dto.Email ?? "", "Verify Your SmartJob Account", emailBody);
                }
                catch (Exception emailEx)
                {
                    // Log the error but don't fail the signup if the user is already created
                    // Better to let them "Resend OTP" than failing the whole process
                    Console.WriteLine($"Email sending failed for {dto.Email}: {emailEx.Message}");
                    return Ok(new
                    {
                        message = "Signup successful, but we couldn't send the verification email. Please use the 'Resend OTP' option.",
                        userId = userId,
                        emailError = true
                    });
                }

                return Ok(new
                {
                    message = "Signup successful! A 6-digit OTP has been sent to your email.",
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Signup error: {ex}");
                return StatusCode(500, new { message = "An error occurred during signup. Please try again later." });
            }
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

            var dbOtpEncrypted = reader["EmailOTP"]?.ToString();
            var expiryObj = reader["EmailOTPExpiry"];
            
            if (string.IsNullOrEmpty(dbOtpEncrypted))
                return BadRequest(new { message = "Invalid OTP" });

            string dbOtp = SecurityHelper.Decrypt(dbOtpEncrypted, _encryptionKey);

            if (dbOtp != dto.Otp)
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
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            using SqlConnection con = new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand("SELECT UserId FROM Users WHERE Email = @Email AND IsActive = 1", con);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound(new { message = "User not found" });
            
            reader.Close();

            // 🎲 Generate NEW OTP
            string otp = new Random().Next(100000, 999999).ToString();
            string encryptedOtp = SecurityHelper.Encrypt(otp, _encryptionKey);
            DateTime expiry = DateTime.UtcNow.AddMinutes(5);

            var updateCmd = new SqlCommand(@"
                UPDATE Users 
                SET EmailOTP = @OTP, EmailOTPExpiry = @Expiry 
                WHERE Email = @Email
            ", con);
            updateCmd.Parameters.AddWithValue("@OTP", encryptedOtp);
            updateCmd.Parameters.AddWithValue("@Expiry", expiry);
            updateCmd.Parameters.AddWithValue("@Email", dto.Email);
            updateCmd.ExecuteNonQuery();

            // 📧 Send Email
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #2196F3; text-align: center;'>New Verification OTP</h2>
                    <p>Hello,</p>
                    <p>We received a request for a new verification code for your SmartJob account. Please use the OTP below:</p>
                    <div style='background-color: #f0f7ff; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #0d47a1; margin: 20px 0; border-radius: 5px;'>
                        {otp}
                    </div>
                    <p style='color: #666; font-size: 14px;'>This OTP is valid for <strong>5 minutes</strong>. If you did not request this, your account is still secure.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='text-align: center; color: #999; font-size: 12px;'>&copy; {DateTime.Now.Year} SmartJob System. All rights reserved.</p>
                </div>";

            await _emailService.SendEmailAsync(dto.Email ?? "", "Your New Verification Code", emailBody);

            return Ok(new { message = "OTP resent successfully" });
        }
    }

    // ================= DTOs =================
    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class SignupDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class VerifyEmailDto
    {
        public string? Email { get; set; }
        public string? Otp { get; set; }
    }

    public class ResendOtpDto
    {
        public string? Email { get; set; }
    }
}
