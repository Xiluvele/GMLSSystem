namespace GMLSSystem.Models
{
    public class SystemLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string Type { get; set; }
    }
}