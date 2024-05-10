using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Relationship;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using VoidTO = Altium.PLM.Custom.Void;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class CreateRelationshipsNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<CreateRelationshipsNotificationHandler> logger) : INotificationHandler<CreateRelationshipsNotification>
{

    public async Task Handle(CreateRelationshipsNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Create Relationships request");
        
        await plmService.CreateRelationships(
            notification.Request.Relationships.Select(mapper.Map<RelationshipTable>), cancellationToken);
        await grpcClient.ReturnCreateRelationshipsAsync(new VoidEx
        {
            CorrelationId = notification.CorrelationId,
            Value = new VoidTO()
        }, cancellationToken: cancellationToken);
    }
}
