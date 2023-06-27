using System.Text.Json;
using MapsterMapper;
using MassTransit;
using MassTransit.Initializers;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;
using TCG.Common.MassTransit.Messages;
using TCG.Common.Middlewares.MiddlewareException;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Application.Order.DTO.Response;
using ILogger = Serilog.ILogger;

namespace TCG.InvoiceService.Application.Order.Query;

public record GetBuyedOrderDetailQuery(int OrderId) : IRequest<OrderBuyerDtoResponse>;
public class GetBuyedOrderDetailQueryHandler : IRequestHandler<GetBuyedOrderDetailQuery, OrderBuyerDtoResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestClient<BuyerTransaction> _requestClient;
    private readonly IRequestClient<UserById> _requestUser;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBuyedOrderDetailQueryHandler> _logger;

    public GetBuyedOrderDetailQueryHandler(IOrderRepository orderRepository, IRequestClient<BuyerTransaction> requestClient,IRequestClient<UserById> requestUser, IMapper mapper, ILogger<GetBuyedOrderDetailQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _requestClient = requestClient;
        _requestUser = requestUser;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<OrderBuyerDtoResponse> Handle(GetBuyedOrderDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var buyerOrder = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (buyerOrder == null)
            {
                throw new NotFoundException("Il n'y a pas de transaction");
            }
            var merchPostId = buyerOrder.MerchPostId;
            List<Guid> merchPostIdList = new List<Guid> { merchPostId };
            var merchPostIdsSerialized = JsonSerializer.Serialize(merchPostIdList);
            var buyerTransaction = new BuyerTransaction(merchPostIdsSerialized, buyerOrder.BuyerId);
            var merchPostNames =
                await _requestClient.GetResponse<BuyerTransactionResponse>(buyerTransaction, cancellationToken);
            var seller = new UserById(buyerOrder.SellerId);
            var username = await _requestUser.GetResponse<UserByIdResponse>(seller, cancellationToken);
            if (merchPostNames.Message.Name.StartsWith("ERROR"))
            {
                _logger.LogError("Pas de merchPost");
                throw new Exception(merchPostNames.Message.Name);
            }

            var merchPostNamesDeserialized =
                JsonSerializer.Deserialize<List<BuyerTransactionNameSerializedDto>>(merchPostNames.Message.Name);
            var mapped = _mapper.Map<OrderBuyerDtoResponse>(buyerOrder);
            mapped.TotalWithShip = buyerOrder.TotalPrice + buyerOrder.ShipmentFee;
            mapped.Username = username.Message.username;
            mapped.MerchPostName = merchPostNamesDeserialized[0].MerchPostName;
            mapped.MerchPostNamePhotos = merchPostNamesDeserialized[0].MerchPostNamePhotos;
            return mapped;
        }
        catch (NotFoundException e)
        {
            _logger.LogError(e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}