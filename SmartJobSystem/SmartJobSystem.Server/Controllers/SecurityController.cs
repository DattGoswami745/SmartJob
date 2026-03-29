using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Helpers;
using Microsoft.Extensions.Configuration;
using SmartJobSystem.Server.Data;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/security")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly DbHelper _db;

        public SecurityController(DbHelper db)
        {
            _db = db;
        }

        [HttpPost("encrypt")]
        public async Task<IActionResult> Encrypt([FromBody] SecurityRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Text to encrypt is required.");

            try
            {
                var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") 
                    ?? "default_key_12345678901234567890";
                
                string encrypted = SecurityHelper.Encrypt(request.Text, encryptionKey);
                return Ok(new { Result = encrypted });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Encryption failed: {ex.Message}");
            }
        }

        [HttpPost("decrypt")]
        public async Task<IActionResult> Decrypt([FromBody] SecurityRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Cipher text to decrypt is required.");

            try
            {
                var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") 
                    ?? "default_key_12345678901234567890";

                string decrypted = SecurityHelper.Decrypt(request.Text, encryptionKey);
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
