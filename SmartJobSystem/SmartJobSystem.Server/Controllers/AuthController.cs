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

            HttpContext.Session.SetInt32("UserId", (int)reader["UserId"]);
            HttpContext.Session.SetString("UserName", reader["FullName"].ToString());
            HttpContext.Session.SetString("Role", reader["Role"].ToString());

            return Ok(new
            {
                userId = reader["UserId"],
                name = reader["FullName"],
                role = reader["Role"]
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
                INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive)
                OUTPUT INSERTED.UserId
                VALUES (@Name, @Email, @Password, 'User', 1)
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
}
