using GMLSSystem.Models.Enums;

namespace GMLSSystem.Models
{
    
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string RequestNumber { get; set; }
        public string Description { get; set; }
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime RequestDate { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime? CompletionDate { get; set; }

        //navigation property
        public Contract? Contract { get; set; }
       

        public ServiceRequest()
        {
            RequestNumber = GenerateRequestNumber();
            RequestDate = DateTime.UtcNow;
            Status = RequestStatus.Pending;
        }

        private string GenerateRequestNumber()
        {
            return $"SRQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}
