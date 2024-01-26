using Binder_Cart.Models; 

namespace Binder_Cart.Dtos
{
    public class ProductDto
    { 
        public required string ProductName { get; set; }
        public string? ProductDescription { get; set; } 
        public decimal ProductPrice { get; set; }
        public int? ProductStock { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? ProductImageLocalPath { get; set; }
        public IFormFile? ProductImage { get; set; }

        public virtual Category Category { get; set; }
        public virtual Brand Brand { get; set; }
    }
}
