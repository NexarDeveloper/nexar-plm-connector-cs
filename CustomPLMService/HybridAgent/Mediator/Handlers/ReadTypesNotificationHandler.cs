using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using TypeTO = Altium.PLM.Custom.Type;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class ReadTypesNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmMetadataService plmMetadataService,
    IMapper mapper,
    ILogger<ReadTypesNotificationHandler> logger) : INotificationHandler<ReadTypesNotification>
{

    public async Task Handle(ReadTypesNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Read Types request");

        var responseStream = grpcClient.ReturnReadTypes(
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)],
            cancellationToken: cancellationToken).RequestStream;
        try
        {
            var types = await plmMetadataService.ReadTypes(notification.Request.Data.Select(mapper.Map<TypeId>), cancellationToken);
            foreach (var type in types.Select(mapper.Map<TypeTO>))
            {
                await responseStream.WriteAsync(new TypeEx
                {
                    Value = type
                }, cancellationToken);
            }
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
