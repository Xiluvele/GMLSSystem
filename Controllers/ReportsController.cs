using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;

namespace GMLSSystem.Controllers
{
    [Authorize(Roles = "Admin,FinanceUser,ContractManager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalContracts = await _context.Contracts.CountAsync();
            ViewBag.ActiveContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            ViewBag.TotalServiceRequests = await _context.ServiceRequests.CountAsync();
            ViewBag.TotalRevenue = await _context.ServiceRequests.SumAsync(r => r.CostZAR);
            ViewBag.TotalClients = await _context.Clients.CountAsync();
            ViewBag.PendingRequests = await _context.ServiceRequests.CountAsync(r => r.Status == RequestStatus.Pending);

            return View();
        }

        // GET: Reports/ContractsReport
        public async Task<IActionResult> ContractsReport(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Status = status;

            return View(contracts);
        }

        // GET: Reports/FinancialReport
        public async Task<IActionResult> FinancialReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ServiceRequests
                .Include(r => r.Contract)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(r => r.RequestDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.RequestDate <= endDate.Value);

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalUSD = requests.Sum(r => r.CostUSD);
            ViewBag.TotalZAR = requests.Sum(r => r.CostZAR);

            return View(requests);
        }

        // GET: Reports/ServiceRequestsReport
        public async Task<IActionResult> ServiceRequestsReport(DateTime? startDate, DateTime? endDate, RequestStatus? status)
        {
            var query = _context.ServiceRequests
                .Include(r => r.Contract)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(r => r.RequestDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.RequestDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Status = status;

            return View(requests);
        }

        // GET: Reports/ExportContracts
        public async Task<IActionResult> ExportContracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .ToListAsync();

            // Create CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Contract #,Client,Start Date,End Date,Status,Service Level,Service Requests Count");

            foreach (var contract in contracts)
            {
                csv.AppendLine($"\"{contract.ContractNumber}\",\"{contract.ClientName}\",{contract.StartDate.ToShortDateString()},{contract.EndDate.ToShortDateString()},{contract.Status},{contract.ServiceLevel},{contract.ServiceRequests?.Count ?? 0}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Contracts_Report_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Reports/ExportFinancial
        public async Task<IActionResult> ExportFinancial(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ServiceRequests
                .Include(r => r.Contract)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(r => r.RequestDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.RequestDate <= endDate.Value);

            var requests = await query.ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Request #,Contract,Description,Cost USD,Cost ZAR,Exchange Rate,Request Date,Status");

            foreach (var request in requests)
            {
                csv.AppendLine($"\"{request.RequestNumber}\",\"{request.Contract?.ContractNumber}\",\"{request.Description}\",{request.CostUSD},{request.CostZAR},{request.ExchangeRate},{request.RequestDate.ToShortDateString()},{request.Status}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Financial_Report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}