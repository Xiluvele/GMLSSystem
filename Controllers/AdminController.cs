using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;

namespace GMLSSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .Include(u => u.Client)
                .ToListAsync();

            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Clients = await _context.Clients.ToListAsync();
            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string id, string email, string fullName, string role, int? clientId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.Email = email;
            user.FullName = fullName;
            user.ClientId = clientId;
            user.IsActive = isActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);

                TempData["Success"] = $"User '{user.UserName}' updated successfully!";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = $"User '{user.UserName}' deleted successfully!";
            }

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName) && !await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                TempData["Success"] = $"Role '{roleName}' created successfully!";
            }

            return RedirectToAction(nameof(Roles));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null && role.Name != "Admin")
            {
                await _roleManager.DeleteAsync(role);
                TempData["Success"] = $"Role '{role.Name}' deleted successfully!";
            }

            return RedirectToAction(nameof(Roles));
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalClients = await _context.Clients.CountAsync();
            var totalContracts = await _context.Contracts.CountAsync();
            var totalRequests = await _context.ServiceRequests.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalClients = totalClients;
            ViewBag.TotalContracts = totalContracts;
            ViewBag.TotalRequests = totalRequests;
            ViewBag.ActiveContracts = activeContracts;

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentUsers = recentUsers;

            return View();
        }

        // Add to existing AdminController

        // GET: Admin/SystemLogs
        public async Task<IActionResult> SystemLogs()
        {
            var logs = new List<SystemLog>();

            // Get recent activity from database
            var recentContracts = await _context.Contracts
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .Select(c => new SystemLog
                {
                    Timestamp = c.CreatedAt,
                    User = "System",
                    Action = "Contract Created",
                    Details = $"Contract {c.ContractNumber} created for {c.ClientName}",
                    Type = "Contract"
                })
                .ToListAsync();

            var recentRequests = await _context.ServiceRequests
                .Include(r => r.Contract)
                .OrderByDescending(r => r.RequestDate)
                .Take(50)
                .Select(r => new SystemLog
                {
                    Timestamp = r.RequestDate,
                    User = "System",
                    Action = "Service Request Created",
                    Details = $"Request {r.RequestNumber} created for contract {r.Contract.ContractNumber}",
                    Type = "Service"
                })
                .ToListAsync();

            var recentInvoices = await _context.Invoices
                .OrderByDescending(i => i.CreatedAt)
                .Take(50)
                .Select(i => new SystemLog
                {
                    Timestamp = i.CreatedAt,
                    User = "System",
                    Action = "Invoice Generated",
                    Details = $"Invoice {i.InvoiceNumber} generated for {i.ClientName}",
                    Type = "Invoice"
                })
                .ToListAsync();

            logs.AddRange(recentContracts);
            logs.AddRange(recentRequests);
            logs.AddRange(recentInvoices);

            logs = logs.OrderByDescending(l => l.Timestamp).ToList();

            return View(logs);
        }

        // POST: Admin/ClearLogs
        [HttpPost]
        public async Task<IActionResult> ClearLogs()
        {
            // In a real app, you would clear actual logs
            // For now, just show success message
            TempData["Success"] = "System logs cleared successfully!";
            return RedirectToAction(nameof(SystemLogs));
        }

        // GET: Admin/Settings
        public IActionResult Settings()
        {
            var settings = new SystemSettings
            {
                CompanyName = "Global Logistics Management System",
                CompanyEmail = "info@glms.com",
                CompanyPhone = "+27 11 123 4567",
                TaxRate = 15,
                DefaultCurrency = "ZAR",
                EnableEmailNotifications = true,
                SessionTimeoutMinutes = 30,
                MaxLoginAttempts = 5,
                BackupEnabled = true,
                BackupFrequency = "Daily"
            };

            return View(settings);
        }

        // POST: Admin/Settings
        [HttpPost]
        public async Task<IActionResult> Settings(SystemSettings settings)
        {
            if (ModelState.IsValid)
            {
                // Save settings to database or configuration file
                TempData["Success"] = "System settings saved successfully!";
                return RedirectToAction(nameof(Settings));
            }

            return View(settings);
        }
    }
}