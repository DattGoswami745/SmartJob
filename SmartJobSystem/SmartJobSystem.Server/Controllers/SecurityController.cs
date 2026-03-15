using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Helpers;
using Microsoft.Extensions.Configuration;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/security")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly string _encryptionKey;

        public SecurityController(IConfiguration config)
        {
            _encryptionKey = config["SecuritySettings:EncryptionKey"] ?? "default_key_12345678901234567890";
        }

        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] SecurityRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Text to encrypt is required.");

            try
            {
                string encrypted = SecurityHelper.Encrypt(request.Text, _encryptionKey);
                return Ok(new { Result = encrypted });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Encryption failed: {ex.Message}");
            }
        }

        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] SecurityRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Cipher text to decrypt is required.");

            try
            {
                string decrypted = SecurityHelper.Decrypt(request.Text, _encryptionKey);
                return Ok(new { Result = decrypted });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Decryption failed: {ex.Message}");
            }
        }
    }

    public class SecurityRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
