using EasyHousingSolution.Filters;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    [AuthorizeUser(Roles = "Admin")] // Secures the entire controller for Admins only
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Fetch stats for the cards
            ViewBag.PendingCount = await _context.Properties.CountAsync(p => !p.IsActive);
            ViewBag.LiveCount = await _context.Properties.CountAsync(p => p.IsActive);
            ViewBag.SellerCount = await _context.Sellers.CountAsync();
            ViewBag.BuyerCount = await _context.Buyers.CountAsync();

            // Fetch the 5 most recent pending properties for the table
            var recentPending = await _context.Properties
                .Where(p => !p.IsActive && p.DeactivationReason == null)
                .Include(p => p.Seller)
                .OrderByDescending(p => p.PropertyId)
                .Take(5)
                .ToListAsync();

            return View(recentPending);
        }

        public async Task<IActionResult> PendingProperties()
        {
            var pending = await _context.Properties
                .Where(p => p.IsActive == false && p.DeactivationReason == null) // Only show truly pending properties
                .Include(p => p.Seller)
                .ToListAsync();
            return View(pending);
        }

        public async Task<IActionResult> VerifiedProperties(int? sellerId, string? region)
        {
            var query = _context.Properties
                .Include(p => p.Seller)
                .Where(p => p.IsActive == true)
                .AsQueryable();

            if (sellerId.HasValue)
            {
                query = query.Where(p => p.SellerId == sellerId.Value);
            }

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.Region.Contains(region));
            }

            ViewBag.Sellers = new SelectList(await _context.Sellers.ToListAsync(), "SellerId", "UserName");
            ViewData["CurrentSellerId"] = sellerId;
            ViewData["CurrentRegion"] = region;

            var properties = await query.ToListAsync();
            return View(properties);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                property.IsActive = true;
                property.DeactivationReason = null; // Clear any previous rejection reason
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Property '{property.PropertyName}' has been approved.";
            }
            // This is the fix: Redirect to the page to show the updated list.
            return RedirectToAction(nameof(PendingProperties));
        }


        [HttpGet]
        public async Task<IActionResult> Deactivate(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }
            return View(property); // Pass the property object to the Deactivate reason form
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int propertyId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                ModelState.AddModelError("Reason", "A reason for deactivation is required.");
                // If reason is empty, reload the form with the property data
                var propertyForView = await _context.Properties.FindAsync(propertyId);
                return View(propertyForView);
            }

            var property = await _context.Properties.FindAsync(propertyId);
            if (property != null)
            {
                property.IsActive = false;
                property.DeactivationReason = reason; // Save the reason
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Property '{property.PropertyName}' has been deactivated.";
            }
            // This is the fix: Redirect to the list of live properties
            return RedirectToAction(nameof(VerifiedProperties));
        }
    }
}
