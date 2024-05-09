using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using CustomPLMService.Configs;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Interceptors;
using CustomPLMService.Middleware;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace CustomPLMService;

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
    
    public static IServiceCollection AddHybridAgent(this IServiceCollection services, IConfiguration configuration)
    {
        var hybridAgentConfig = new HybridAgentConfig();
        configuration.GetSection(HybridAgentConfig.Key).Bind(hybridAgentConfig);
        
        services.Configure<HybridAgentConfig>(options =>
            configuration.GetSection(HybridAgentConfig.Key).Bind(options));
        
        services.AddTransient<IHybridAgent, HybridAgent>();
        services.AddSingleton<IHostedService, HybridAgentServiceImpl>();
        services.AddGrpcClient<ReversePLMService.ReversePLMServiceClient>(o =>
            {
                o.Address = new Uri(hybridAgentConfig.Uri);
                o.CallOptionsActions.Add(opt => opt.CallOptions.WithDeadline(DateTime.UtcNow.AddSeconds(hybridAgentConfig.DeadlineInSeconds)));
            })
            .AddCallCredentials((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(hybridAgentConfig.ApiKey))
                {
                    metadata.Add("Authorization", $"{hybridAgentConfig.ApiKey}");
                }
                return Task.CompletedTask;
            }).ConfigureChannel(o =>
            {
                var defaultMethodConfig = new MethodConfig
                {
                    Names = { MethodName.Default },
                    RetryPolicy = new RetryPolicy
                    {
                        MaxAttempts = hybridAgentConfig.RetryMaxAttempts,
                        InitialBackoff = TimeSpan.FromSeconds(hybridAgentConfig.RetryInitialBackoffInSeconds),
                        MaxBackoff = TimeSpan.FromSeconds(hybridAgentConfig.RetryMaxBackoffInSeconds),
                        BackoffMultiplier = 1.5,
                        RetryableStatusCodes = { StatusCode.Unavailable }
                    }
                };
                o.ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } };
                o.ThrowOperationCanceledOnCancellation = true;
            });

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
