using GMLSSystem.API.Models.Enums;

namespace GMLSSystem.API.Models
{
    public class Contract
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public int ClientId { get; set; }
        public string? ClientName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }

        public ServiceLevel ServiceLevel { get; set; }
        public string? SignedAgreementPath { get; set; }
        public string? OriginalFileName { get; set; }
        public DateTime CreatedAt { get; set; }

        // for factory method pattern - region specific
        public string Region { get; set; }

        //navigation property
        public virtual Client? Client { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }

        public Contract()
        {
            ContractNumber = GenerateContractNumber();
            ServiceRequests = new List<ServiceRequest>();
            CreatedAt = DateTime.UtcNow;
        }

        private string GenerateContractNumber()
        {
            return $"CTR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public bool IsActive()
        {
            var now = DateTime.UtcNow;
            return Status == ContractStatus.Active && StartDate <= now && EndDate >= now;


        }

        public bool canCreateServiceRequest()
        {
            return Status == ContractStatus.Active;
        }
    }
}
