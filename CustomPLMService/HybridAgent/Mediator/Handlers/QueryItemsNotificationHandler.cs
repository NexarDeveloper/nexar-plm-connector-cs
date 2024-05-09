using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Query;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using Type = CustomPLMService.Contract.Models.Metadata.Type;
using IdTO = Altium.PLM.Custom.Id;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class QueryItemsNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapperBase mapper,
    ILogger<QueryItemsNotificationHandler> logger) : INotificationHandler<QueryItemsNotification>
{

    public async Task Handle(QueryItemsNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Query Items request");

        var responseStream = grpcClient.ReturnQueryItems(cancellationToken: cancellationToken).RequestStream;

        try
        {
            var plmItems = await plmService.QueryItems(mapper.Map<Query>(notification.Request.Query),
                mapper.Map<Type>(notification.Request.Type), cancellationToken);
            foreach (var plmItem in plmItems.Select(mapper.Map<IdTO>))
            {
                await responseStream.WriteAsync(new IdEx
                {
                    CorrelationId = notification.CorrelationId,
                    Value = plmItem
                }, cancellationToken);
            }
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
