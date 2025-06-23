using EasyHousingSolution.Filters;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    [AuthorizeUser(Roles = "Buyer")] 
    public class BuyerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BuyerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CORRECTED: Helper now gets the integer ID from the session
        private int? GetCurrentBuyerId()
        {
            return HttpContext.Session.GetInt32("BuyerId");
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Search(string region = "", string type = "", string sortOrder = "name")
        {
            var properties = _context.Properties.Include(p => p.Seller)
                .Where(p => p.IsActive == true &&
                              (string.IsNullOrEmpty(region) || p.Address.Contains(region)) &&
                              (string.IsNullOrEmpty(type) || p.PropertyType == type));

            properties = sortOrder == "price"
                ? properties.OrderBy(p => p.PriceRange)
                : properties.OrderBy(p => p.PropertyName);

            return View(properties.ToList());
        }

        public async Task<IActionResult> PropertyDetails(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Seller) // Include the Seller's data
                .Include(p => p.Images) // It loads all related images.
                .FirstOrDefaultAsync(p => p.PropertyId == id && p.IsActive == true);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int propertyId)
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null)
            {
                return new JsonResult(new { success = false, error = "User not authenticated." });
            }

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.PropertyId == propertyId && c.BuyerId == buyerId.Value);

            if (existingCartItem == null)
            {
                var cartItem = new Cart { PropertyId = propertyId, BuyerId = buyerId.Value };
                _context.Carts.Add(cartItem);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> ViewCart()
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null) return RedirectToAction("Login", "Login");

            var cartItems = await _context.Carts
                .Where(c => c.BuyerId == buyerId.Value)
                .Include(c => c.Property) // Eager load property details
                .Select(c => new CartViewModel
                {
                    CartId = c.CartId,
                    PropertyName = c.Property.PropertyName,
                    PropertyType = c.Property.PropertyType,
                    PropertyOption = c.Property.PropertyOption,
                    PriceRange = c.Property.PriceRange,
                    PropertyId = c.Property.PropertyId
                })
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null) return RedirectToAction("Login", "Login");

            var item = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == cartId && c.BuyerId == buyerId.Value);
            if (item != null)
            {
                _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ViewCart");
        }
        // Add this new method to BuyerController.cs

        [HttpPost]
        public IActionResult Compare(int[] ids)
        {
            // Check if the user selected exactly two properties
            if (ids == null || ids.Length != 2)
            {
                TempData["ErrorMessage"] = "Please select exactly two properties to compare.";
                return RedirectToAction("Search");
            }

            var propertiesToCompare = _context.Properties
                                              .Include(p => p.Seller)
                                              .Where(p => ids.Contains(p.PropertyId))
                                              .ToList();

            // The View will handle displaying the two properties
            return View(propertiesToCompare);
        }
    }
}