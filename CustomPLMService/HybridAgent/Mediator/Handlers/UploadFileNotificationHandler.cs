using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using FileResourceResponseTO = Altium.PLM.Custom.FileResourceResponse;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class UploadFileNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    IMapper mapper,
    ILogger<UploadFileNotificationHandler> logger) : INotificationHandler<UploadFileNotification>
{

    public async Task Handle(UploadFileNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Upload File request");

        var id = await plmService.UploadFile(mapper.Map<FileResource>(notification.Request), cancellationToken);
        await grpcClient.ReturnUploadFileAsync(new FileResourceResponseEx
        {
            CorrelationId = notification.CorrelationId,
            Value = new FileResourceResponseTO
            {
                Id = id
            }
        }, cancellationToken: cancellationToken);
    }
}
