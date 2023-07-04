using Automatonymous;

namespace TCG.InvoiceService.Application.Order.Saga;

public class OrderStateInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public int Orderid { get; set; }
    public Guid PostId { get; set; }
    public int UserId { get; set; }
    public char OrderStatus { get; set; }
}