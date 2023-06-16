using System.Text.Json;
using MapsterMapper;
using MassTransit;
using MassTransit.Initializers;
using MediatR;
using TCG.Common.MassTransit.Messages;
using TCG.InvoiceService.Application.Contracts;
using TCG.InvoiceService.Application.Order.DTO.Response;

namespace TCG.InvoiceService.Application.Order.Query;

public record GetBuyerOrderQuery(int buyerId) : IRequest<IEnumerable<OrderBuyerDtoResponse>>;
public class GetBuyerOrderQueryHandler : IRequestHandler<GetBuyerOrderQuery, IEnumerable<OrderBuyerDtoResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestClient<BuyerTransaction> _requestClient;
    private readonly IMapper _mapper;

    public GetBuyerOrderQueryHandler(IOrderRepository orderRepository, IRequestClient<BuyerTransaction> requestClient, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _requestClient = requestClient;
        _mapper = mapper;
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
                // Gérer l'erreur ici. Par exemple, retourner null ou lever une exception.
                throw new Exception(merchPostNames.Message.Name);
            }
            var merchPostNamesDeserialized = JsonSerializer.Deserialize<IEnumerable<BuyerTransactionNameSerializedDto>>(merchPostNames.Message.Name);
            var mapped = _mapper.Map<List<OrderBuyerDtoResponse>>(buyerOrders);
            mapped.ForEach(order =>
            {
                var merchNameDto = merchPostNamesDeserialized.FirstOrDefault(x => x.MerchPostId == order.MerchPostId);
                order.MerchPostName = merchNameDto.MerchPostName;
                order.MerchPostNamePhotos = merchNameDto.MerchPostNamePhotos;
            });

            return mapped;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        throw new NotImplementedException();
    }
}