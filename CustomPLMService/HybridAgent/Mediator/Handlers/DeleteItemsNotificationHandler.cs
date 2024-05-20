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
using VoidTO = Altium.PLM.Custom.Void;
namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class DeleteItemsNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<DeleteItemsNotificationHandler> logger) : INotificationHandler<DeleteItemsNotification>
{

    public async Task Handle(DeleteItemsNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Delete Items request");

        await plmService.DeleteItems(notification.Request.Data.Select(mapper.Map<Id>), cancellationToken);
        await grpcClient.ReturnDeleteItemsAsync(new VoidEx
            {
                Value = new VoidTO()
            },
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)],
            cancellationToken: cancellationToken);
    }
}
