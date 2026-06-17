using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMLSSystem.Shared.Models
{
    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public string ContractNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
    }

    public class CreateServiceRequestDto
    {
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
    }
}
