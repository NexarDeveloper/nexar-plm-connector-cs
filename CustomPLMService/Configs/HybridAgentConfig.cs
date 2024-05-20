using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Configs
{
    [ExcludeFromCodeCoverage]
    public class HybridAgentConfig
    {
        public const string Key = "HybridAgent";

        public string Uri { get; init; }
        public string ApiKey { get; init; }

        public int DeadlineInSeconds { get; init; } = 30;
        public int RetryMaxAttempts { get; init; } = 5;
        public double RetryInitialBackoffInSeconds { get; init; } = 1;
        public double RetryMaxBackoffInSeconds { get; init; } = 5;
        public string TenantId { get; init; }
        public string AgentId { get; init; }
        public double OnExceptionTimeoutInSeconds { get; init; } = 2;
    }
}