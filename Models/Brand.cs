using System.ComponentModel.DataAnnotations;

namespace Binder_Cart.Models
{
    public class Brand : BaseModelEntity
    {
        [Required]
        public required string BrandName { get; set; }
        public string? BrandImageUrl { get; set; }

        public string? BrandImageLocalPath { get; set; } 
    }
}
