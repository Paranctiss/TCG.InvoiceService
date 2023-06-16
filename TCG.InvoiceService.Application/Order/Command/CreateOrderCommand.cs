using MapsterMapper;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using TCG.Common.MassTransit.Messages;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Application.Order.DTO.Request;
using TCG.InvoiceService.Application.Order.DTO.Response;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Application.Order.Command;

public record CreateOrderCommand(OrderDtoRequest OrderDtoRequest) : IRequest<OrderDtoResponse>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDtoResponse>
{
    private readonly ILogger _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IInvoiceHeadRepository _invoiceHeadRepository;
    private readonly IInvoiceBodyRepository _invoiceBodyRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(
        ILogger<CreateOrderCommandHandler> logger, 
        IOrderRepository orderRepository, 
        IPublishEndpoint publishEndpoint,
        IMapper mapper,
        IInvoiceHeadRepository invoiceHeadRepository,
        IInvoiceBodyRepository invoiceBodyRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _invoiceHeadRepository = invoiceHeadRepository;
        _invoiceBodyRepository = invoiceBodyRepository;
    }
    
    public async Task<OrderDtoResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Domain.Order order = new Domain.Order
            {
                ShipAddress = request.OrderDtoRequest.ShipAddress,
                SellerId = request.OrderDtoRequest.SellerId,
                BuyerId = request.OrderDtoRequest.BuyerId,
                MerchPostId = request.OrderDtoRequest.MerchPostId,
                OrderStateId = request.OrderDtoRequest.StateId,
                ShipmentFee = request.OrderDtoRequest.ShipmentFee,
                TotalPrice = request.OrderDtoRequest.TotalPrice
            };

            await _orderRepository.ExecuteInTransactionAsync(async() =>
            {
                await _orderRepository.AddAsync(order, cancellationToken);

                //Rabbit
                var statusMessage = new OrderStatusChanged(order.MerchPostId, order.OrderStateId);
                await _publishEndpoint.Publish(statusMessage, cancellationToken);
                
                var invoiceHead = new InvoiceHead
                {
                    Date = DateTime.Now,
                    TotalPriceHT = request.OrderDtoRequest.TotalPrice,
                    TVA = 20,
                    Delivery = "La poste",
                    ShipAddress = request.OrderDtoRequest.ShipAddress,
                    BillAddress = request.OrderDtoRequest.ShipAddress,
                    Quantity = 1,
                    OrderId = order.Id
                };
                invoiceHead.TotalPriceTTC = order.TotalPrice * (1 + invoiceHead.TVA / 100);

                await _invoiceHeadRepository.AddAsync(invoiceHead, cancellationToken);

                var invoiceBody = new InvoiceBody
                {
                    Line = 1,
                    Title = "Carte1",
                    Price = invoiceHead.TotalPriceTTC,
                    Reduction = 0,
                    InvoiceHeadId = invoiceHead.Id
                };

                await _invoiceBodyRepository.AddAsync(invoiceBody, cancellationToken);
                
            }, cancellationToken);
            
            var mapped = _mapper.Map<OrderDtoResponse>(order);
            return mapped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error while adding user");
            throw;
        }
    }
}