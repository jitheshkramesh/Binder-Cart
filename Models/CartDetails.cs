using Binder_Cart.Dtos;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Binder_Cart.Models
{
    public class CartDetails
    {
        [Key]
        public int CartDetailsId { get; set; }

        public int CartHeaderId { get; set; }

        [NotMapped]
        public CartHeader CartHeader { get; set; }
        public int ProductId { get; set; }
        [NotMapped]
        public ProductDto Product { get; set; }
        public int Count { get; set; }
    }
}
