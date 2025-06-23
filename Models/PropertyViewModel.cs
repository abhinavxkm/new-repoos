using System.ComponentModel.DataAnnotations;

namespace EasyHousingSolution.Models
{
    public class PropertyViewModel
    {
        public int PropertyId { get; set; }

        [Required, MaxLength(50)]
        public string PropertyName { get; set; }

        [Required, MaxLength(15)]
        public string PropertyType { get; set; }

        [Required, MaxLength(10)]
        public string PropertyOption { get; set; } // Sell or Rent

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required, MaxLength(250)]
        public string Address { get; set; }

        [Required, MaxLength(50)]
        public string Region { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal PriceRange { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Initial Deposit cannot be negative.")]
        public decimal? InitialDeposit { get; set; } // For Rent only

        [Required, MaxLength(25)]
        public string Landmark { get; set; }

        // SellerId has been REMOVED as it is handled by the session.
    }
}