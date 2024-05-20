using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using AuthResultTO = Altium.PLM.Custom.AuthResult;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class TestAccessNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<TestAccessNotificationHandler> logger) : INotificationHandler<TestAccessNotification>
{

    public async Task Handle(TestAccessNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Test Access request");

        var authResult = new AuthResultTO();

        if (await plmService.TestAccess(mapper.Map<Auth>(notification.Request), cancellationToken))
        {
            authResult.Success = true;
            authResult.Status = AuthResultTO.Types.Status.Success;
        }
        else
        {
            authResult.Success = false;
            authResult.Status = AuthResultTO.Types.Status.InvalidCredentials;
            logger.LogInformation("Invalid Credentials Provided");
        }

        await grpcClient.ReturnTestAccessAsync(new AuthResultEx
            {
                Value = authResult
            },
            [new Metadata.Entry(Constants.CorrelationIdKey, notification.CorrelationId)],
            cancellationToken: cancellationToken);
    }
}
