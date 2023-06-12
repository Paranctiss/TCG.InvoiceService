using TCG.Common.MySqlDb;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Persistence.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ServiceDbContext dbContext) : base(dbContext)
    {
    }
}
