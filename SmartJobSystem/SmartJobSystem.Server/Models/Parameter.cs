namespace SmartJobSystem.Server.Models
{
    public class Parameter
    {
        public string ParamKey { get; set; } = string.Empty;
        public string ParamValue { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
