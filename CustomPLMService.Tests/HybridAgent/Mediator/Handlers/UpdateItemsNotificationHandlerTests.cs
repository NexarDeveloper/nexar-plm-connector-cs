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
using ItemResult = CustomPLMService.Contract.Models.Items.ItemResult;
using ItemUpdateSpec = CustomPLMService.Contract.Models.Items.ItemUpdateSpec;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class UpdateItemsNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<UpdateItemsNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<ItemResultEx>> grpcClientResponseStream = new();

    private readonly UpdateItemsNotificationHandler handler;

    public UpdateItemsNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnUpdateItems(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new UpdateItemsNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.UpdateItems(
                It.IsAny<IEnumerable<ItemUpdateSpec>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new ItemResult(), new ItemResult(), new ItemResult()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new UpdateItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new ItemUpdateRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnUpdateItems(
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>())
            , Times.Once);
        grpcClientResponseStream.Verify(m => m.WriteAsync(
                It.IsAny<ItemResultEx>(), cancellationToken),
            Times.Exactly(3));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.UpdateItems(It.IsAny<IEnumerable<ItemUpdateSpec>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ItemResult>);

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new UpdateItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new ItemUpdateRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m => m.UpdateItems(It.IsAny<IEnumerable<ItemUpdateSpec>>(), cancellationToken), Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
