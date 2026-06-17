namespace GMLSSystem.API.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public string Region { get; set; }
        public DateTime CreatedAt { get; set; }

        //navigation property
        public virtual ICollection<Contract> Contracts { get; set; }

        public Client()
        {
            Contracts = new List<Contract>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
