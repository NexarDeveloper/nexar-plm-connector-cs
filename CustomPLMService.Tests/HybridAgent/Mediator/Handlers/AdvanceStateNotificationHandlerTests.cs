using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.HybridAgent.Mediator.Handlers;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class AdvanceStateNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<AdvanceStateNotificationHandler>> loggerMock = new();

    private readonly AdvanceStateNotificationHandler handler;

    public AdvanceStateNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Altium.PLM.Custom.Void()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnAdvanceStateAsync(It.IsAny<VoidEx>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new AdvanceStateNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        var notification = new AdvanceStateNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new AdvanceStateRequest
            {
                Id = new Id()
            }
        };
        var cancellationToken = (new CancellationTokenSource()).Token;

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnAdvanceStateAsync(
                It.Is<VoidEx>(v => v.CorrelationId == notification.CorrelationId),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                cancellationToken)
            , Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        var cancellationToken = (new CancellationTokenSource()).Token;
        var mappedId = new Contract.Models.Items.Id()
        {
            PublicId = "MappedTestPublicId"
        };
        var notification = new AdvanceStateNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new AdvanceStateRequest
            {
                Id = new Id
                {
                    PublicId = "TestPublicId"
                }
            }
        };

        mapperMock.Setup(m => m.Map<CustomPLMService.Contract.Models.Items.Id>(It.Is<Id>(r => r.PublicId == notification.Request.Id.PublicId))).Returns(mappedId);

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m =>
                m.AdvanceState(It.Is<CustomPLMService.Contract.Models.Items.Id>(id => id.PublicId == mappedId.PublicId),
                    cancellationToken),
            Times.Once);
    }
}
