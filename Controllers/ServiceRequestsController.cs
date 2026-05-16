using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;  // Make sure this is included for RequestStatus
using GMLSSystem.Services;

namespace GMLSSystem.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICurrencyService currencyService)
        {
            _context = context;
            _userManager = userManager;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            List<ServiceRequest> requests;

            if (userRole == "Client" && user.ClientId.HasValue)
            {
                requests = await _context.ServiceRequests
                    .Include(r => r.Contract)
                    .Where(r => r.Contract.ClientId == user.ClientId.Value)
                    .OrderByDescending(r => r.RequestDate)
                    .ToListAsync();
            }
            else
            {
                requests = await _context.ServiceRequests
                    .Include(r => r.Contract)
                    .OrderByDescending(r => r.RequestDate)
                    .ToListAsync();
            }

            return View(requests);
        }

        // GET: ServiceRequests/Search
        // GET: ServiceRequests/Search
        public async Task<IActionResult> Search(RequestStatus? status, string searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var query = _context.ServiceRequests
                .Include(r => r.Contract)
                .AsQueryable();

            // Filter by client role
            if (userRole == "Client" && user.ClientId.HasValue)
            {
                query = query.Where(r => r.Contract.ClientId == user.ClientId.Value);
            }

            // Filter by status - this handles the enum correctly
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // Filter by search term (request number or description)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.RequestNumber.Contains(searchTerm) ||
                    r.Description.Contains(searchTerm) ||
                    r.Contract.ContractNumber.Contains(searchTerm));
            }

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return PartialView("_RequestList", requests);
        }
        [Authorize(Roles = "Admin,LogisticsCoordinator")]
        public async Task<IActionResult> Create(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                return NotFound();

            // Check if contract allows service requests (only Active contracts)
            if (!contract.canCreateServiceRequest())
            {
                TempData["Error"] = $"Cannot create service request. Contract status is {contract.Status}";
                return RedirectToAction("Details", "Contracts", new { id = contractId });
            }

            ViewBag.Contract = contract;
            ViewBag.CurrentRate = await _currencyService.GetExchangeRate("USD", "ZAR");

            return View(new ServiceRequest { ContractId = contractId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,LogisticsCoordinator")]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest, decimal costUSD)
        {
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

            if (contract == null)
                return NotFound();

            // Check if contract allows service requests (only Active contracts)
            if (!contract.canCreateServiceRequest())
            {
                ModelState.AddModelError("", "Cannot create service request for expired or on-hold contracts");
                ViewBag.Contract = contract;
                ViewBag.CurrentRate = await _currencyService.GetExchangeRate("USD", "ZAR");
                return View(serviceRequest);
            }

            if (ModelState.IsValid)
            {
                var rate = await _currencyService.GetExchangeRate("USD", "ZAR");
                serviceRequest.CostUSD = costUSD;
                serviceRequest.CostZAR = costUSD * rate;
                serviceRequest.ExchangeRate = rate;
                serviceRequest.Status = RequestStatus.Pending;
                serviceRequest.RequestDate = DateTime.UtcNow;

                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Service request created successfully! Cost: ${costUSD:F2} USD / R{serviceRequest.CostZAR:F2} ZAR";
                return RedirectToAction("Details", "Contracts", new { id = contract.ContractId });
            }

            ViewBag.Contract = contract;
            ViewBag.CurrentRate = await _currencyService.GetExchangeRate("USD", "ZAR");
            return View(serviceRequest);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(r => r.ServiceRequestId == id);

            if (request == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole == "Client" && (!user.ClientId.HasValue || request.Contract.ClientId != user.ClientId.Value))
                return Unauthorized();

            return View(request);
        }

        [Authorize(Roles = "Admin,LogisticsCoordinator")]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatus status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = status;

            if (status == RequestStatus.Completed)
                request.CompletionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Service request status updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            var rate = await _currencyService.GetExchangeRate(fromCurrency, toCurrency);
            return Json(new { rate });
        }

        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.ClientId.HasValue)
                return Unauthorized();

            var requests = await _context.ServiceRequests
                .Include(r => r.Contract)
                .Where(r => r.Contract.ClientId == user.ClientId.Value)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View("Index", requests);
        }
    }
}