namespace GMLSSystem.Shared.Models
{
    public class CreateContractDto
    {
        public string ContractNumber { get; set; } = string.Empty;

        public int ClientId { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(12);

        public string ServiceLevel { get; set; } = "Standard";

        public string Region { get; set; } = string.Empty;

        public string Status { get; set; } = "Draft";

        public string? SignedAgreementPath { get; set; }

        public string? OriginalFileName { get; set; }
    }
}