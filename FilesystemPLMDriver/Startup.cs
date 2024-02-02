using CustomPLMService;
using CustomPLMService.Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CustomPLMDriver
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration) => this.configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var metadataConfig = configuration.GetSection("Metadata").Get<MetadataConfig>();

            CustomPLMService.Converter.Init();

            services.AddGrpc();

            services.AddSingleton<ICustomPlmMetadataService>(new FileSystemPlmMetadataService(metadataConfig));
            services.AddSingleton<ICustomPlmService, FileSystemPlmService>();
            services.AddSingleton(new ItemRepository(Environment.CurrentDirectory));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ExceptionHandlingGrpcService>();
            });
        }
    }
}
