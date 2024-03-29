namespace CustomPLMService.Contract.Models.Authentication;

/// <summary>
/// Represents credentials in external system.
/// </summary>
public class Credentials
{
    /// <summary>
    /// Account name
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Account password
    /// </summary>
    public string Password { get; set; }
}
