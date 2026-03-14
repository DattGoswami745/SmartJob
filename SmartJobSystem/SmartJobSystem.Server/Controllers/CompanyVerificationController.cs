using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Models;
using System.IO;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/company")]
    public class CompanyVerificationController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IWebHostEnvironment _env;

        public CompanyVerificationController(DbHelper db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPost("upload-verification-documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] List<IFormFile> files)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");
            if (companyId == null) return Unauthorized(new { message = "Company session expired" });

            if (files == null || files.Count != 3)
                return BadRequest(new { message = "Please upload all 3 required documents (Incorporation, GST, PAN)." });

            // 🧹 Clear existing documents before re-uploading (to fix broken absolute paths)
            await _db.DeleteCompanyDocumentsAsync(companyId.Value);

            var docs = new List<CompanyVerificationDocument>();
            var allowedTypes = new[] { "Incorporation", "GST", "PAN" };

            // Ensure directory exists in wwwroot
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "Uploads", "VerificationDocuments");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var docType = i < allowedTypes.Length ? allowedTypes[i] : "Other";
                
                var fileName = $"{companyId}_{docType}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Relative path for web access
                var dbPath = $"/Uploads/VerificationDocuments/{fileName}";

                docs.Add(new CompanyVerificationDocument
                {
                    CompanyId = companyId.Value,
                    DocumentType = docType,
                    FileName = file.FileName,
                    FilePath = dbPath,
                    RecordedBy = HttpContext.Session.GetInt32("UserId")
                });
            }

            await _db.UploadVerificationDocumentsAsync(docs);
            return Ok(new { message = "Documents uploaded successfully. Verification is pending." });
        }
    }
}
