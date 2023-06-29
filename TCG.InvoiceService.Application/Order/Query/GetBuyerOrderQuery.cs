using System.Text.Json;
using MapsterMapper;
using MassTransit;
using MassTransit.Initializers;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;
using TCG.Common.MassTransit.Messages;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Application.Order.DTO.Response;
using ILogger = Serilog.ILogger;

namespace TCG.InvoiceService.Application.Order.Query;

public record GetBuyerOrderQuery(int buyerId) : IRequest<IEnumerable<OrderBuyerDtoResponse>>;
public class GetBuyerOrderQueryHandler : IRequestHandler<GetBuyerOrderQuery, IEnumerable<OrderBuyerDtoResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestClient<BuyerTransaction> _requestClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBuyerOrderQueryHandler> _logger;

    public GetBuyerOrderQueryHandler(IOrderRepository orderRepository, IRequestClient<BuyerTransaction> requestClient, IMapper mapper, ILogger<GetBuyerOrderQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _requestClient = requestClient;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<IEnumerable<OrderBuyerDtoResponse>> Handle(GetBuyerOrderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var buyerOrders = await _orderRepository.GetBuyerTransaction(cancellationToken, request.buyerId);
            var merchPostIds = buyerOrders.Select(order => order.MerchPostId).ToList();
            var merchPostIdsSerialized = JsonSerializer.Serialize(merchPostIds);
            var buyerTransaction = new BuyerTransaction(merchPostIdsSerialized, request.buyerId);
            var merchPostNames = await _requestClient.GetResponse<BuyerTransactionResponse>(buyerTransaction, cancellationToken);
            if (merchPostNames.Message.Name.StartsWith("ERROR"))
            {
                _logger.LogError("Pas de merchPost");
                throw new Exception(merchPostNames.Message.Name);
            }
            var merchPostNamesDeserialized = JsonSerializer.Deserialize<IEnumerable<BuyerTransactionNameSerializedDto>>(merchPostNames.Message.Name);
            var mapped = _mapper.Map<List<OrderBuyerDtoResponse>>(buyerOrders);
            mapped.ForEach(order =>
            {
                var merchNameDto = merchPostNamesDeserialized.FirstOrDefault(x => x.MerchPostId == order.MerchPostId);
                order.MerchPostName = merchNameDto.MerchPostName;
                order.TotalWithShip = order.TotalPrice + order.ShipmentFee;
                order.MerchPostNamePhotos = merchNameDto.MerchPostNamePhotos;
                _logger.LogInformation("Transactions has been mapped");
            });

            return mapped;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}