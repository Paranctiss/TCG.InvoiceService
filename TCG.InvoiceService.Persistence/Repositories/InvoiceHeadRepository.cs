using Microsoft.EntityFrameworkCore;
using TCG.Common.MySqlDb;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Persistence.Repositories;

public class InvoiceHeadRepository : Repository<InvoiceHead>, IInvoiceHeadRepository
{
    public InvoiceHeadRepository(ServiceDbContext dbContext) : base(dbContext)
    {
    }
}