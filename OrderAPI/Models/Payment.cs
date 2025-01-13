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

    public string StripePaymentIntentId { get; set; }  
    
    public string PaymentStatus { get; set; } = "Pending";  
    public DateTime PaymentDate { get; set; }

    
}

}