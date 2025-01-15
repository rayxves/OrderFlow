using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderAPI.Models
{
    public class DeliveryDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime DeliveryDate { get; set; }
        public int AddressId { get; set; }


    }
}
