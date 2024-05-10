using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using ItemTO = Altium.PLM.Custom.Item;
using ItemResultTO = Altium.PLM.Custom.ItemResult;
using ErrorTO = Altium.PLM.Custom.Error;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class CreateItemsNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient, 
    ICustomPlmService plmService, IMapper mapper, 
    ILogger<CreateItemsNotificationHandler> logger) : INotificationHandler<CreateItemsNotification>
{

    public async Task Handle(CreateItemsNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Create Items request");
        
        var responseStream = grpcClient.ReturnCreateItems(cancellationToken: cancellationToken).RequestStream;
        try
        {
            var createdItems =
                await plmService.CreateItems(notification.Request.Data.Select(mapper.Map<ItemCreateSpec>), cancellationToken);
            foreach (var createdItem in createdItems.Select(mapper.Map<ItemTO>))
            {
                await responseStream.WriteAsync(new ItemResultEx
                    {
                        CorrelationId = notification.CorrelationId,
                        Value = new ItemResultTO
                        {
                            Item = createdItem
                        }
                    }
                    , cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while trying to create items");
            await responseStream.WriteAsync(new ItemResultEx
            {
                CorrelationId = notification.CorrelationId,
                Value = new ItemResultTO
                {
                    Error = new ErrorTO
                    {
                        Message = e.Message
                    }
                }
            }, cancellationToken);
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
