using MediatR;
namespace CustomPLMService.HybridAgent.Mediator;

public abstract class BaseNotification<T> : INotification
{
    public string CorrelationId { get; init; }
    public T Request { get; init; }
}
