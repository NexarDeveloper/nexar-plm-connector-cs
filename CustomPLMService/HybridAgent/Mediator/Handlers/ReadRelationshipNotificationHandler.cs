using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using RelationshipTableTO = Altium.PLM.Custom.RelationshipTable;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class ReadRelationshipNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<ReadRelationshipNotificationHandler> logger) : INotificationHandler<ReadRelationshipNotification>
{

    public async Task Handle(ReadRelationshipNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Read Relationships request");

        var responseStream = grpcClient.ReturnReadRelationships(cancellationToken: cancellationToken).RequestStream;

        try
        {
            var relationships = await plmService.ReadRelationships(
                notification.Request.Ids.Select(mapper.Map<Id>),
                mapper.Map<RelationshipType>(notification.Request.Type), cancellationToken);
            foreach (var relationship in relationships.Select(mapper.Map<RelationshipTableTO>))
            {
                await responseStream.WriteAsync(new RelationshipTableEx
                {
                    CorrelationId = notification.CorrelationId,
                    Value = relationship
                }, cancellationToken);
            }
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
