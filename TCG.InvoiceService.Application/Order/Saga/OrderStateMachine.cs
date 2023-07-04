using Automatonymous;
using Serilog;
using MassTransit;
using TCG.Common.MassTransit.Events;
using TCG.Common.MassTransit.Messages;

namespace TCG.InvoiceService.Application.Order.Saga;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    private readonly ILogger _logger;

    //Commands
    private Event<CreateOrderMessage> CreateOrderMessage { get; set; }

    //Events
    private Event<OrderStateChangedEvent> OrderStateChangedEvent  { get; set; }
    private Event<OrderStateChangedFailedEvent> OrderStateChangedFailedEvent  { get; set; }
    private Event<FidelityPointCompletedEvent> FidelityPointCompletedEvent  { get; set; }
    private Event<FidelityPointFailedEvent> FidelityPointFailedEvent { get; set; }

    //States
    private State OrderCreated { get; set; }
    private State FidelityFailed { get; set; }
    private State FidelityCredited { get; set; }
    private State OrderCompleted { get; set; }
    private State OrderFailed { get; set; }

    public OrderStateMachine()
    {
        _logger = Serilog.Log.Logger;
        InstanceState(x => x.CurrentState);

        Event(() => CreateOrderMessage, y => y.CorrelateBy<int>(x => x.Orderid, z => z.Message.OrderId)
            .SelectId(context => Guid.NewGuid()));
        Event(() => OrderStateChangedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => OrderStateChangedFailedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

        Initially(
            When(CreateOrderMessage)
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("CreateOrderMessage received in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .Then(context =>
                {
                    context.Instance.Orderid = context.Data.OrderId;
                    context.Instance.OrderStatus = context.Data.OrderStatus;
                    context.Instance.PostId = context.Data.PostId;
                    context.Instance.UserId = context.Data.UserId;
                })
                .Publish(context => new OrderCreatedEvent(
                            CorrelationId: context.Instance.CorrelationId,
                            PostId: context.Data.PostId,
                            UserId: context.Data.UserId,
                            Status: context.Data.OrderStatus)
                )
                .TransitionTo(OrderCreated)
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("OrderCreatedEvent published in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
        );

        During(OrderCreated,
            When(OrderStateChangedEvent)
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("OrderStateChangedEvent received in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .TransitionTo(FidelityCredited)
                .Publish(context => new AddFidelityPointMessage(
                    CorrelationId: context.Instance.CorrelationId,
                    Point: 5,
                    UserId: context.Instance.UserId
                ))
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("Fidelity point credited in OrderStateMachine: {ContextSaga} ", context.Instance);
                }),
            When(OrderStateChangedFailedEvent)
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("OrderStateChangedFailedEvent received in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .TransitionTo(FidelityFailed)
                .Publish(context => new OrderCreatedFailedEvent(
                    ErrorMessage: context.Data.ErrorMessage,
                    PostId: context.Instance.PostId,
                    Status: context.Instance.OrderStatus
                ))
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("OrderCreatedFailedEvent published in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
        );

        During(FidelityCredited,
            When(FidelityPointCompletedEvent)
                .Then(context =>
                {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information(
                        "FidelityCompleted received in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .TransitionTo(OrderCompleted)
                .Publish(context => new OrderCompletedEvent(
                    PostId: context.Instance.PostId,
                    CorrelationId: context.Instance.CorrelationId
                ))
                .Then(context =>
                {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information(
                        "OrderCompletedEvent published in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .Finalize(),
            When(FidelityPointFailedEvent)
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("PaymentFailedEvent received in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .Publish(context => new OrderCreatedFailedEvent(
                    Status: context.Instance.OrderStatus,
                    PostId: context.Instance.PostId,
                    ErrorMessage: context.Data.ErrorMessage
                ))
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("OrderFailedEvent published in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .Publish(context => new OrderStateRollbackMessage(PostId: context.Instance.PostId))
                .Then(context => {
                    _logger.ForContext("CorrelationId", context.Instance.CorrelationId).Information("StockRollbackMessage sent in OrderStateMachine: {ContextSaga} ", context.Instance);
                })
                .TransitionTo(OrderFailed)
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
}