namespace GMLSSystem.Models
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string ContractManager = "ContractManager";
        public const string LogisticsCoordinator = "LogisticsCoordinator";
        public const string FinanceUser = "FinanceUser";
        public const string Client = "Client";

        public static readonly string[] AllRoles = new[]
        {
            Admin, ContractManager, LogisticsCoordinator, FinanceUser, Client
        };
    }
}
