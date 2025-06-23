using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // This helper method now takes the ViewModel as a parameter
        private void PopulateDropdowns(RegistrationViewModel model)
        {
            model.States = new SelectList(_context.States.ToList(), "StateId", "StateName");
            model.Cities = new SelectList(new List<City>(), "CityId", "CityName"); // Start with empty cities
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Create a new ViewModel instance
            var model = new RegistrationViewModel();
            // Populate the dropdown properties ON the model
            PopulateDropdowns(model);
            // Pass the entire model to the view
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            // Conditional validation for Sellers
            if (model.UserType == "Seller")
            {
                if (string.IsNullOrEmpty(model.Address)) { ModelState.AddModelError("Address", "The Address field is required for Sellers."); }
                if (!model.StateId.HasValue) { ModelState.AddModelError("StateId", "The State field is required for Sellers."); }
                if (!model.CityId.HasValue) { ModelState.AddModelError("CityId", "The City field is required for Sellers."); }
            }

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                var user = new User { /* ... create user ... */ };
                _context.Users.Add(user);

                if (model.UserType == "Seller")
                {
                    var seller = new Seller { /* ... create seller ... */ };
                    _context.Sellers.Add(seller);
                }
                else if (model.UserType == "Buyer")
                {
                    var buyer = new Buyer { /* ... create buyer ... */ };
                    _context.Buyers.Add(buyer);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login", "Login");
            }

            // If we get here, validation failed.
            // Re-populate the dropdowns ON THE MODEL before returning the view.
            PopulateDropdowns(model);
            return View(model);
        }

        [HttpGet]
        public JsonResult GetCitiesByState(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { value = c.CityId, text = c.CityName })
                .ToList();
            return new JsonResult(cities);
        }
    }
}