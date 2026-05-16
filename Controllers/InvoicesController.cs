using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Data;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;
using GMLSSystem.Services.Builders;
using GMLSSystem.Services.Factories;

namespace GMLSSystem.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoicesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            List<Invoice> invoices;

            if (userRole == "Client" && user.ClientId.HasValue)
            {
                invoices = await _context.Invoices
                    .Include(i => i.Contract)
                    .Where(i => i.Contract.ClientId == user.ClientId.Value)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();
            }
            else
            {
                invoices = await _context.Invoices
                    .Include(i => i.Contract)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();
            }

            return View(invoices);
        }

        // GET: Invoices/Generate
        [Authorize(Roles = "Admin,FinanceUser")]
        public async Task<IActionResult> Generate(int? contractId)
        {
            ViewBag.Contracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .OrderBy(c => c.ClientName)
                .ToListAsync();

            if (contractId.HasValue)
            {
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .FirstOrDefaultAsync(c => c.ContractId == contractId.Value);

                if (contract != null)
                {
                    ViewBag.SelectedContract = contract;
                }
            }

            return View();
        }

        // POST: Invoices/Generate
        [HttpPost]
        [Authorize(Roles = "Admin,FinanceUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int contractId, List<InvoiceItem> items, decimal taxRate, DateTime dueDate, string notes)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
            {
                TempData["Error"] = "Contract not found";
                return RedirectToAction("Generate");
            }

            // Filter out empty items
            var validItems = items?.Where(i => !string.IsNullOrEmpty(i.Description) && i.Quantity > 0 && i.UnitPrice > 0).ToList();

            if (validItems == null || !validItems.Any())
            {
                TempData["Error"] = "Please add at least one invoice item with valid description, quantity and price";
                return RedirectToAction("Generate");
            }

            // Use Builder Pattern to create invoice
            var builder = new InvoiceBuilder();

            // Start building the invoice
            var invoiceBuilder = builder
                .SetClient(contract.ClientName)
                .SetTaxRate(taxRate)
                .SetDueDate(dueDate)
                .SetCurrency(contract.Region == "International" ? "USD" : "ZAR");

            // Add all items
            foreach (var item in validItems)
            {
                invoiceBuilder.AddItem(item.Description, item.Quantity, item.UnitPrice);
            }

            // Build the invoice
            var invoice = invoiceBuilder.Build();
            invoice.ContractId = contractId;
            invoice.Notes = notes;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Invoice {invoice.InvoiceNumber} created successfully!";
            return RedirectToAction("Details", new { id = invoice.InvoiceId });
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            // Check if client has access
            if (userRole == "Client" && (!user.ClientId.HasValue || invoice.Contract.ClientId != user.ClientId.Value))
                return Unauthorized();

            return View(invoice);
        }

        // POST: Invoices/UpdateStatus
        [Authorize(Roles = "Admin,FinanceUser")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            if (Enum.TryParse<InvoiceStatus>(status, out var newStatus))
            {
                invoice.Status = newStatus;
                if (newStatus == InvoiceStatus.Paid)
                {
                    invoice.PaidAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Invoice {invoice.InvoiceNumber} status updated to {newStatus}";
            }

            return RedirectToAction("Details", new { id });
        }

        // GET: Invoices/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        // GET: Invoices/MyInvoices (for clients)
        public async Task<IActionResult> MyInvoices()
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.ClientId.HasValue)
                return Unauthorized();

            var invoices = await _context.Invoices
                .Include(i => i.Contract)
                .Where(i => i.Contract.ClientId == user.ClientId.Value)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return View("Index", invoices);
        }

        // GET: Invoices/UnpaidInvoices
        [Authorize(Roles = "Admin,FinanceUser")]
        public async Task<IActionResult> UnpaidInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Contract)
                .Where(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Draft)
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            ViewBag.FilterType = "Unpaid Invoices";
            return View("Index", invoices);
        }

        // GET: Invoices/OverdueInvoices
        [Authorize(Roles = "Admin,FinanceUser")]
        public async Task<IActionResult> OverdueInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Contract)
                .Where(i => i.Status != InvoiceStatus.Paid && i.DueDate < DateTime.UtcNow)
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            // Update overdue status
            foreach (var invoice in invoices.Where(i => i.Status != InvoiceStatus.Overdue))
            {
                invoice.Status = InvoiceStatus.Overdue;
            }
            await _context.SaveChangesAsync();

            ViewBag.FilterType = "Overdue Invoices";
            return View("Index", invoices);
        }

        // GET: Invoices/CreateFromContract/5
        [Authorize(Roles = "Admin,FinanceUser")]
        public async Task<IActionResult> CreateFromContract(int contractId)
        {
            return RedirectToAction("Generate", new { contractId });
        }
    }
}