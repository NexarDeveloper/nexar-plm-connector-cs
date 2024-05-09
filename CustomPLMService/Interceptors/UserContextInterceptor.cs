using System.Threading.Tasks;
using AutoMapper;
using CustomPLMService.Contract.Models.Authentication;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Auth = Altium.PLM.Custom.Auth;
namespace CustomPLMService.Interceptors;

public class UserContextInterceptor(IContext userContext, IMapperBase mapper) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        HandleAuth(request);
        return await continuation(request, context);
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        HandleAuth(request);
        return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
    }

    private void HandleAuth<TRequest>(TRequest request)
    {
        var authPropertyInfo = request.GetType().GetProperty("Auth");

        if (authPropertyInfo?.GetValue(request) is Auth auth)
        {
            userContext.FromAuth(mapper.Map<CustomPLMService.Contract.Models.Authentication.Auth>(auth));
        }
    }
}
