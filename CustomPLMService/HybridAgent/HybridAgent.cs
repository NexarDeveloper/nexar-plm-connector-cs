﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using VoidTO = Altium.PLM.Custom.Void;

namespace CustomPLMService.HybridAgent;

public interface IHybridAgent
{
    Task Run(CancellationToken ct);
}

public class HybridAgent(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    IPublisher mediator,
    ILogger<HybridAgent> logger) : IHybridAgent
{
    public async Task Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var getRequestResponse = grpcClient.GetRequest(new VoidTO(), cancellationToken: ct);
                while (await getRequestResponse.ResponseStream.MoveNext())
                {
                    var request = getRequestResponse.ResponseStream.Current;
                    logger.LogInformation($"Received request {request}");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    (Task.Run(() => mediator.Publish(request.AsNotification(), ct), ct)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                logger.LogDebug(ex, "Deadline Exceeded");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Cancellation request received");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occured");
            }
        }
    }
}
