using MassTransit;
using Microsoft.Extensions.Logging;
using TCG.Common.MassTransit.Events;
using TCG.InvoiceService.Application.Contracts;

namespace TCG.InvoiceService.Application.Consumer.Event;

public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILogger<OrderCompletedEventConsumer> _logger;

    public OrderCompletedEventConsumer(IOrderRepository orderRepository, ILogger<OrderCompletedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        _logger.LogInformation("Order with Id: {MessageOrderId} added successfully", context.Message.PostId);
        return Task.CompletedTask;
    }
}