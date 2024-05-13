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
using BaseType = CustomPLMService.Contract.Models.Metadata.BaseType;
using TypeId = CustomPLMService.Contract.Models.Metadata.TypeId;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class ReadTypeIdentifiersNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmMetadataService> plmMetadataServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<ReadTypeIdentifiersNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<TypeIdEx>> grpcClientResponseStream = new();

    private readonly ReadTypeIdentifiersNotificationHandler handler;

    public ReadTypeIdentifiersNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnReadTypeIdentifiers(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new ReadTypeIdentifiersNotificationHandler(grpcClientMock.Object, plmMetadataServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmMetadataServiceMock.Setup(m => m.ReadTypeIdentifiers(
                It.IsAny<BaseType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new TypeId(), new TypeId(), new TypeId()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadTypeIdentifiersNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new TypeRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientResponseStream.Verify(m => m.WriteAsync(
                It.Is<TypeIdEx>(item => item.CorrelationId == notification.CorrelationId), cancellationToken),
            Times.Exactly(3));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmMetadataServiceMock.Setup(m => m.ReadTypeIdentifiers(
                It.IsAny<BaseType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TypeId>());

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadTypeIdentifiersNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new TypeRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmMetadataServiceMock.Verify(m => m.ReadTypeIdentifiers(
                It.IsAny<BaseType>(), cancellationToken),
            Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
