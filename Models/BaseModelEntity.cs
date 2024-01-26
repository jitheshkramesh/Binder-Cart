using System.ComponentModel.DataAnnotations;

namespace Binder_Cart.Models
{
    public class BaseModelEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedId { get; set; }
        public string? UpdatedId { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
