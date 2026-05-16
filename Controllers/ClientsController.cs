using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;

namespace GMLSSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Include(c => c.Contracts)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(clients);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                client.CreatedAt = DateTime.UtcNow;

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                await LinkUserToClientAsync(client);

                TempData["Success"] = $"Client '{client.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.ClientId)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(client);
                await _context.SaveChangesAsync();

                await LinkUserToClientAsync(client);

                TempData["Success"] = $"Client '{client.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(m => m.ClientId == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Client '{client.Name}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LinkUserToClientAsync(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.ContactEmail))
                return;

            var user = await _userManager.FindByEmailAsync(client.ContactEmail);

            if (user == null)
                return;

            user.ClientId = client.ClientId;
            await _userManager.UpdateAsync(user);

            if (!await _userManager.IsInRoleAsync(user, "Client"))
            {
                await _userManager.AddToRoleAsync(user, "Client");
            }
        }
    }
}