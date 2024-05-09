using System;
using Google.Protobuf;
namespace CustomPLMService.HybridAgent;

public class RequestMappingFailedException : Exception
{
    public RequestMappingFailedException(IMessage request) : base("Failed to map request to notification object: " + request)
    { 
    }
}
