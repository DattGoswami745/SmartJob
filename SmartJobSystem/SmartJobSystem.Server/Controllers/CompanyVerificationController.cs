using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Models;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Helpers;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SmartJobSystem.Server.Controllers
{
    [ApiController]
    [Route("api/company")]
    public class CompanyVerificationController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IConfiguration _config;

        public CompanyVerificationController(DbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
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
            var encryptionKey = _config["SecuritySettings:EncryptionKey"] ?? "";

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var docType = i < allowedTypes.Length ? allowedTypes[i] : "Other";
                
                // Read file content
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                byte[] fileBytes = ms.ToArray();

                // ENCRYPT THE FILE CONTENT
                byte[] encryptedFileBytes = SecurityHelper.EncryptBytes(fileBytes, encryptionKey);

                docs.Add(new CompanyVerificationDocument
                {
                    CompanyId = companyId.Value,
                    DocumentType = docType,
                    FileName = file.FileName,
                    FilePath = "(pending)",
                    DocumentFile = encryptedFileBytes,
                    ContentType = file.ContentType,
                    RecordedBy = HttpContext.Session.GetInt32("UserId")
                });
            }

            await _db.UploadVerificationDocumentsAsync(docs);

            // POST-PROCESSING: Update FilePath to the new download endpoint (Encrypted Download Path)
            var savedDocs = await _db.GetCompanyDocumentsAsync(companyId.Value);
            foreach (var d in savedDocs)
            {
                var downloadPathPlain = $"/api/company/download-document/{d.DocumentId}";
                var encryptedDownloadPath = SecurityHelper.Encrypt(downloadPathPlain, encryptionKey);
                
                await _db.UpdateDocumentPathAsync(d.DocumentId, encryptedDownloadPath);
            }

            return Ok(new { message = "Documents uploaded and secured in database. Verification is pending." });
        }

        [HttpGet("download-document/{id}")]
        public async Task<IActionResult> DownloadDocument(long id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");
            var role = HttpContext.Session.GetString("Role");
            
            if (companyId == null && role != "Central" && role != "SuperAdmin") 
                return Unauthorized(new { message = "Session expired or unauthorized" });

            var doc = await _db.GetCompanyDocumentBinaryAsync(id);
            if (doc == null || doc.DocumentFile == null)
                return NotFound(new { message = "Document not found." });

            // Permission check: either Central Admin or the Company owner
            if (role != "Central" && role != "SuperAdmin" && doc.CompanyId != companyId)
                return Unauthorized(new { message = "You do not have permission to view this document." });

            var encryptionKey = _config["SecuritySettings:EncryptionKey"] ?? "";

            // DECRYPT THE FILE CONTENT
            byte[] decryptedFileBytes;
            try {
                decryptedFileBytes = SecurityHelper.DecryptBytes(doc.DocumentFile, encryptionKey);
            } catch {
                decryptedFileBytes = doc.DocumentFile; // Fallback
            }

            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{doc.FileName}\"");
            return File(decryptedFileBytes, doc.ContentType ?? "application/octet-stream");
        }
    }
}
