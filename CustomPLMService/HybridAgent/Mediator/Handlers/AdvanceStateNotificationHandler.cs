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

public class AdvanceStateNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<AdvanceStateNotificationHandler> logger) : INotificationHandler<AdvanceStateNotification>
{
    public async Task Handle(AdvanceStateNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Advance State request");

        await plmService.AdvanceState(
            mapper.Map<Id>(notification.Request.Id), cancellationToken);
        await grpcClient.ReturnAdvanceStateAsync(new VoidEx
            {
                Value = new VoidTO()
            },
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)],
            cancellationToken: cancellationToken);
    }
}
