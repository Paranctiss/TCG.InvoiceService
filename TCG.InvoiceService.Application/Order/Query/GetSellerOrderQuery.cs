using System.Text.Json;
using MapsterMapper;
using MassTransit;
using MassTransit.Initializers;
using MediatR;
using Microsoft.Extensions.Logging;
using TCG.Common.MassTransit.Messages;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Application.Order.DTO.Response;
using ILogger = Serilog.ILogger;

namespace TCG.InvoiceService.Application.Order.Query;

public record GetSellerOrderQuery(int sellerId) : IRequest<IEnumerable<OrderBuyerDtoResponse>>;
public class GetSellerOrderQueryHandler : IRequestHandler<GetSellerOrderQuery, IEnumerable<OrderBuyerDtoResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestClient<BuyerTransaction> _requestClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerOrderQueryHandler> _logger;

    public GetSellerOrderQueryHandler(IOrderRepository orderRepository, IRequestClient<BuyerTransaction> requestClient, IMapper mapper, ILogger<GetSellerOrderQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _requestClient = requestClient;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<IEnumerable<OrderBuyerDtoResponse>> Handle(GetSellerOrderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sellerOrders = await _orderRepository.GetSellerTransaction(cancellationToken, request.sellerId);
            var merchPostIds = sellerOrders.Select(order => order.MerchPostId).ToList();
            var merchPostIdsSerialized = JsonSerializer.Serialize(merchPostIds);
            var sellerTransaction = new BuyerTransaction(merchPostIdsSerialized, request.sellerId);
            var merchPostNames = await _requestClient.GetResponse<BuyerTransactionResponse>(sellerTransaction, cancellationToken);
            if (merchPostNames.Message.Name.StartsWith("ERROR"))
            {
                _logger.LogError("Pas de merchPost");
                throw new Exception(merchPostNames.Message.Name);
            }
            var merchPostNamesDeserialized = JsonSerializer.Deserialize<IEnumerable<BuyerTransactionNameSerializedDto>>(merchPostNames.Message.Name);
            var mapped = _mapper.Map<List<OrderBuyerDtoResponse>>(sellerOrders);
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