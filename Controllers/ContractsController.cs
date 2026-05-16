using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;

namespace GMLSSystem.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContractsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var contracts = await GetContractsForCurrentUser();

            return View(contracts);
        }

        [Authorize(Roles = "Admin,ContractManager")]
        public IActionResult Create()
        {
            ViewBag.Clients = _context.Clients.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile signedAgreement)
        {
            if (ModelState.IsValid)
            {
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    var fileResult = await SaveSignedAgreementAsync(signedAgreement);

                    if (!fileResult.Success)
                    {
                        ModelState.AddModelError("signedAgreement", fileResult.ErrorMessage);
                        ViewBag.Clients = _context.Clients.ToList();
                        return View(contract);
                    }

                    contract.SignedAgreementPath = fileResult.FilePath;
                    contract.OriginalFileName = fileResult.OriginalFileName;
                }

                var client = await _context.Clients.FindAsync(contract.ClientId);

                contract.ClientName = client?.Name;
                contract.CreatedAt = DateTime.UtcNow;
                contract.ContractNumber = GenerateContractNumber();

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Contract '{contract.ContractNumber}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = _context.Clients.ToList();
            return View(contract);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Client"))
            {
                if (user == null || !user.ClientId.HasValue || contract.ClientId != user.ClientId.Value)
                    return Unauthorized();
            }

            return View(contract);
        }

        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound();

            ViewBag.Clients = _context.Clients.ToList();
            return View(contract);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.ContractId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingContract = await _context.Contracts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (existingContract == null)
                    return NotFound();

                contract.SignedAgreementPath = existingContract.SignedAgreementPath;
                contract.OriginalFileName = existingContract.OriginalFileName;
                contract.ContractNumber = existingContract.ContractNumber;
                contract.CreatedAt = existingContract.CreatedAt;

                var client = await _context.Clients.FindAsync(contract.ClientId);
                contract.ClientName = client?.Name;

                _context.Update(contract);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Contract '{contract.ContractNumber}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = _context.Clients.ToList();
            return View(contract);
        }

        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> UpdateStatus(int id, ContractStatus status)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound();

            contract.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Contract status updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin,ContractManager,Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignedAgreement(int contractId, IFormFile signedAgreement)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Client"))
            {
                if (user == null || !user.ClientId.HasValue || contract.ClientId != user.ClientId.Value)
                    return Unauthorized();
            }

            var fileResult = await SaveSignedAgreementAsync(signedAgreement);

            if (!fileResult.Success)
            {
                TempData["Error"] = fileResult.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id = contractId });
            }

            contract.SignedAgreementPath = fileResult.FilePath;
            contract.OriginalFileName = fileResult.OriginalFileName;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Signed agreement uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id = contractId });
        }

        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Client"))
            {
                if (user == null || !user.ClientId.HasValue)
                    return Unauthorized();

                query = query.Where(c => c.ClientId == user.ClientId.Value);
            }

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return PartialView("_ContractList", contracts);
        }

        public async Task<IActionResult> MyContracts()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || !user.ClientId.HasValue)
            {
                TempData["Error"] = "Your account is not linked to a client profile.";
                return RedirectToAction("Dashboard", "Home");
            }

            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.ClientId == user.ClientId.Value)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View("Index", contracts);
        }

        public async Task<IActionResult> ActiveContracts()
        {
            var contracts = await GetContractsByStatusForCurrentUser(ContractStatus.Active);
            ViewBag.FilterType = "Active Contracts";

            return View("Index", contracts);
        }

        public async Task<IActionResult> ExpiringSoon()
        {
            var expiringThreshold = DateTime.UtcNow.AddDays(30);

            var query = _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active && c.EndDate <= expiringThreshold);

            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Client"))
            {
                if (user == null || !user.ClientId.HasValue)
                    return Unauthorized();

                query = query.Where(c => c.ClientId == user.ClientId.Value);
            }

            var contracts = await query
                .OrderBy(c => c.EndDate)
                .ToListAsync();

            ViewBag.FilterType = "Contracts Expiring Soon (Next 30 Days)";
            return View("Index", contracts);
        }

        public async Task<IActionResult> ExpiredContracts()
        {
            var contracts = await GetContractsByStatusForCurrentUser(ContractStatus.Expired);
            ViewBag.FilterType = "Expired Contracts";

            return View("Index", contracts);
        }

        public async Task<IActionResult> DraftContracts()
        {
            var contracts = await GetContractsByStatusForCurrentUser(ContractStatus.Draft);
            ViewBag.FilterType = "Draft Contracts";

            return View("Index", contracts);
        }

        public async Task<IActionResult> OnHoldContracts()
        {
            var contracts = await GetContractsByStatusForCurrentUser(ContractStatus.OnHold);
            ViewBag.FilterType = "On Hold Contracts";

            return View("Index", contracts);
        }

        private async Task<List<Contract>> GetContractsForCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (User.IsInRole("Client"))
            {
                if (user == null || !user.ClientId.HasValue)
                    return new List<Contract>();

                query = query.Where(c => c.ClientId == user.ClientId.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        private async Task<List<Contract>> GetContractsByStatusForCurrentUser(ContractStatus status)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == status);

            if (User.IsInRole("Client"))
            {
                if (user == null || !user.ClientId.HasValue)
                    return new List<Contract>();

                query = query.Where(c => c.ClientId == user.ClientId.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        private async Task<FileUploadResult> SaveSignedAgreementAsync(IFormFile signedAgreement)
        {
            if (signedAgreement == null || signedAgreement.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Please select a PDF file."
                };
            }

            var extension = Path.GetExtension(signedAgreement.FileName).ToLower();

            if (extension != ".pdf")
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Only PDF files are allowed."
                };
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "contracts"
            );

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var safeFileName = Path.GetFileName(signedAgreement.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await signedAgreement.CopyToAsync(stream);
            }

            return new FileUploadResult
            {
                Success = true,
                FilePath = $"/uploads/contracts/{uniqueFileName}",
                OriginalFileName = safeFileName
            };
        }

        private string GenerateContractNumber()
        {
            return $"CTR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private class FileUploadResult
        {
            public bool Success { get; set; }
            public string? FilePath { get; set; }
            public string? OriginalFileName { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}