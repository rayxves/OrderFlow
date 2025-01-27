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
        private DateTime _deliveryDate;
        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set => _deliveryDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public int AddressId { get; set; }
        public Address Address { get; set; }

    }
}
