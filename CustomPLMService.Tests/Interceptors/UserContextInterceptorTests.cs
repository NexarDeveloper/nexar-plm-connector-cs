using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoMapper;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Interceptors;
using Grpc.Core;
using Moq;
using Xunit;
using Auth = Altium.PLM.Custom.Auth;
using Credentials = Altium.PLM.Custom.Credentials;
namespace CustomPLMService.Tests.Interceptors;

[ExcludeFromCodeCoverage]
public class UserContextInterceptorTests
{
    private readonly Mock<IContext> userContextMock = new();
    private readonly Mock<ServerCallContext> serverCallContextMock = new();

    private readonly UserContextInterceptor contextInterceptor;

    public UserContextInterceptorTests()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new PlmServiceMappingProfile());
        });

        var mapper = mappingConfig.CreateMapper();
        contextInterceptor = new UserContextInterceptor(userContextMock.Object, mapper);
    }

    [Fact]
    public async Task UnaryServerHandler_NoAuthPresent_ContinuationCalled()
    {
        // Arrange
        var nextMock = new Mock<UnaryServerMethod<RequestWithoutAuth, Response>>();
        var request = new RequestWithoutAuth
        {
            SomeField = "TestValue"
        };

        // Act
        await contextInterceptor.UnaryServerHandler(request, serverCallContextMock.Object, nextMock.Object);

        // Assert
        userContextMock.Verify(m => m.FromAuth(It.IsAny<Contract.Models.Authentication.Auth>()), Times.Never);
        nextMock.Verify(m => m.Invoke(request, serverCallContextMock.Object), Times.Once);
    }

    [Fact]
    public async Task UnaryServerHandler_IncorrectlyNamedAuthPresent_ContinuationCalled()
    {
        // Arrange
        var nextMock = new Mock<UnaryServerMethod<RequestWithIncorrectlyNamedAuth, Response>>();
        var request = new RequestWithIncorrectlyNamedAuth
        {
            SomeField = "TestValue",
            AuthField = BuildAuthField("TestUser", "TestPassword")
        };

        // Act
        await contextInterceptor.UnaryServerHandler(request, serverCallContextMock.Object, nextMock.Object);

        // Assert
        userContextMock.Verify(m => m.FromAuth(It.IsAny<Contract.Models.Authentication.Auth>()), Times.Never);
        nextMock.Verify(m => m.Invoke(request, serverCallContextMock.Object), Times.Once);
    }

    [Fact]
    public async Task UnaryServerHandler_AuthPresent_UserContextBuildAndContinuationCalled()
    {
        // Arrange
        var nextMock = new Mock<UnaryServerMethod<RequestWithAuth, Response>>();
        var request = new RequestWithAuth
        {
            SomeField = "TestValue",
            Auth = BuildAuthField("TestUser", "TestPassword")
        };

        // Act
        await contextInterceptor.UnaryServerHandler(request, serverCallContextMock.Object, nextMock.Object);

        // Assert
        userContextMock.Verify(
            m => m.FromAuth(It.Is<Contract.Models.Authentication.Auth>(
                auth => auth.Credentials.Username == request.Auth.Credentials.Username &&
                    auth.Credentials.Password == request.Auth.Credentials.Password)
            ), Times.Once);
        nextMock.Verify(m => m.Invoke(request, serverCallContextMock.Object), Times.Once);
    }

    [Fact]
    public async Task ServerStreamingServerHandler_NoAuthPresent_ContinuationCalled()
    {
        // Arrange
        var responseStreamMock = new Mock<IServerStreamWriter<Response>>();
        var nextMock = new Mock<ServerStreamingServerMethod<RequestWithoutAuth, Response>>();
        var request = new RequestWithoutAuth
        {
            SomeField = "TestValue"
        };

        // Act
        await contextInterceptor.ServerStreamingServerHandler(request, responseStreamMock.Object, serverCallContextMock.Object, nextMock.Object);

        // Assert
        userContextMock.Verify(m => m.FromAuth(It.IsAny<Contract.Models.Authentication.Auth>()), Times.Never);
        nextMock.Verify(m => m.Invoke(request, responseStreamMock.Object, serverCallContextMock.Object), Times.Once);
    }

    [Fact]
    public async Task ServerStreamingServerHandler_IncorrectlyNamedAuthPresent_ContinuationCalled()
    {
        // Arrange
        var responseStreamMock = new Mock<IServerStreamWriter<Response>>();
        var nextMock = new Mock<ServerStreamingServerMethod<RequestWithIncorrectlyNamedAuth, Response>>();
        var request = new RequestWithIncorrectlyNamedAuth
        {
            SomeField = "TestValue",
            AuthField = BuildAuthField("TestUser", "TestPassword")
        };

        // Act
        await contextInterceptor.ServerStreamingServerHandler(request, responseStreamMock.Object, serverCallContextMock.Object, nextMock.Object);

        // Assert
        userContextMock.Verify(m => m.FromAuth(It.IsAny<Contract.Models.Authentication.Auth>()), Times.Never);
        nextMock.Verify(m => m.Invoke(request, responseStreamMock.Object, serverCallContextMock.Object), Times.Once);
    }

    [Fact]
    public async Task ServerStreamingServerHandler_AuthPresent_UserContextBuildAndContinuationCalled()
    {
        // Arrange
        var responseStreamMock = new Mock<IServerStreamWriter<Response>>();
        var nextMock = new Mock<ServerStreamingServerMethod<RequestWithAuth, Response>>();
        var request = new RequestWithAuth
        {
            SomeField = "TestValue",
            Auth = BuildAuthField("TestUser", "TestPassword")
        };

        // Act
        await contextInterceptor.ServerStreamingServerHandler(request, responseStreamMock.Object, serverCallContextMock.Object, nextMock.Object);

        // Assert
        userContextMock.Verify(
            m => m.FromAuth(It.Is<Contract.Models.Authentication.Auth>(
                auth => auth.Credentials.Username == request.Auth.Credentials.Username &&
                    auth.Credentials.Password == request.Auth.Credentials.Password)
            ), Times.Once);
        nextMock.Verify(m => m.Invoke(request, responseStreamMock.Object, serverCallContextMock.Object), Times.Once);
    }

    private static Auth BuildAuthField(string username, string password)
    {
        return new Auth
        {
            Credentials = new Credentials
            {
                Username = username,
                Password = password
            }
        };
    }

    public class RequestWithoutAuth
    {
        public string SomeField { get; set; }
    }

    public class RequestWithAuth
    {
        public string SomeField { get; set; }
        public Auth Auth { get; set; }
    }

    public class RequestWithIncorrectlyNamedAuth
    {
        public string SomeField { get; set; }
        public Auth AuthField { get; set; }
    }

    public class Response
    {
        public string SomeField { get; set; }
    }
}
