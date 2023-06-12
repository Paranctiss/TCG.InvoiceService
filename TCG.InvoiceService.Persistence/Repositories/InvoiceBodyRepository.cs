using Microsoft.EntityFrameworkCore;
using TCG.Common.MySqlDb;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Persistence.Repositories;

public class InvoiceBodyRepository : Repository<InvoiceBody>, IInvoiceBodyRepository
{
    public InvoiceBodyRepository(ServiceDbContext dbContext) : base(dbContext)
    {
    }
}