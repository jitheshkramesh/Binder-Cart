using System.ComponentModel.DataAnnotations;

namespace Binder_Cart.Models
{
    public class Category : BaseModelEntity
    {
        [Required]
        public required string CategoryName { get; set; }
        public string? CategoryImageUrl { get; set; }

        public string? CategoryImageLocalPath { get; set; } 
    }
}
