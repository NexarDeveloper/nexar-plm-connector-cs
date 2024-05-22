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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePLMServices(env);
        }
    }
}
