namespace TCG.InvoiceService.Domain;

public class InvoiceHead
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Delivery { get; set; }
    public Decimal TotalPriceHT { get; set; }
    public Decimal TotalPriceTTC { get; set; }
    public Decimal TVA { get; set; }
    public string ShipAddress { get; set; }
    public string BillAddress { get; set; }
    public int Quantity { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public ICollection<InvoiceBody> InvoiceBodies { get; set; }
}