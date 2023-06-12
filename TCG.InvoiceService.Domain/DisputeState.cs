using System.ComponentModel.DataAnnotations.Schema;

namespace TCG.InvoiceService.Domain;

public class DisputeState
{
    [Column(TypeName = "char(1)")]
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Dispute> Disputes { get; set; }
}
