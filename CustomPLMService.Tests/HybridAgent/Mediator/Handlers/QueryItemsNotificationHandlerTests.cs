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
using Id = CustomPLMService.Contract.Models.Items.Id;
using Query = CustomPLMService.Contract.Models.Query.Query;
using Type = CustomPLMService.Contract.Models.Metadata.Type;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class QueryItemsNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<QueryItemsNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<IdEx>> grpcClientResponseStream = new();

    private readonly QueryItemsNotificationHandler handler;

    public QueryItemsNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnQueryItems(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new QueryItemsNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.QueryItems(
                It.IsAny<Query>(),
                It.IsAny<Type>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Id(), new Id(), new Id(), new Id()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new QueryItemsNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new QueryItemsRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnQueryItems(
            It.Is<Metadata>(metadata => metadata.Get(Constants.CorrelationIdKey).Value == notification.CorrelationId),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        grpcClientResponseStream.Verify(m => m.WriteAsync(It.IsAny<IdEx>(), cancellationToken), Times.Exactly(4));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.QueryItems(
                It.IsAny<Query>(),
                It.IsAny<Type>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Id>());

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new QueryItemsNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new QueryItemsRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m => m.QueryItems(It.IsAny<Query>(), It.IsAny<Type>(), cancellationToken), Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
