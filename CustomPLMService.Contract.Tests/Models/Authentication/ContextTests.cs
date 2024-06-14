using CustomPLMService.Contract.Models.Authentication;
using FluentAssertions;
namespace CustomPLMService.Contract.Tests;

public class ContextTests
{
    [Fact]
    public void FromAuth_AuthTokenProvided_OnlyAuthTokenSet()
    {
        // Arrange
        var context = new Context();
        var auth = new Auth
        {
            AuthToken = "Test Token"
        };

        // Act
        context.FromAuth(auth);

        // Assert
        context.Token.Should().Be(auth.AuthToken);
        context.Credentials.Should().BeNull();
        context.Initialized.Should().BeTrue();
    }

    [Fact]
    public void FromAuth_CredentialsProvided_OnlyCredentialsSet()
    {
        // Arrange
        var context = new Context();
        var auth = new Auth
        {
            Credentials = new Credentials
            {
                Username = "Test User",
                Password = "Test Password"
            }
        };

        // Act
        context.FromAuth(auth);

        // Assert
        context.Token.Should().BeNull();
        context.Credentials.Should().NotBeNull();
        context.Credentials.Username.Should().Be(auth.Credentials.Username);
        context.Credentials.Password.Should().Be(auth.Credentials.Password);
        context.Initialized.Should().BeTrue();
    }

    [Fact]
    public void FromAuth_AuthTokenAndCredentialsProvided_AuthTokenAndCredentialsSet()
    {
        // Arrange
        var context = new Context();
        var auth = new Auth
        {
            AuthToken = "Test Token",
            Credentials = new Credentials
            {
                Username = "Test User",
                Password = "Test Password"
            }
        };

        // Act
        context.FromAuth(auth);

        // Assert
        context.Token.Should().Be(auth.AuthToken);
        context.Credentials.Should().NotBeNull();
        context.Credentials.Username.Should().Be(auth.Credentials.Username);
        context.Credentials.Password.Should().Be(auth.Credentials.Password);
        context.Initialized.Should().BeTrue();
    }

    [Fact]
    public void FromAuth_ContextAlreadyInitialized_ThrowsException()
    {
        // Arrange
        var context = new Context();
        var auth = new Auth
        {
            AuthToken = "Test Token",
        };

        // Act
        context.FromAuth(auth); // set initialized
        var action = () => context.FromAuth(auth);

        // Assert
        context.Initialized.Should().BeTrue();
        action.Should().Throw<Exception>().WithMessage("Context already initialized");
    }
}
