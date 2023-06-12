namespace TCG.InvoiceService.Domain;

public class Dispute
{
    public int Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string Title { get; set; }
    public string Comment { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int DisputeStateId { get; set; }
    public DisputeState DisputeState{ get; set; }
    
}