using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMLSSystem.Shared.Models;
using GMLSSystem.Services.Api;

namespace GMLSSystem.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IApiClient apiClient, ILogger<ContractsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<List<ContractDto>>>("api/contracts");

                if (response.Success && response.Data != null)
                    return View(response.Data);

                TempData["Error"] = response.Message;
                return View(new List<ContractDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contracts");
                TempData["Error"] = "Could not load contracts. Please try again.";
                return View(new List<ContractDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<ContractDto>>($"api/contracts/{id}");

                if (response.Success && response.Data != null)
                    return View(response.Data);

                TempData["Error"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract {Id}", id);
                TempData["Error"] = "Could not load contract details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> Create()
        {
            await LoadClientsAsync();
            return View(new CreateContractDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractDto contract, IFormFile? signedAgreement)
        {
            if (!ModelState.IsValid)
            {
                await LoadClientsAsync();
                return View(contract);
            }

            try
            {
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    var extension = Path.GetExtension(signedAgreement.FileName).ToLower();

                    if (extension != ".pdf")
                    {
                        ModelState.AddModelError("signedAgreement", "Only PDF files are allowed.");
                        await LoadClientsAsync();
                        return View(contract);
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

                    contract.SignedAgreementPath = $"/uploads/contracts/{uniqueFileName}";
                    contract.OriginalFileName = safeFileName;
                }

                var response = await _apiClient.PostAsync<ApiResponse<ContractDto>>("api/contracts", contract);

                if (response.Success)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", response.Message ?? "Could not create contract.");
                await LoadClientsAsync();
                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                ModelState.AddModelError("", "Could not create contract. Please try again.");
                await LoadClientsAsync();
                return View(contract);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var updateDto = new UpdateContractStatusDto
                {
                    Status = status
                };

                var response = await _apiClient.PatchAsync<ApiResponse<object>>(
                    $"api/contracts/{id}/status",
                    updateDto
                );

                TempData[response.Success ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract status");
                TempData["Error"] = "Could not update contract status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string? status)
        {
            try
            {
                var query = new List<string>();

                if (startDate.HasValue)
                    query.Add($"startDate={startDate.Value:yyyy-MM-dd}");

                if (endDate.HasValue)
                    query.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Add($"status={Uri.EscapeDataString(status)}");

                var queryString = query.Any() ? "?" + string.Join("&", query) : "";

                var response = await _apiClient.GetAsync<ApiResponse<List<ContractDto>>>(
                    $"api/contracts{queryString}"
                );

                if (response.Success && response.Data != null)
                    return PartialView("_ContractList", response.Data);

                return PartialView("_ContractList", new List<ContractDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching contracts");
                return PartialView("_ContractList", new List<ContractDto>());
            }
        }

        public async Task<IActionResult> ActiveContracts()
        {
            return await FilterByStatus("Active", "Active Contracts");
        }

        public async Task<IActionResult> ExpiredContracts()
        {
            return await FilterByStatus("Expired", "Expired Contracts");
        }

        public async Task<IActionResult> DraftContracts()
        {
            return await FilterByStatus("Draft", "Draft Contracts");
        }

        public async Task<IActionResult> OnHoldContracts()
        {
            return await FilterByStatus("OnHold", "On Hold Contracts");
        }

        public async Task<IActionResult> ExpiringSoon()
        {
            try
            {
                var today = DateTime.UtcNow;
                var next30Days = today.AddDays(30);

                var response = await _apiClient.GetAsync<ApiResponse<List<ContractDto>>>(
                    $"api/contracts?startDate={today:yyyy-MM-dd}&endDate={next30Days:yyyy-MM-dd}&status=Active"
                );

                ViewBag.FilterType = "Contracts Expiring Soon (Next 30 Days)";

                if (response.Success && response.Data != null)
                    return View("Index", response.Data);

                return View("Index", new List<ContractDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expiring contracts");
                return View("Index", new List<ContractDto>());
            }
        }

        private async Task<IActionResult> FilterByStatus(string status, string filterName)
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<List<ContractDto>>>(
                    $"api/contracts?status={status}"
                );

                ViewBag.FilterType = filterName;

                if (response.Success && response.Data != null)
                    return View("Index", response.Data);

                return View("Index", new List<ContractDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering contracts by status {Status}", status);
                return View("Index", new List<ContractDto>());
            }
        }

        private async Task LoadClientsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<List<ClientDto>>>("api/clients");

                ViewBag.Clients = response.Success && response.Data != null
                    ? response.Data
                    : new List<ClientDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients");
                ViewBag.Clients = new List<ClientDto>();
            }
        }
    }
}