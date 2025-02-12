using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderAPI.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string PaymentUrl { get; set; } = string.Empty;
        public string StripeSessionId { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; }


    }

}