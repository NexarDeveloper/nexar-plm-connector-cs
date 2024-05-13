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
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class CreateRelationshipsNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<CreateRelationshipsNotificationHandler>> loggerMock = new();

    private readonly CreateRelationshipsNotificationHandler handler;

    public CreateRelationshipsNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Altium.PLM.Custom.Void()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnCreateRelationshipsAsync(It.IsAny<VoidEx>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new CreateRelationshipsNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        var notification = new CreateRelationshipsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new CreateRelationshipsRequest()
        };
        var cancellationToken = (new CancellationTokenSource()).Token;

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnCreateRelationshipsAsync(
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
        var mappedRelationship = new Contract.Models.Relationship.RelationshipTable()
        {
            Id = new Id
            {
                PublicId = "TestRelationshipId"
            }
        };
        var request = new CreateRelationshipsRequest();
        request.Relationships.Add(new RelationshipTable
        {
            Id = new Altium.PLM.Custom.Id
            {
                PublicId = "TestRelationshipId"
            }
        });
        var notification = new CreateRelationshipsNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = request
        };

        mapperMock.Setup(m => m.Map<CustomPLMService.Contract.Models.Relationship.RelationshipTable>(It.IsAny<RelationshipTable>())).Returns(mappedRelationship);

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m =>
                m.CreateRelationships(It.Is<IEnumerable<CustomPLMService.Contract.Models.Relationship.RelationshipTable>>(
                        relationshipTables => relationshipTables.First().Id.PublicId == mappedRelationship.Id.PublicId),
                    cancellationToken),
            Times.Once);
    }
}
