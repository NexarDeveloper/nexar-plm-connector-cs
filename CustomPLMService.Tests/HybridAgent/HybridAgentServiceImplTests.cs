using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CustomPLMService.HybridAgent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace CustomPLMService.Tests.HybridAgent;

[ExcludeFromCodeCoverage]
public class HybridAgentServiceImplTests
{
    private readonly Mock<IHybridAgent> hybridAgentMock = new();
    private readonly HybridAgentServiceImpl hybridAgentServiceImpl;

    public HybridAgentServiceImplTests()
    {
        Mock<ILogger<HybridAgentServiceImpl>> logger = new();
        hybridAgentServiceImpl = new HybridAgentServiceImpl(hybridAgentMock.Object, logger.Object);
    }

    [Fact]
    public async Task StartAsync_HybridAgentStartedWithCancellationToken()
    {
        // Arrange

        // Act
        await hybridAgentServiceImpl.StartAsync(CancellationToken.None);

        // Assert
        hybridAgentMock.Verify(m => m.Run(It.Is<CancellationToken>(ct => ct != CancellationToken.None)));
    }

    [Fact]
    public async Task StopAsync_UsesCancellationTokenProvidedOnStart()
    {
        // Arrange
        var testedCancellationToken = CancellationToken.None;
        hybridAgentMock.Setup(m => m.Run(It.IsAny<CancellationToken>())).Callback((CancellationToken ct) => { testedCancellationToken = ct; });
        await hybridAgentServiceImpl.StartAsync(CancellationToken.None);

        // Act
        await hybridAgentServiceImpl.StopAsync(CancellationToken.None);

        // Assert
        testedCancellationToken.Should().NotBe(CancellationToken.None);
        testedCancellationToken.IsCancellationRequested.Should().BeTrue();
    }
}
