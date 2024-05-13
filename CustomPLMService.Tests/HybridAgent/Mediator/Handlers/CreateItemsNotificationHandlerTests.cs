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
using ItemCreateSpec = CustomPLMService.Contract.Models.Items.ItemCreateSpec;
using ItemResult = CustomPLMService.Contract.Models.Items.ItemResult;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class CreateItemsNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<CreateItemsNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<ItemResultEx>> grpcClientResponseStream = new();

    private readonly CreateItemsNotificationHandler handler;

    public CreateItemsNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnCreateItems(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new CreateItemsNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.CreateItems(It.IsAny<IEnumerable<ItemCreateSpec>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new ItemResult(), new ItemResult(), new ItemResult()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new CreateItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new ItemCreateRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientResponseStream.Verify(m => m.WriteAsync(
            It.Is<ItemResultEx>(item=>item.CorrelationId == notification.CorrelationId), cancellationToken),
            Times.Exactly(3));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.CreateItems(It.IsAny<IEnumerable<ItemCreateSpec>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ItemResult>);

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new CreateItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new ItemCreateRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m => m.CreateItems(It.IsAny<IEnumerable<ItemCreateSpec>>(), cancellationToken), Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
