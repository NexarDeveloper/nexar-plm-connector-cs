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
using Id = CustomPLMService.Contract.Models.Items.Id;
using Item = CustomPLMService.Contract.Models.Items.Item;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class ReadItemsNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<ReadItemsNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<ItemEx>> grpcClientResponseStream = new();

    private readonly ReadItemsNotificationHandler handler;

    public ReadItemsNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnReadItems(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new ReadItemsNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.ReadItems(
                It.IsAny<IEnumerable<Id>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Item(), new Item()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new IdRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientResponseStream.Verify(m => m.WriteAsync(
                It.Is<ItemEx>(item => item.CorrelationId == notification.CorrelationId), cancellationToken),
            Times.Exactly(2));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.ReadItems(
                It.IsAny<IEnumerable<Id>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Item>());

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new IdRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m => m.ReadItems(It.IsAny<IEnumerable<Id>>(), cancellationToken), Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
