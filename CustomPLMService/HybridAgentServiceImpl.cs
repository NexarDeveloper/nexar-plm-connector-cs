using System;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using CustomPLMService.Configs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomPLMService;

public class HybridAgentServiceImpl(IHybridAgent hybridAgent, ReversePLMService.ReversePLMServiceClient plmClient, IOptions<HybridAgentConfig> config, ILogger<HybridAgentServiceImpl> logger)
    : IHostedService, IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Hybrid Agent Service starting");
        _ = hybridAgent.Run(cancellationTokenSource.Token);
        logger.LogInformation("Hybrid Agent Service started");

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Hybrid Agent Service stopping");
        await cancellationTokenSource.CancelAsync();
        logger.LogInformation("Hybrid Agent Service stopped");
    }

    public void Dispose()
    {
        logger.LogInformation("Hybrid Agent Service disposing");
    }
}