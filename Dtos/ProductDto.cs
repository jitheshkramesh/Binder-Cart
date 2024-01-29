using Binder_Cart.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binder_Cart.Dtos
{
    public class ProductDto
    { 
        public int? Id { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; } 
        public decimal? ProductPrice { get; set; }
        public int? ProductStock { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? ProductImageLocalPath { get; set; }
        public IFormFile? ProductImage { get; set; }
        public int? CategoryId { get; set; }

        [NotMapped]
        public virtual Category? Category { get; set; }
        public int? BrandId { get; set; }

        [NotMapped]
        public virtual Brand? Brand { get; set; }
    }
}
