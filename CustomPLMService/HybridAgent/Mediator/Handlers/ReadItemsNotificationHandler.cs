using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using ItemTO = Altium.PLM.Custom.Item; 

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class ReadItemsNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<ReadItemsNotificationHandler> logger) : INotificationHandler<ReadItemsNotification>
{

    public async Task Handle(ReadItemsNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Read Items request");

        var responseStream = grpcClient.ReturnReadItems(
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)],
            cancellationToken: cancellationToken).RequestStream;

        try
        {
            var items = await plmService.ReadItems(notification.Request.Data.Select(mapper.Map<Id>), cancellationToken);
            foreach (var item in items.Select(mapper.Map<ItemTO>))
            {
                await responseStream.WriteAsync(new ItemEx
                {
                    Value = item
                }, cancellationToken);
            }
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
