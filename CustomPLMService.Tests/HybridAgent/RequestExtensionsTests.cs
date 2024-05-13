using System;
using System.Diagnostics.CodeAnalysis;
using Altium.PLM.Custom;
using Altium.PLM.Custom.Reverse;
using CustomPLMService.HybridAgent;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using FluentAssertions;
using Xunit;
namespace CustomPLMService.Tests.HybridAgent;

[ExcludeFromCodeCoverage]
public class RequestExtensionsTests
{

    [Fact]
    public void AsNotification_TestAccessParameterSet_TestAccessNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            TestAccess = new Auth()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<TestAccessNotification>();
        ((TestAccessNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((TestAccessNotification)notification).Request.Should().Be(request.TestAccess);
    }

    [Fact]
    public void AsNotification_AdvanceStateParameterSet_AdvanceStateNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            AdvanceState = new AdvanceStateRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<AdvanceStateNotification>();
        ((AdvanceStateNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((AdvanceStateNotification)notification).Request.Should().Be(request.AdvanceState);
    }

    [Fact]
    public void AsNotification_IsOperationSupportedParameterSet_IsOperationSupportedNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            IsOperationSupported = new OperationSupportedRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<IsOperationSupportedNotification>();
        ((IsOperationSupportedNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((IsOperationSupportedNotification)notification).Request.Should().Be(request.IsOperationSupported);
    }

    [Fact]
    public void AsNotification_CreateRelationshipsParameterSet_CreateRelationshipsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            CreateRelationships = new CreateRelationshipsRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<CreateRelationshipsNotification>();
        ((CreateRelationshipsNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((CreateRelationshipsNotification)notification).Request.Should().Be(request.CreateRelationships);
    }

    [Fact]
    public void AsNotification_ReadRelationshipsParameterSet_ReadRelationshipsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            ReadRelationships = new RelationshipRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<ReadRelationshipNotification>();
        ((ReadRelationshipNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((ReadRelationshipNotification)notification).Request.Should().Be(request.ReadRelationships);
    }

    [Fact]
    public void AsNotification_UploadFileParameterSet_UploadFileNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            UploadFile = new FileResource()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<UploadFileNotification>();
        ((UploadFileNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((UploadFileNotification)notification).Request.Should().Be(request.UploadFile);
    }

    [Fact]
    public void AsNotification_CreateItemsParameterSet_CreateItemsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            CreateItems = new ItemCreateRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<CreateItemsNotification>();
        ((CreateItemsNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((CreateItemsNotification)notification).Request.Should().Be(request.CreateItems);
    }

    [Fact]
    public void AsNotification_DeleteItemsParameterSet_DeleteItemsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            DeleteItems = new IdRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<DeleteItemsNotification>();
        ((DeleteItemsNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((DeleteItemsNotification)notification).Request.Should().Be(request.DeleteItems);
    }

    [Fact]
    public void AsNotification_QueryItemsParameterSet_QueryItemsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            QueryItems = new QueryItemsRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<QueryItemsNotification>();
        ((QueryItemsNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((QueryItemsNotification)notification).Request.Should().Be(request.QueryItems);
    }

    [Fact]
    public void AsNotification_ReadItemsParameterSet_ReadItemsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            ReadItems = new IdRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<ReadItemsNotification>();
        ((ReadItemsNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((ReadItemsNotification)notification).Request.Should().Be(request.ReadItems);
    }

    [Fact]
    public void AsNotification_UpdateItemsParameterSet_UpdateItemsNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            UpdateItems = new ItemUpdateRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<UpdateItemsNotification>();
        ((UpdateItemsNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((UpdateItemsNotification)notification).Request.Should().Be(request.UpdateItems);
    }

    [Fact]
    public void AsNotification_ReadTypesParameterSet_ReadTypesNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            ReadTypes = new TypeIdRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<ReadTypesNotification>();
        ((ReadTypesNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((ReadTypesNotification)notification).Request.Should().Be(request.ReadTypes);
    }

    [Fact]
    public void AsNotification_ReadTypeIdentifiersParameterSet_ReadTypeIdentifiersNotificationReturned()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId",
            ReadTypeIdentifiers = new TypeRequest()
        };

        // Act
        var notification = request.AsNotification();

        // Assert
        notification.Should().NotBeNull();
        notification.Should().BeOfType<ReadTypeIdentifiersNotification>();
        ((ReadTypeIdentifiersNotification)notification).CorrelationId.Should().Be(request.CorrelationId);
        ((ReadTypeIdentifiersNotification)notification).Request.Should().Be(request.ReadTypeIdentifiers);
    }

    [Fact]
    public void AsNotification_NoParameterSet_ExceptionThrown()
    {
        // Arrange
        var request = new Request
        {
            CorrelationId = "TestCorrelationId"
        };

        // Act
        Action action = () => request.AsNotification();

        // Assert
        action.Should().ThrowExactly<RequestMappingFailedException>().Where(e => e.Request == request);
    }
}
