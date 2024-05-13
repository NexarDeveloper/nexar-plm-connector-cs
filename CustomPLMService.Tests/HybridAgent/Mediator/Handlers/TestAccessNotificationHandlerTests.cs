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
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class TestAccessNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<TestAccessNotificationHandler>> loggerMock = new();

    private readonly TestAccessNotificationHandler handler;

    public TestAccessNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Altium.PLM.Custom.Void()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnTestAccessAsync(It.IsAny<AuthResultEx>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new TestAccessNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Theory]
    [InlineData(true, true, AuthResult.Types.Status.Success)]
    [InlineData(false, false, AuthResult.Types.Status.InvalidCredentials)]
    public async Task Handle_CallsGrpcService(bool serviceResponse, bool expectedResponse, AuthResult.Types.Status expectedStatus)
    {
        // Arrange
        var notification = new TestAccessNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new Auth()
        };
        var cancellationToken = (new CancellationTokenSource()).Token;
        plmServiceMock.Setup(m => m.TestAccess(
                It.IsAny<Contract.Models.Authentication.Auth>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResponse);

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnTestAccessAsync(
                It.Is<AuthResultEx>(v =>
                    v.CorrelationId == notification.CorrelationId &&
                    v.Value.Success == expectedResponse &&
                    v.Value.Status == expectedStatus),
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
        var notification = new TestAccessNotification()
        {
            CorrelationId = "TestCorrelationId",
            Request = new Auth()
        };

        var mappedAuth = new CustomPLMService.Contract.Models.Authentication.Auth
        {
            AuthToken = "TestToken"
        };
        mapperMock.Setup(m => m.Map<CustomPLMService.Contract.Models.Authentication.Auth>(It.IsAny<Auth>())).Returns(mappedAuth);

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m =>
                m.TestAccess(It.Is<CustomPLMService.Contract.Models.Authentication.Auth>(id => id.AuthToken == mappedAuth.AuthToken),
                    cancellationToken),
            Times.Once);
    }
}
