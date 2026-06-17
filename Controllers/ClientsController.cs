using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMLSSystem.Shared.Models;
using GMLSSystem.Services.Api;

namespace GMLSSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IApiClient apiClient, ILogger<ClientsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<List<ClientDto>>>("api/clients");
                if (response.Success)
                {
                    return View(response.Data);
                }
                TempData["Error"] = response.Message;
                return View(new List<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients");
                TempData["Error"] = "Could not load clients.";
                return View(new List<ClientDto>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientDto client)
        {
            try
            {
                var response = await _apiClient.PostAsync<ApiResponse<ClientDto>>("api/clients", client);
                if (response.Success)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", response.Message);
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                ModelState.AddModelError("", "Could not create client.");
                return View(client);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<ClientDto>>($"api/clients/{id}");
                if (response.Success)
                {
                    return View(response.Data);
                }
                TempData["Error"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client for edit");
                TempData["Error"] = "Could not load client.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientDto client)
        {
            try
            {
                var response = await _apiClient.PutAsync<ApiResponse<ClientDto>>($"api/clients/{id}", client);
                if (response.Success)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", response.Message);
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client");
                ModelState.AddModelError("", "Could not update client.");
                return View(client);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiResponse<ClientDto>>($"api/clients/{id}");
                if (response.Success)
                {
                    return View(response.Data);
                }
                TempData["Error"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client for delete");
                TempData["Error"] = "Could not load client.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _apiClient.DeleteAsync($"api/clients/{id}");
                if (success)
                {
                    TempData["Success"] = "Client deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Could not delete client.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client");
                TempData["Error"] = "Could not delete client.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}