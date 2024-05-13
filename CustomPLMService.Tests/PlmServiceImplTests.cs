using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
namespace CustomPLMService.Tests;

[ExcludeFromCodeCoverage]
public class PlmServiceImplTests
{
    private readonly Mock<ICustomPlmService> serviceMock = new();
    private readonly Mock<ICustomPlmMetadataService> metadataServiceMock = new();
    private readonly Mock<ILogger<PlmServiceImpl>> loggerMock = new();
    private readonly ServerCallContext serverCallContext;

    private readonly PlmServiceImpl plmServiceImpl;

    public PlmServiceImplTests()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new PlmServiceMappingProfile());
        });

        var mapper = mappingConfig.CreateMapper();

        serverCallContext = TestServerCallContext.Create("testCall", "testHost", DateTime.Now.AddMinutes(1), [], CancellationToken.None,
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
        serviceMock.Setup(m => m.TestAccess(It.IsAny<Contract.Models.Authentication.Auth>(), It.IsAny<CancellationToken>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await plmServiceImpl.TestAccess(request, serverCallContext);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().Be(expectedResponse);
        result.Status.Should().Be(expectedStatus);
        serviceMock.Verify(m => m.TestAccess(It.Is<Contract.Models.Authentication.Auth>(r => r.AuthToken == request.AuthToken), It.IsAny<CancellationToken>()), Times.Once);
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
        serviceMock.Setup(m => m.IsOperationSupported(It.Is<SupportedOperation>(operation => operation == SupportedOperation.CreateInfoNumbering), It.IsAny<CancellationToken>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await plmServiceImpl.IsOperationSupported(request, serverCallContext);

        // Assert
        result.Should().NotBeNull();
        result.IsSupported.Should().Be(expectedResponse);
        serviceMock.Verify(m => m.IsOperationSupported(It.Is<SupportedOperation>(operation => operation == SupportedOperation.CreateInfoNumbering), It.IsAny<CancellationToken>()), Times.Once);
    }
}
