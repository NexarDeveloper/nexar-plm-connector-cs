using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Type = Altium.PLM.Custom.Type;
namespace CustomPLMService.Tests;

[ExcludeFromCodeCoverage]
public class PlmServiceImplTests
{
    private readonly Mock<ICustomPlmService> serviceMock = new();
    private readonly Mock<ICustomPlmMetadataService> metadataServiceMock = new();
    private readonly Mock<ILogger<PlmServiceImpl>> loggerMock = new();
    private readonly ServerCallContext serverCallContext;
    private readonly CancellationToken cancellationToken;

    private readonly PlmServiceImpl plmServiceImpl;

    public PlmServiceImplTests()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new PlmServiceMappingProfile());
        });

        var mapper = mappingConfig.CreateMapper();

        cancellationToken = (new CancellationTokenSource()).Token;

        serverCallContext = TestServerCallContext.Create("testCall", "testHost", DateTime.Now.AddMinutes(1), [], cancellationToken,
            "testPeer", new AuthContext(null, new Dictionary<string, List<AuthProperty>>()), null,
            (_ => Task.CompletedTask), () => null, _ => { });
        plmServiceImpl = new PlmServiceImpl(metadataServiceMock.Object, serviceMock.Object, mapper, loggerMock.Object);
    }

    [Theory]
    [InlineData(true, true, AuthResult.Types.Status.Success)]
    [InlineData(false, false, AuthResult.Types.Status.InvalidCredentials)]
    public async Task TestAccess_PassesServiceResponse(bool serviceResponse, bool expectedResponse, AuthResult.Types.Status expectedStatus)
    {
        // Arrange
        var request = new Auth
        {
            AuthToken = "testToken"
        };
        serviceMock.Setup(m => m.TestAccess(It.Is<Contract.Models.Authentication.Auth>(r => r.AuthToken == request.AuthToken), cancellationToken)).ReturnsAsync(serviceResponse);

        // Act
        var result = await plmServiceImpl.TestAccess(request, serverCallContext);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().Be(expectedResponse);
        result.Status.Should().Be(expectedStatus);
        serviceMock.Verify(m => m.TestAccess(It.IsAny<Contract.Models.Authentication.Auth>(), cancellationToken), Times.Once);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task IsOperationSupported_PassesServiceResponse(bool serviceResponse, bool expectedResponse)
    {
        // Arrange
        var request = new OperationSupportedRequest
        {
            Operation = OperationSupportedRequest.Types.Operation.CreateInfoNumbering
        };
        serviceMock.Setup(m => m.IsOperationSupported(It.Is<SupportedOperation>(operation => operation == SupportedOperation.CreateInfoNumbering), cancellationToken)).ReturnsAsync(serviceResponse);

        // Act
        var result = await plmServiceImpl.IsOperationSupported(request, serverCallContext);

        // Assert
        result.Should().NotBeNull();
        result.IsSupported.Should().Be(expectedResponse);
        serviceMock.Verify(m => m.IsOperationSupported(It.IsAny<SupportedOperation>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ReadTypeIdentifiers_PassesServiceResponse()
    {
        // Arrange
        var request = new TypeRequest()
        {
            BaseType = BaseType.Change
        };
        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Metadata.TypeId(), new CustomPLMService.Contract.Models.Metadata.TypeId()
        };
        metadataServiceMock.Setup(m => m.ReadTypeIdentifiers(
                It.Is<CustomPLMService.Contract.Models.Metadata.BaseType>(
                    type => type == Contract.Models.Metadata.BaseType.Change),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<TypeId>>();

        // Act
        await plmServiceImpl.ReadTypeIdentifiers(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<TypeId>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        metadataServiceMock.Verify(m => m.ReadTypeIdentifiers(
            It.IsAny<CustomPLMService.Contract.Models.Metadata.BaseType>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ReadTypes_PassesServiceResponse()
    {
        // Arrange
        var request = new TypeIdRequest
        {
            Data =
            {
                new[]
                {
                    new TypeId(), new TypeId(), new TypeId()
                }
            }
        };
        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Metadata.Type(), new CustomPLMService.Contract.Models.Metadata.Type(), new CustomPLMService.Contract.Models.Metadata.Type()
        };
        metadataServiceMock.Setup(m => m.ReadTypes(
                It.Is<IEnumerable<CustomPLMService.Contract.Models.Metadata.TypeId>>(types => types.Count() == request.Data.Count),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<Type>>();

        // Act
        await plmServiceImpl.ReadTypes(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<Type>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        metadataServiceMock.Verify(m => m.ReadTypes(
            It.IsAny<IEnumerable<CustomPLMService.Contract.Models.Metadata.TypeId>>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ReadItems_PassesServiceResponse()
    {
        // Arrange
        var request = new IdRequest()
        {
            Data =
            {
                new[]
                {
                    new Id(), new Id()
                }
            }
        };
        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Items.Item(), new CustomPLMService.Contract.Models.Items.Item()
        };

        serviceMock.Setup(m => m.ReadItems(
                It.Is<IEnumerable<CustomPLMService.Contract.Models.Items.Id>>(types => types.Count() == request.Data.Count),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<Item>>();

        // Act
        await plmServiceImpl.ReadItems(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<Item>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        serviceMock.Verify(m => m.ReadItems(
            It.IsAny<IEnumerable<CustomPLMService.Contract.Models.Items.Id>>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task QueryItems_PassesServiceResponse()
    {
        // Arrange
        var request = new QueryItemsRequest()
        {
            Type = new Type(),
            Query = new Query()
        };
        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Items.Id(), new CustomPLMService.Contract.Models.Items.Id()
        };

        serviceMock.Setup(m => m.QueryItems(
                It.IsAny<CustomPLMService.Contract.Models.Query.Query>(),
                It.IsAny<CustomPLMService.Contract.Models.Metadata.Type>(),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<Id>>();

        // Act
        await plmServiceImpl.QueryItems(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<Id>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        serviceMock.Verify(m => m.QueryItems(
            It.IsAny<CustomPLMService.Contract.Models.Query.Query>(),
            It.IsAny<CustomPLMService.Contract.Models.Metadata.Type>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateItems_PassesServiceResponse()
    {
        // Arrange
        var request = new ItemCreateRequest()
        {
            Data =
            {
                new[]
                {
                    new ItemCreateSpec(), new ItemCreateSpec()
                }
            }
        };
        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Items.ItemResult(), new CustomPLMService.Contract.Models.Items.ItemResult()
        };

        serviceMock.Setup(m => m.CreateItems(
                It.Is<IEnumerable<CustomPLMService.Contract.Models.Items.ItemCreateSpec>>(types => types.Count() == request.Data.Count),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<ItemResult>>();

        // Act
        await plmServiceImpl.CreateItems(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<ItemResult>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        serviceMock.Verify(m => m.CreateItems(
            It.IsAny<IEnumerable<CustomPLMService.Contract.Models.Items.ItemCreateSpec>>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateItems_PassesServiceResponse()
    {
        // Arrange
        var request = new ItemUpdateRequest()
        {
            Data =
            {
                new[]
                {
                    new ItemUpdateSpec(), new ItemUpdateSpec()
                }
            }
        };
        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Items.ItemResult
            {
                Item = new Contract.Models.Items.Item()
            },
            new CustomPLMService.Contract.Models.Items.ItemResult()
            {
                Item = new Contract.Models.Items.Item()
            }
        };

        serviceMock.Setup(m => m.UpdateItems(
                It.Is<IEnumerable<CustomPLMService.Contract.Models.Items.ItemUpdateSpec>>(types => types.Count() == request.Data.Count),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<ItemResult>>();

        // Act
        await plmServiceImpl.UpdateItems(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<ItemResult>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        serviceMock.Verify(m => m.UpdateItems(
            It.IsAny<IEnumerable<CustomPLMService.Contract.Models.Items.ItemUpdateSpec>>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UploadFile_PassesServiceResponse()
    {
        // Arrange
        var request = new FileResource
        {
            FileName = "Test File"
        };

        const string serviceResponse = "Test File ID";

        serviceMock.Setup(m => m.UploadFile(
                It.Is<CustomPLMService.Contract.Models.Items.FileResource>(file => file.FileName == request.FileName),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await plmServiceImpl.UploadFile(request, serverCallContext);

        // Assert
        result.Id.Should().Be(serviceResponse);
        serviceMock.Verify(m => m.UploadFile(
            It.IsAny<CustomPLMService.Contract.Models.Items.FileResource>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateRelationships_PassesServiceResponse()
    {
        // Arrange
        var request = new CreateRelationshipsRequest
        {
            Relationships =
            {
                new[]
                {
                    new RelationshipTable(), new RelationshipTable()
                }
            }
        };

        // Act
        await plmServiceImpl.CreateRelationships(request, serverCallContext);

        // Assert
        serviceMock.Verify(m => m.CreateRelationships(
            It.Is<IEnumerable<CustomPLMService.Contract.Models.Relationship.RelationshipTable>>(relationships => relationships.Count() == request.Relationships.Count),
            cancellationToken), Times.Once());
    }

    [Fact]
    public async Task DeleteItems_PassesServiceResponse()
    {
        // Arrange
        var request = new IdRequest()
        {
            Data =
            {
                new[]
                {
                    new Id(), new Id()
                }
            }
        };

        // Act
        await plmServiceImpl.DeleteItems(request, serverCallContext);

        // Assert
        serviceMock.Verify(m => m.DeleteItems(
            It.Is<IEnumerable<CustomPLMService.Contract.Models.Items.Id>>(relationships => relationships.Count() == request.Data.Count),
            cancellationToken), Times.Once());
    }

    [Fact]
    public async Task AdvanceState_PassesServiceResponse()
    {
        // Arrange
        var request = new AdvanceStateRequest()
        {
            Id = new Id
            {
                PublicId = "Test Public Id"
            }
        };

        // Act
        await plmServiceImpl.AdvanceState(request, serverCallContext);

        // Assert
        serviceMock.Verify(m => m.AdvanceState(
            It.Is<CustomPLMService.Contract.Models.Items.Id>(id => id.PublicId == request.Id.PublicId),
            cancellationToken), Times.Once());
    }

    [Fact]
    public async Task ReadRelationships_PassesServiceResponse()
    {
        // Arrange
        var request = new RelationshipRequest()
        {
            Type = RelationshipType.Attachments,
            Ids =
            {
                new[]
                {
                    new Id(), new Id()
                }
            }
        };

        var serviceResponse = new[]
        {
            new CustomPLMService.Contract.Models.Relationship.RelationshipTable(), new CustomPLMService.Contract.Models.Relationship.RelationshipTable()
        };

        serviceMock.Setup(m => m.ReadRelationships(
                It.Is<IEnumerable<CustomPLMService.Contract.Models.Items.Id>>(ids => ids.Count() == request.Ids.Count),
                It.Is<CustomPLMService.Contract.Models.Metadata.RelationshipType>(type => type == Contract.Models.Metadata.RelationshipType.Attachments),
                cancellationToken))
            .ReturnsAsync(serviceResponse);

        var responseStreamMock = new Mock<IServerStreamWriter<RelationshipTable>>();

        // Act
        await plmServiceImpl.ReadRelationships(request, responseStreamMock.Object, serverCallContext);

        // Assert
        responseStreamMock.Verify(m => m.WriteAsync(It.IsAny<RelationshipTable>(), cancellationToken), Times.Exactly(serviceResponse.Length));
        serviceMock.Verify(m => m.ReadRelationships(
            It.IsAny<IEnumerable<CustomPLMService.Contract.Models.Items.Id>>(),
            It.IsAny<CustomPLMService.Contract.Models.Metadata.RelationshipType>(),
            cancellationToken), Times.Once);
    }
}
