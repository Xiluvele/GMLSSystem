using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;

namespace GMLSSystem.Services
{
    public interface IContractRepository
    {
        Task<Contract> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<Contract> AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task DeleteAsync(int id);
        Task<IEnumerable<Contract>> SearchAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
    }
}