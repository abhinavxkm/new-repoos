using EasyHousingSolution.Filters;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    [AuthorizeUser(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentSellerId()
        {
            return HttpContext.Session.GetInt32("SellerId");
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddProperty()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty(PropertyViewModel model)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var property = new Property
                {
                    PropertyName = model.PropertyName,
                    PropertyType = model.PropertyType,
                    PropertyOption = model.PropertyOption,
                    Description = model.Description,
                    Address = model.Address,
                    Region = model.Region,
                    PriceRange = model.PriceRange,
                    InitialDeposit = model.InitialDeposit,
                    Landmark = model.Landmark,
                    IsActive = false,
                    SellerId = sellerId.Value
                };
                _context.Properties.Add(property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property added successfully! It is pending verification.";
                return RedirectToAction("Dashboard");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.PropertyId == id && p.SellerId == sellerId.Value);
            if (property == null) return NotFound();

            var model = new PropertyViewModel
            {
                PropertyId = property.PropertyId,
                PropertyName = property.PropertyName,
                PropertyType = property.PropertyType,
                PropertyOption = property.PropertyOption,
                Description = property.Description,
                Address = property.Address,
                Region = property.Region,
                PriceRange = property.PriceRange,
                InitialDeposit = property.InitialDeposit,
                Landmark = property.Landmark
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(PropertyViewModel model)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var propertyToUpdate = await _context.Properties.FirstOrDefaultAsync(p => p.PropertyId == model.PropertyId && p.SellerId == sellerId.Value);
                if (propertyToUpdate == null) return NotFound();

                // CORRECTED MAPPING: Access properties directly from the model
                propertyToUpdate.PropertyName = model.PropertyName;
                propertyToUpdate.PropertyType = model.PropertyType;
                propertyToUpdate.PropertyOption = model.PropertyOption;
                propertyToUpdate.Description = model.Description;
                propertyToUpdate.Address = model.Address;
                propertyToUpdate.Region = model.Region;
                propertyToUpdate.PriceRange = model.PriceRange;
                propertyToUpdate.InitialDeposit = model.InitialDeposit;
                propertyToUpdate.Landmark = model.Landmark;

                propertyToUpdate.IsActive = false;
                propertyToUpdate.DeactivationReason = null; // Clear reason on resubmission

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property has been successfully updated and resubmitted for verification.";
                return RedirectToAction("Dashboard");

            }
            return View(model);
        }

        public async Task<IActionResult> VerifiedProperties()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");
            var properties = await _context.Properties
                .Where(p => p.SellerId == sellerId.Value && p.IsActive == true)
                .ToListAsync();
            return View(properties);
        }

        public async Task<IActionResult> PendingProperties()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");
            var properties = await _context.Properties
                .Where(p => p.SellerId == sellerId.Value && !p.IsActive && p.DeactivationReason == null)
                .ToListAsync();
            return View(properties);
        }

        public async Task<IActionResult> DeactivatedProperties()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");
            var properties = await _context.Properties
                .Where(p => p.SellerId == sellerId.Value && !p.IsActive && p.DeactivationReason != null)
                .ToListAsync();
            return View(properties);
        }
    }
}
