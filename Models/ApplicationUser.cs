using Microsoft.AspNetCore.Identity;

namespace Binder_Cart.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
