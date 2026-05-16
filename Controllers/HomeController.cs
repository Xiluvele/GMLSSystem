using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;
using System.Diagnostics;

namespace GMLSSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard");

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            ViewBag.UserName = user?.FullName ?? User.Identity.Name;
            ViewBag.UserRole = userRole;

            // Common stats for all users
            var totalContracts = await _context.Contracts.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            var totalRequests = await _context.ServiceRequests.CountAsync();
            var pendingRequests = await _context.ServiceRequests.CountAsync(r => r.Status == RequestStatus.Pending);

            ViewBag.TotalContracts = totalContracts;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.TotalRequests = totalRequests;
            ViewBag.PendingRequests = pendingRequests;

            // Role-specific data
            if (userRole == "Admin")
            {
                var totalClients = await _context.Clients.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var expiringContracts = await _context.Contracts
                    .CountAsync(c => c.EndDate <= DateTime.UtcNow.AddDays(30) && c.Status == ContractStatus.Active);
                var recentActivity = await _context.ServiceRequests
                    .Include(r => r.Contract)
                    .OrderByDescending(r => r.RequestDate)
                    .Take(10)
                    .ToListAsync();

                ViewBag.TotalClients = totalClients;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.ExpiringContracts = expiringContracts;
                ViewBag.RecentActivity = recentActivity;
            }
            else if (userRole == "Client" && user.ClientId.HasValue)
            {
                var myContracts = await _context.Contracts
                    .Where(c => c.ClientId == user.ClientId.Value)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var myRequests = await _context.ServiceRequests
                    .Include(r => r.Contract)
                    .Where(r => r.Contract.ClientId == user.ClientId.Value)
                    .OrderByDescending(r => r.RequestDate)
                    .Take(10)
                    .ToListAsync();

                var myInvoices = await _context.Invoices
                    .Include(i => i.Contract)
                    .Where(i => i.Contract.ClientId == user.ClientId.Value)
                    .OrderByDescending(i => i.InvoiceDate)
                    .Take(10)
                    .ToListAsync();

                ViewBag.MyContracts = myContracts;
                ViewBag.MyRequests = myRequests;
                ViewBag.MyInvoices = myInvoices;
            }

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}