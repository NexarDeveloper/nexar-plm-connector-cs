using System;
using System.Collections.Generic;
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
using Type = CustomPLMService.Contract.Models.Metadata.Type;
using TypeId = CustomPLMService.Contract.Models.Metadata.TypeId;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class ReadTypesNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmMetadataService> plmMetadataServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<ReadTypesNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<TypeEx>> grpcClientResponseStream = new();

    private readonly ReadTypesNotificationHandler handler;

    public ReadTypesNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnReadTypes(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new ReadTypesNotificationHandler(grpcClientMock.Object, plmMetadataServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmMetadataServiceMock.Setup(m => m.ReadTypes(
                It.IsAny<IEnumerable<TypeId>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Type(), new Type(), new Type(), new Type(), new Type()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadTypesNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new TypeIdRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientResponseStream.Verify(m => m.WriteAsync(
                It.Is<TypeEx>(item => item.CorrelationId == notification.CorrelationId), cancellationToken),
            Times.Exactly(5));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmMetadataServiceMock.Setup(m => m.ReadTypes(
                It.IsAny<IEnumerable<TypeId>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Type>());

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadTypesNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new TypeIdRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmMetadataServiceMock.Verify(m => m.ReadTypes(It.IsAny<IEnumerable<TypeId>>(), cancellationToken), Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
