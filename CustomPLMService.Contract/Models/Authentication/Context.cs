using System;
namespace CustomPLMService.Contract.Models.Authentication;

/// <summary>
/// Provides context for authentication in external system.
/// </summary>
public class Context : IContext
{
    public string Token { get; private set; }
    public Credentials Credentials { get; private set; }
    public bool Initialized { get; private set; } = false;

    public void FromAuth(Auth auth)
    {
        if (Initialized)
        {
            throw new Exception("Context already initialized");
        }
        
        if (auth?.AuthToken is not null)
        {
            Token = auth.AuthToken;
        }
        
        if (auth?.Credentials is not null)
        {
            Credentials = new Credentials
            {
                Username = auth.Credentials.Username,
                Password = auth.Credentials.Password,
            };
        }
        
        Initialized = true;
    }
}
