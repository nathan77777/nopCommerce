using Microsoft.OpenApi;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.Api.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Controllers;
using System;

/// <summary>
/// Startup class for the plugin.
/// </summary>
public class PluginNopStartup : INopStartup
{
    /// <summary>
    /// Configures services for the plugin.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        Console.WriteLine(">>> PluginNopStartup.ConfigureServices called <<<");

        services.AddMvcCore()
                .AddApplicationPart(typeof(PluginNopStartup).Assembly);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1-product-admin-api", new OpenApiInfo
            {
                Title = "Plugin for NopCommerce Product Management",
                Version = "v1"
            });
        });

    }

    /// <summary>
    /// Configures the application's request pipeline for the plugin.
    /// </summary>
    /// <param name="application">The application builder</param>
    public void Configure(IApplicationBuilder application)
    {
        application.UseSwagger();
        application.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1-product-admin-api/swagger.json", "Mon Plugin API v1");
        });
    }

    public int Order => 3000;
}