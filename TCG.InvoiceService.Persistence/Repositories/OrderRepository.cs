using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using TCG.Common.MySqlDb;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Persistence.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private readonly ServiceDbContext _dbContext;
    public OrderRepository(ServiceDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Order>> GetBuyerTransaction(CancellationToken cancellationToken, int buyerId)
    {
        return await _dbContext.Orders.Where(u => u.BuyerId == buyerId).ToListAsync();
    }
    
    public async Task<IEnumerable<Order>> GetSellerTransaction(CancellationToken cancellationToken, int sellerId)
    {
        return await _dbContext.Orders.Where(u => u.SellerId == sellerId).ToListAsync();
    }

}
