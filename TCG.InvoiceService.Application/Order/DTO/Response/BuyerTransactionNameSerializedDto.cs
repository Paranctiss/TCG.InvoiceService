namespace TCG.InvoiceService.Application.Order.DTO.Response;

public class BuyerTransactionNameSerializedDto
{
    public Guid MerchPostId { get; set; }
    public string MerchPostName { get; set; }
    public IEnumerable<string> MerchPostNamePhotos { get; set; }
}