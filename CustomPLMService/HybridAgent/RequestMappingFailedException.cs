using System;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
namespace CustomPLMService.HybridAgent;

[ExcludeFromCodeCoverage]
public class RequestMappingFailedException(IMessage request) : Exception("Failed to map request to notification object: " + request)
{
    public IMessage Request { get; init; } = request;
}
