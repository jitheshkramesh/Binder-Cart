using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Binder_Cart.Models
{
    public class Product:BaseModelEntity
    { 
        [Required]
        public required string ProductName { get; set; }
        public string? ProductDescription { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal ProductPrice { get; set; }
        public int? ProductStock { get; set; }
        public string? ProductImageUrl { get; set; }

        public string? ProductImageLocalPath { get; set; } 

        public virtual Category Category { get; set; }
        public virtual Brand Brand { get; set; }
    }
}
