using System;
using System.Diagnostics.CodeAnalysis;
using CustomPLMService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace FilesystemPLMDriver
{
    [ExcludeFromCodeCoverage]
    public class Startup(IConfiguration configuration)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MetadataConfig>(options => configuration.GetSection(MetadataConfig.Key).Bind(options));
            services.AddSingleton(new ItemRepository(Environment.CurrentDirectory));

            services.AddPLMServices<FileSystemPlmMetadataService, FileSystemPlmService>();

            // Comment if you don't want to use Hybrid Agent mode (Generic Connector opens connection to Altium 365)
            services.AddHybridAgent(configuration, true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Comment if you dont want to use Classic mode (Generic Connector acting as a server)
            app.UsePLMServices(env);
        }
    }
}
