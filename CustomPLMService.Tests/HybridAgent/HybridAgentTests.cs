using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using Altium.PLM.Custom.Reverse;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Void = Altium.PLM.Custom.Void;

namespace CustomPLMService.Tests.HybridAgent;

[ExcludeFromCodeCoverage]
public class HybridAgentTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<IAsyncStreamReader<Request>> grpcServerResponseStream = new();

    private readonly Mock<IPublisher> mediatorMock = new();
    private readonly Mock<ILogger<CustomPLMService.HybridAgent.HybridAgent>> loggerMock = new();

    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly CustomPLMService.HybridAgent.HybridAgent hybridAgent;

    public HybridAgentTests()
    {
        var grpcServerResponseMock = TestCalls.AsyncServerStreamingCall(grpcServerResponseStream.Object, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });

        grpcClientMock.Setup(m => m.GetRequest(
            It.IsAny<Void>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())
        ).Returns(grpcServerResponseMock);

        hybridAgent = new CustomPLMService.HybridAgent.HybridAgent(grpcClientMock.Object, mediatorMock.Object, loggerMock.Object);
    }

    [Fact(Timeout = 1000)]
    public async Task Run_StopsWhenCancellationTokenCancelled()
    {
        // Arrange
        grpcServerResponseStream.Setup(m => m.MoveNext(It.IsAny<CancellationToken>())).Callback(async () =>
        {
            await cancellationTokenSource.CancelAsync();
        }).ReturnsAsync(false);

        // Act
        var wrapperTask = Task.Run<Task>(() => hybridAgent.Run(cancellationTokenSource.Token));
        var agentTask = await wrapperTask;

        // Assert
        wrapperTask.Status.Should().Be(TaskStatus.RanToCompletion);
        agentTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact(Timeout = 1000)]
    public async Task Run_ContinuesIfRpcDeadlineExceededExceptionIsThrown()
    {
        // Arrange
        grpcServerResponseStream.SetupSequence(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .Throws(new RpcException(new Status(StatusCode.DeadlineExceeded, "TestDeadlineExceeded", null)))
            .ReturnsAsync(() =>
            {
                cancellationTokenSource.Cancel();
                return false;
            });

        // Act
        await Task.Run(() => hybridAgent.Run(cancellationTokenSource.Token));

        // Assert
        grpcServerResponseStream.Verify(m => m.MoveNext(It.IsAny<CancellationToken>()), Times.AtLeast(2));
        VerifyLogger(LogLevel.Debug, "Deadline Exceeded", Times.Once());
    }

    [Fact(Timeout = 1000)]
    public async Task Run_ContinuesIfAnyExceptionIsThrown()
    {
        // Arrange
        grpcServerResponseStream.SetupSequence(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .Throws(new Exception())
            .ReturnsAsync(() =>
            {
                cancellationTokenSource.Cancel();
                return false;
            });

        // Act
        await Task.Run(() => hybridAgent.Run(cancellationTokenSource.Token));

        // Assert
        grpcServerResponseStream.Verify(m => m.MoveNext(It.IsAny<CancellationToken>()), Times.AtLeast(2));
        VerifyLogger(LogLevel.Error, "Unexpected error occured", Times.Once());
    }

    [Fact(Timeout = 1000)]
    public async Task Run_ContinuesIfOperationCanceledExceptionIsThrown()
    {
        // Arrange
        grpcServerResponseStream.SetupSequence(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .Throws(new OperationCanceledException())
            .ReturnsAsync(() =>
            {
                cancellationTokenSource.Cancel();
                return false;
            });

        // Act
        await Task.Run(() => hybridAgent.Run(cancellationTokenSource.Token));

        // Assert
        grpcServerResponseStream.Verify(m => m.MoveNext(It.IsAny<CancellationToken>()), Times.AtLeast(2));
        VerifyLogger(LogLevel.Information, "Cancellation request received", Times.Once());
    }

    [Fact(Timeout = 1000)]
    public async Task Run_CallsMediatorIfRequestGiven()
    {
        // Arrange
        var firstCall = true;
        grpcServerResponseStream.Setup(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (!firstCall)
                    return false;

                firstCall = false;
                return true;
            });
        mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Callback((INotification _, CancellationToken _) =>
        {
            cancellationTokenSource.Cancel();
        });

        grpcServerResponseStream.SetupGet(m => m.Current).Returns(new Request
        {
            TestAccess = new Auth()
        });

        // Act
        await Task.Run(() => hybridAgent.Run(cancellationTokenSource.Token));

        // Assert
        grpcServerResponseStream.Verify(m => m.MoveNext(It.IsAny<CancellationToken>()), Times.AtLeast(2));
        mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Timeout = 1000)]
    public async Task Run_ContinuesIfMediatorThrows()
    {
        // Arrange
        grpcServerResponseStream.Setup(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Callback((INotification _, CancellationToken _) =>
        {
            cancellationTokenSource.Cancel();
        }).Throws(new Exception());

        grpcServerResponseStream.SetupGet(m => m.Current).Returns(new Request
        {
            TestAccess = new Auth()
        });

        // Act
        await Task.Run(() => hybridAgent.Run(cancellationTokenSource.Token));

        // Assert
        mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        VerifyLogger(LogLevel.Error, "Unexpected mediator error occured", Times.AtLeastOnce());
    }
    
    [Fact(Timeout = 1000)]
    public async Task Run_DoesntBlockOnMultipleCalls()
    {
        // Arrange
        var remainingCallCount = 3;
        grpcServerResponseStream.Setup(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Callback((INotification _, CancellationToken _) =>
        {
            if (remainingCallCount-- == 0)
            {
                cancellationTokenSource.Cancel();
            }
        }).Returns((INotification _, CancellationToken ct) => new Task(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }));

        grpcServerResponseStream.SetupGet(m => m.Current).Returns(new Request
        {
            TestAccess = new Auth()
        });

        // Act
        await Task.Run(() => hybridAgent.Run(cancellationTokenSource.Token));

        // Assert
        mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.AtLeast(3));
    }

    private void VerifyLogger(LogLevel level, string message, Times times)
    {
        loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == level),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            times);
    }
}
