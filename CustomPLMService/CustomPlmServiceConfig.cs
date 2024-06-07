using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService
{
    [ExcludeFromCodeCoverage]
    public class CustomPlmServiceConfig
    {
        public const string Key = "CustomPlmService";

        public int MaxReceiveMessageSizeInMb { get; init; }
    }
}
