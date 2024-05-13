using System.Diagnostics.CodeAnalysis;
using MediatR;
namespace CustomPLMService.HybridAgent.Mediator;

[ExcludeFromCodeCoverage]
public abstract class BaseNotification<T> : INotification
{
    public string CorrelationId { get; init; }
    public T Request { get; init; }
}
