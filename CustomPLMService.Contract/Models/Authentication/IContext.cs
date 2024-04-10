namespace CustomPLMService.Contract.Models.Authentication;

public interface IContext
{
    string Token { get; }
    Credentials Credentials { get; }
    bool Initialized { get; }
    void FromAuth(Auth auth);
}
