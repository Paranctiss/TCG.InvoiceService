namespace TCG.InvoiceService.Domain;

public class InvoiceBody
{
    public int Id { get; set; }
    public int Line { get; set; }
    public string Title { get; set; }
    public Decimal Price { get; set; }
    public int Reduction { get; set; }

    public int InvoiceHeadId { get; set; }
    public InvoiceHead InvoiceHead { get; set; }
}