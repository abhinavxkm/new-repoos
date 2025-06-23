using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyHousingSolution.Models
{
    public class UploadImageViewModel
    {
        [Required]
        public int PropertyId { get; set; }

        // This is not submitted, just used to display on the page
        public string PropertyName { get; set; }

        [Required(ErrorMessage = "Please select at least one image.")]
        [Display(Name = "Select Images (Max 6)")]
        public List<IFormFile> Images { get; set; }
    }
}