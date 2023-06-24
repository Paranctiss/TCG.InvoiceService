using MediatR;
using Microsoft.AspNetCore.Mvc;
using TCG.InvoiceService.Application.Order.Command;
using TCG.InvoiceService.Application.Order.DTO.Request;
using TCG.InvoiceService.Application.Order.Query;

namespace TCG.InvoiceService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("add")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDtoRequest orderDtoRequest,
        CancellationToken cancellationToken)
    
    {
        await _mediator.Send(new CreateOrderCommand(orderDtoRequest), cancellationToken);
        return Ok();
    }

    [HttpGet("transaction/buyer")]
    public async Task<IActionResult> GetBuyerTransactions(int buyerId, CancellationToken cancellationToken)
    {
        var transactions = await _mediator.Send(new GetBuyerOrderQuery(buyerId), cancellationToken);
        if (transactions == null)
        {
            return NotFound();
        }
        return Ok(transactions);
    }
    
    [HttpGet("transaction/seller")]
    public async Task<IActionResult> GetSellerTransactions(int sellerId, CancellationToken cancellationToken)
    {
        var transactions = await _mediator.Send(new GetSellerOrderQuery(sellerId), cancellationToken);
        if (transactions == null)
        {
            return NotFound();
        }
        return Ok(transactions);
    }
}