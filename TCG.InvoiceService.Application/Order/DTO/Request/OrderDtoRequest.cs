using System.ComponentModel.DataAnnotations.Schema;

namespace TCG.InvoiceService.Application.Order.DTO.Request;

public class OrderDtoRequest
{
    public string ShipAddress { get; set; }
    public Guid MerchPostId { get; set; }
    public decimal ShipmentFee { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public char StateId { get; set; }
}