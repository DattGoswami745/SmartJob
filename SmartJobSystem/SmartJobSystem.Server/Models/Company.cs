namespace SmartJobSystem.Server.Models
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}