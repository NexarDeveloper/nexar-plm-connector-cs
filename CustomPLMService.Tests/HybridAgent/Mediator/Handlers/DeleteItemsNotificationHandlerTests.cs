using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class DeleteItemsNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<DeleteItemsNotificationHandler>> loggerMock = new();

    private readonly DeleteItemsNotificationHandler handler;

    public DeleteItemsNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Void()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnDeleteItemsAsync(It.IsAny<VoidEx>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new DeleteItemsNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        var notification = new DeleteItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new IdRequest()
        };
        var cancellationToken = (new CancellationTokenSource()).Token;

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnDeleteItemsAsync(
                It.IsAny<VoidEx>(),
                It.Is<Metadata>(metadata=>metadata.Get(Constants.CorrelationIdKey).Value == notification.CorrelationId),
                It.IsAny<DateTime?>(),
                cancellationToken)
            , Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        var cancellationToken = (new CancellationTokenSource()).Token;

        var request = new IdRequest();
        request.Data.AddRange(new[]
        {
            new Altium.PLM.Custom.Id(), new Altium.PLM.Custom.Id()
        });
        var notification = new DeleteItemsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = request
        };

        mapperMock.Setup(m => m.Map<Id>(It.IsAny<Altium.PLM.Custom.Id>())).Returns(new Id());

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m =>
                m.DeleteItems(It.Is<IEnumerable<Id>>(
                        ids => ids.Count() == 2),
                    cancellationToken),
            Times.Once);
    }
}
