using System.ComponentModel.DataAnnotations;

namespace Binder_Cart.Models
{
    public class Customer:BaseModelEntity
    {
        [Required]
        public required string firstName { get; set; }
        public string? lastName { get; set; }
        [Required]
        [EmailAddress]
        public required string email { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? zipcode { get; set; }
        public string? country { get; set; }
        public string? phone { get; set; }
        [Required]

        public DateTime birthDate { get; set; }
        public string? gender { get; set; }

    } 
}
