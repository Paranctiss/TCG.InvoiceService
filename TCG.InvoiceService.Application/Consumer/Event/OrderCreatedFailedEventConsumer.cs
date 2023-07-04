using MassTransit;
using Microsoft.Extensions.Logging;
using TCG.Common.MassTransit.Events;
using TCG.InvoiceService.Application.Contracts;

namespace TCG.InvoiceService.Application.Consumer.Event;

public class OrderCreatedFailedEventConsumer : IConsumer<OrderCreatedFailedEvent>
{
    private readonly ILogger<OrderCreatedFailedEvent> _logger;

    public OrderCreatedFailedEventConsumer(ILogger<OrderCreatedFailedEvent> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreatedFailedEvent> context)
    {
        _logger.LogInformation("Order with Id: {MessageOrderId} failed", context.Message.PostId);
        return Task.CompletedTask;
    }
}