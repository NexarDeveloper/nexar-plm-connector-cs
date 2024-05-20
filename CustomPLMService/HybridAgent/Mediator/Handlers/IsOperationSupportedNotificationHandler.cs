using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

using OperationSupportedResponseTO = OperationSupportedResponse;

public class IsOperationSupportedNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<IsOperationSupportedNotificationHandler> logger) : INotificationHandler<IsOperationSupportedNotification>
{
    public async Task Handle(IsOperationSupportedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Is Operation Supported request");

        var isSupported =
            await plmService.IsOperationSupported(
                mapper.Map<SupportedOperation>(notification.Request.Operation), cancellationToken);
        await grpcClient.ReturnIsOperationSupportedAsync(new OperationSupportedResponseEx
            {
                Value = new OperationSupportedResponseTO
                {
                    IsSupported = isSupported
                }
            },
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)]
            , cancellationToken: cancellationToken);
    }
}
