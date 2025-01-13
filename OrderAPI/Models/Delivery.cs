using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderAPI.Models
{
    public class Delivery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Status { get; set; } = "Pending";  
        public DateTime DeliveryDate { get; set; }

        public int AddressId { get; set; }
        public Address Address { get; set; }
        
    }
}
