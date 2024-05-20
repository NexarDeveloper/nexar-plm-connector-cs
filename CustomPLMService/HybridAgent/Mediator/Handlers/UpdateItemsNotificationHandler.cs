using System;
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
using ItemResultTO = Altium.PLM.Custom.ItemResult;
using ErrorTO = Altium.PLM.Custom.Error;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class UpdateItemsNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<UpdateItemsNotificationHandler> logger) : INotificationHandler<UpdateItemsNotification>
{

    public async Task Handle(UpdateItemsNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Update Items request");

        var responseStream = grpcClient.ReturnUpdateItems(
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)],
            cancellationToken: cancellationToken).RequestStream;
        try
        {
            var updatedItems =
                await plmService.UpdateItems(notification.Request.Data.Select(mapper.Map<ItemUpdateSpec>), cancellationToken);
            foreach (var updatedItem in updatedItems)
            {
                await responseStream.WriteAsync(new ItemResultEx
                {
                    Value = mapper.Map<ItemResultTO>(updatedItem)
                }, cancellationToken);
            }
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
