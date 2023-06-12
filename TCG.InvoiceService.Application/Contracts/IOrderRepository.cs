using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCG.Common.Contracts;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Application.Contracts;

public interface IOrderRepository : IRepository<Domain.Order>
{
    
}


