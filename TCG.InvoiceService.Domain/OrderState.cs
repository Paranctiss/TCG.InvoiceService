using System.ComponentModel.DataAnnotations.Schema;

namespace TCG.InvoiceService.Domain;

public class OrderState
{
    [Column(TypeName = "char(1)")]
    public char Id { get; set; }

    public string StateName { get; set; }
    public ICollection<Order> Orders { get; set; }
}