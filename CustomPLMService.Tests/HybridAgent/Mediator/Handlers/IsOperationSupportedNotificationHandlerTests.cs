using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
using CustomPLMService.HybridAgent.Mediator.Handlers;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class IsOperationSupportedNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<IsOperationSupportedNotificationHandler>> loggerMock = new();

    private readonly IsOperationSupportedNotificationHandler handler;

    public IsOperationSupportedNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Altium.PLM.Custom.Void()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnIsOperationSupportedAsync(It.IsAny<OperationSupportedResponseEx>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new IsOperationSupportedNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Handle_CallsGrpcService(bool serviceResponse, bool expectedResult)
    {
        // Arrange
        var notification = new IsOperationSupportedNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new OperationSupportedRequest()
        };
        const SupportedOperation mappedOperation = SupportedOperation.AdvanceChangeOrder;
        mapperMock.Setup(m => m.Map<SupportedOperation>(It.IsAny<OperationSupportedRequest.Types.Operation>())).Returns(mappedOperation);
        plmServiceMock.Setup(m => m.IsOperationSupported(It.IsAny<SupportedOperation>(), It.IsAny<CancellationToken>())).ReturnsAsync(serviceResponse);
        var cancellationToken = (new CancellationTokenSource()).Token;

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnIsOperationSupportedAsync(
                It.Is<OperationSupportedResponseEx>(v => v.Value.IsSupported == expectedResult),
                It.Is<Metadata>(metadata=>metadata.Get(Constants.CorrelationIdKey).Value == notification.CorrelationId),
                It.IsAny<DateTime?>(),
                cancellationToken)
            , Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        var notification = new IsOperationSupportedNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new OperationSupportedRequest()
        };
        const SupportedOperation mappedOperation = SupportedOperation.AdvanceChangeOrder;
        mapperMock.Setup(m => m.Map<SupportedOperation>(It.IsAny<OperationSupportedRequest.Types.Operation>())).Returns(mappedOperation);
        var cancellationToken = (new CancellationTokenSource()).Token;

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m => m.IsOperationSupported(mappedOperation, cancellationToken), Times.Once);
    }
}
