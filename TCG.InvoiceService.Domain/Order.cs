using System.ComponentModel.DataAnnotations.Schema;

namespace TCG.InvoiceService.Domain;

public class Order
{
    public int Id { get; set; }
    public DateTime BuyDate{ get; set; } = DateTime.Now;
    public DateTime ShipDate{ get; set; }
    public DateTime DeliveryDate{ get; set; }
    public decimal ServiceFee{ get; set; }
    public decimal ShipmentFee{ get; set; }
    public decimal TotalPrice{ get; set; }
    public bool Received { get; set; }
    public int GivenFidelityPoint { get; set; }
    public string ShipAddress { get; set; }
    public Guid MerchPostId { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }

    [Column(TypeName = "char(1)")]
    public char OrderStateId { get; set; }
    public OrderState OrderState { get; set; }
    public InvoiceHead InvoiceHead { get; set; }

    public Dispute Dispute { get; set; }
}