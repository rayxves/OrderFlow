using Microsoft.AspNetCore.Identity;

namespace OrderAPI.Models
{
    public class User : IdentityUser
    {
        public string Phone { get; set; } = string.Empty;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}