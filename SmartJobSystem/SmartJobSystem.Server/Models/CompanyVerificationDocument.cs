using System;

namespace SmartJobSystem.Server.Models
{
    public class CompanyVerificationDocument
    {
        public long DocumentId { get; set; }
        public long CompanyId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public byte[] DocumentFile { get; set; }
        public string ContentType { get; set; }
        public bool IsVerified { get; set; }
        public long? VerifiedBy { get; set; }
        public DateTime? VerifiedOnUTC { get; set; }
        public bool IsRejected { get; set; }
        public string RejectReason { get; set; }
        public long? RecordedBy { get; set; }
        public DateTime RecordedOnUTC { get; set; }
    }
}
