namespace GMLSSystem.Interfaces
{
    // Factory Method Pattern - Product Interface
    public interface IContract
    {
        int ContractId { get; set; }
        string ContractNumber { get; set; }
        string ClientName { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        string Status { get; set; }
        bool IsValid();
        string GetRegionSpecificTerms();
    }

    // Factory Method Pattern - Creator Interface
    public interface IContractCreator
    {
        IContract CreateContract();
    }
}
