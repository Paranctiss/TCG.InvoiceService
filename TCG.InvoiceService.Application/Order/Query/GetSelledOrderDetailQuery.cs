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

public record GetSelledOrderDetailQuery(int OrderId) : IRequest<OrderBuyerDtoResponse>;
public class GetSelledOrderDetailQueryHandler : IRequestHandler<GetSelledOrderDetailQuery, OrderBuyerDtoResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestClient<BuyerTransaction> _requestClient;
    private readonly IRequestClient<UserById> _requestUser;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSelledOrderDetailQueryHandler> _logger;

    public GetSelledOrderDetailQueryHandler(IOrderRepository orderRepository, IRequestClient<BuyerTransaction> requestClient,IRequestClient<UserById> requestUser, IMapper mapper, ILogger<GetSelledOrderDetailQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _requestClient = requestClient;
        _requestUser = requestUser;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<OrderBuyerDtoResponse> Handle(GetSelledOrderDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sellerOrder = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (sellerOrder == null)
            {
                throw new NotFoundException("Il n'y a pas de transaction");
            }
            var merchPostId = sellerOrder.MerchPostId;
            List<Guid> merchPostIdList = new List<Guid> { merchPostId };
            var merchPostIdsSerialized = JsonSerializer.Serialize(merchPostIdList);
            var buyerTransaction = new BuyerTransaction(merchPostIdsSerialized, sellerOrder.SellerId);
            var merchPostNames =
                await _requestClient.GetResponse<BuyerTransactionResponse>(buyerTransaction, cancellationToken);
            var buyer = new UserById(sellerOrder.BuyerId);
            var username = await _requestUser.GetResponse<UserByIdResponse>(buyer, cancellationToken);
            if (merchPostNames.Message.Name.StartsWith("ERROR"))
            {
                _logger.LogError("Pas de merchPost");
                throw new Exception(merchPostNames.Message.Name);
            }

            var merchPostNamesDeserialized =
                JsonSerializer.Deserialize<List<BuyerTransactionNameSerializedDto>>(merchPostNames.Message.Name);
            var mapped = _mapper.Map<OrderBuyerDtoResponse>(sellerOrder);
            mapped.TotalWithShip = sellerOrder.TotalPrice + sellerOrder.ShipmentFee;
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