using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMLSSystem.Shared.Models
{
    public class ContractDto
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceLevel { get; set; } = string.Empty;
        public string? SignedAgreementPath { get; set; }
        public string? OriginalFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Region { get; set; }
        public List<ServiceRequestDto> ServiceRequests { get; set; } = new();
    }

    public class CreateContactDto 
    {
      public int ClientId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ServiceLevel { get; set; } = "Standard";
        public string? Region { get; set; }
        public string Status { get; set; } = "Draft";
    }
    public class UpdateContractStatusDto
    {
      public string Status { get; set; } = string.Empty;
    }
}
