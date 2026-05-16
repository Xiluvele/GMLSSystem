using GMLSSystem.Models.Enums;
using GMLSSystem.Models;

namespace GMLSSystem.Services.Factories
{
    // Factory Method Pattern Implementation
    public abstract class ContractFactory
    {
        public abstract Contract CreateContract(Client client, DateTime startDate, DateTime endDate, ServiceLevel level);

        public static ContractFactory GetFactory(string region)
        {
            return region?.ToLower() switch
            {
                "international" => new InternationalContractFactory(),
                "local" => new LocalContractFactory(),
                _ => new LocalContractFactory()
            };
        }
    }

    public class LocalContractFactory : ContractFactory
    {
        public override Contract CreateContract(Client client, DateTime startDate, DateTime endDate, ServiceLevel level)
        {
            return new Contract
            {
                ClientId = client.ClientId,
                ClientName = client.Name,
                StartDate = startDate,
                EndDate = endDate,
                ServiceLevel = level,
                Region = client.Region,
                Status = ContractStatus.Draft
            };
        }
    }

    public class InternationalContractFactory : ContractFactory
    {
        public override Contract CreateContract(Client client, DateTime startDate, DateTime endDate, ServiceLevel level)
        {
            return new Contract
            {
                ClientId = client.ClientId,
                ClientName = client.Name,
                StartDate = startDate,
                EndDate = endDate,
                ServiceLevel = level,
                Region = "International",
                Status = ContractStatus.Draft
            };
        }
    }
}
