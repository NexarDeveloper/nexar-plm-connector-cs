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
using RelationshipTable = CustomPLMService.Contract.Models.Relationship.RelationshipTable;
using RelationshipType = CustomPLMService.Contract.Models.Metadata.RelationshipType;
using Void = Altium.PLM.Custom.Void;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class ReadRelationshipNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<ReadRelationshipNotificationHandler>> loggerMock = new();
    private readonly Mock<IClientStreamWriter<RelationshipTableEx>> grpcClientResponseStream = new();

    private readonly ReadRelationshipNotificationHandler handler;

    public ReadRelationshipNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncClientStreamingCall(
            grpcClientResponseStream.Object,
            Task.FromResult(new Void()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnReadRelationships(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new ReadRelationshipNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.ReadRelationships(
                It.IsAny<IEnumerable<Id>>(),
                It.IsAny<RelationshipType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new RelationshipTable(), new RelationshipTable(), new RelationshipTable()
            });

        var cancellationToken = (new CancellationTokenSource()).Token;
        var notification = new ReadRelationshipNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new RelationshipRequest()
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientResponseStream.Verify(m => m.WriteAsync(
                It.Is<RelationshipTableEx>(item => item.CorrelationId == notification.CorrelationId), cancellationToken),
            Times.Exactly(3));
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        plmServiceMock.Setup(m => m.ReadRelationships(
                It.IsAny<IEnumerable<Id>>(),
                It.IsAny<RelationshipType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RelationshipTable>());

        var cancellationToken = (new CancellationTokenSource()).Token;
        var relationshipRequest = new RelationshipRequest();
        relationshipRequest.Ids.Add(new Altium.PLM.Custom.Id());
       
        var notification = new ReadRelationshipNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = relationshipRequest
        };

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m => m.ReadRelationships(
            It.IsAny<IEnumerable<Id>>(), 
            It.IsAny<RelationshipType>(), 
            cancellationToken), 
            Times.Once());
        grpcClientResponseStream.Verify(m => m.CompleteAsync(), Times.Once);
    }
}
