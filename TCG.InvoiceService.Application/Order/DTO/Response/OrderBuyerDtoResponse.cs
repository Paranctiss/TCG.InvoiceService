namespace TCG.InvoiceService.Application.Order.DTO.Response;

public class OrderBuyerDtoResponse
{
    public int Id { get; set; }
    public DateTime BuyDate{ get; set; }
    public DateTime ShipDate{ get; set; }
    public DateTime DeliveryDate{ get; set; }
    public decimal ServiceFee{ get; set; }
    public decimal ShipmentFee{ get; set; }
    public decimal TotalPrice{ get; set; }
    public decimal TotalWithShip { get; set; }
    public bool Received { get; set; }
    public string ShipAddress { get; set; }
    public string MerchPostName { get; set; }
    public decimal Type { get; set; }
    public string Username { get; set; }
    public IEnumerable<string> MerchPostNamePhotos { get; set; }
    public Guid MerchPostId { get; set; }
}