using System.Diagnostics.CodeAnalysis;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Interceptors;
using CustomPLMService.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace CustomPLMService;

[ExcludeFromCodeCoverage]
public static class CustomPLMServiceExtensions
{
    public static IServiceCollection AddPLMServices<TMetadataService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TMetadataService : class, ICustomPlmMetadataService where TService : class, ICustomPlmService
    {
        services.AddLogging();
        services.AddAutoMapper(typeof(PlmServiceMappingProfile));

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<UserContextInterceptor>();
        });

        services.AddScoped<IContext, Context>();
        services.AddScoped<ICustomPlmMetadataService, TMetadataService>();
        services.AddScoped<ICustomPlmService, TService>();

        services.AddTransient<GlobalExceptionHandler>();

        return services;
    }

    public static IApplicationBuilder UsePLMServices(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<PlmServiceImpl>();
        });
        return app;
    }
}
