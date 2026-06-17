using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GMLSSystem.Models;
using GMLSSystem.Services.Api;
using GMLSSystem.Shared.Models;

namespace GMLSSystem.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            IApiClient apiClient,
            UserManager<ApplicationUser> userManager,
            ILogger<ServiceRequestsController> logger)
        {
            _apiClient = apiClient;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<List<ServiceRequestDto>>>("api/servicerequests");

                if (response.Success && response.Data != null)
                    return View(response.Data);

                TempData["Error"] = response.Message;
                return View(new List<ServiceRequestDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service requests");
                TempData["Error"] = "Could not load service requests.";
                return View(new List<ServiceRequestDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<ServiceRequestDto>>($"api/servicerequests/{id}");

                if (response.Success && response.Data != null)
                    return View(response.Data);

                TempData["Error"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service request {Id}", id);
                TempData["Error"] = "Could not load service request details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,LogisticsCoordinator")]
        public async Task<IActionResult> Create(int contractId)
        {
            try
            {
                var contractResponse = await _apiClient.GetAsync<ApiResponse<ContractDto>>($"api/contracts/{contractId}");

                if (!contractResponse.Success || contractResponse.Data == null)
                {
                    TempData["Error"] = contractResponse.Message ?? "Contract not found.";
                    return RedirectToAction("Index", "Contracts");
                }

                var contract = contractResponse.Data;

                if (contract.Status != "Active")
                {
                    TempData["Error"] = $"Cannot create service request. Contract status is {contract.Status}.";
                    return RedirectToAction("Details", "Contracts", new { id = contractId });
                }

                ViewBag.Contract = contract;
                ViewBag.CurrentRate = await GetExchangeRateAsync();

                return View(new CreateServiceRequestDto
                {
                    ContractId = contractId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create service request form");
                TempData["Error"] = "Could not load create form.";
                return RedirectToAction("Index", "Contracts");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,LogisticsCoordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                await ReloadCreateViewDataAsync(request.ContractId);
                return View(request);
            }

            try
            {
                var response = await _apiClient.PostAsync<ApiResponse<ServiceRequestDto>>(
                    "api/servicerequests",
                    request
                );

                if (response.Success)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Details", "Contracts", new { id = request.ContractId });
                }

                ModelState.AddModelError("", response.Message ?? "Could not create service request.");
                await ReloadCreateViewDataAsync(request.ContractId);
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                ModelState.AddModelError("", "Could not create service request. Please try again.");
                await ReloadCreateViewDataAsync(request.ContractId);
                return View(request);
            }
        }

        // POST: ServiceRequests/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Admin,LogisticsCoordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var updateDto = new UpdateServiceRequestStatusDto
                {
                    Status = status
                };

                var response = await _apiClient.PatchAsync<ApiResponse<object>>(
                    $"api/servicerequests/{id}/status",
                    updateDto
                );

                TempData[response.Success ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request status");
                TempData["Error"] = "Could not update request status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Search(string? status, string? searchTerm)
        {
            try
            {
                var query = new List<string>();

                if (!string.IsNullOrWhiteSpace(status))
                    query.Add($"status={Uri.EscapeDataString(status)}");

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

                var queryString = query.Any() ? "?" + string.Join("&", query) : "";

                var response = await _apiClient.GetAsync<ApiResponse<List<ServiceRequestDto>>>(
                    $"api/servicerequests{queryString}"
                );

                if (response.Success && response.Data != null)
                    return PartialView("_RequestList", response.Data);

                return PartialView("_RequestList", new List<ServiceRequestDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching service requests");
                return PartialView("_RequestList", new List<ServiceRequestDto>());
            }
        }

        public async Task<IActionResult> MyRequests()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.ClientId.HasValue)
                {
                    TempData["Error"] = "No client associated with your account.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await _apiClient.GetAsync<ApiResponse<List<ServiceRequestDto>>>(
                    $"api/servicerequests?clientId={user.ClientId.Value}"
                );

                if (response.Success && response.Data != null)
                    return View("Index", response.Data);

                return View("Index", new List<ServiceRequestDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading my requests");
                return View("Index", new List<ServiceRequestDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            return Json(new { rate });
        }

        private async Task ReloadCreateViewDataAsync(int contractId)
        {
            try
            {
                var contractResponse = await _apiClient.GetAsync<ApiResponse<ContractDto>>($"api/contracts/{contractId}");

                ViewBag.Contract = contractResponse.Success
                    ? contractResponse.Data
                    : null;

                ViewBag.CurrentRate = await GetExchangeRateAsync();
            }
            catch
            {
                ViewBag.Contract = null;
                ViewBag.CurrentRate = 18.50m;
            }
        }

        private async Task<decimal> GetExchangeRateAsync(string fromCurrency = "USD", string toCurrency = "ZAR")
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<decimal>>(
                    $"api/currency/rate?fromCurrency={fromCurrency}&toCurrency={toCurrency}"
                );

                if (response.Success)
                    return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load exchange rate from API. Using fallback rate.");
            }

            return 18.50m;
        }
    }
}