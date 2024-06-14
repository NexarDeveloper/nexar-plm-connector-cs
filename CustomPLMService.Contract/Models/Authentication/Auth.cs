using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Authentication;

/// <summary>
/// Provides context for user authentication.
/// </summary>
[ExcludeFromCodeCoverage]
public class Auth
{
    /// <summary>
    /// External system instance URL
    /// </summary>
    public string PlmUrl { get; set; }

    /// <summary>
    /// Nexus session identifier
    /// </summary>
    public string AuthToken { get; set; }

    /// <summary>
    /// External system account credentials (see <see cref="Credentials"/>)
    /// </summary>
    public Credentials Credentials { get; set; }

    public List<string> Licenses { get; set; } = [];

    public string Context { get; set; }
}
