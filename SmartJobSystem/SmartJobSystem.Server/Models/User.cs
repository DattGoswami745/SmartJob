namespace SmartJobSystem.Server.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }    // Add this
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Add this
        public string Role { get; set; }         // Add this
        public DateTime CreatedAt { get; set; }  // Add this
        public bool IsActive { get; set; }       // Add this
    }
}