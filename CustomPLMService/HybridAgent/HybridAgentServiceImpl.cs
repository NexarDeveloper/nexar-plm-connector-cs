using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace CustomPLMService.HybridAgent;

public class HybridAgentServiceImpl(IHybridAgent hybridAgent, ILogger<HybridAgentServiceImpl> logger)
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