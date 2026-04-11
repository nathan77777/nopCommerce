using Microsoft.OpenApi.Models;

namespace Nop.Plugin.Misc.Api.Infrastructure;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

/// <summary>
/// Plugin startup class for NopCommerce API.
/// </summary>
public class PluginNopStartup : INopStartup
{
    /// <summary>
    /// Configures the services for the plugin.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        Console.WriteLine(">>> API Configuration initialized <<<");

        services.AddMvcCore()
                .AddApplicationPart(typeof(PluginNopStartup).Assembly);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1-product-admin-api", new OpenApiInfo
            {
                Title = "Plugin for NopCommerce Product Management",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter Raw Token only (without 'Bearer '). Swagger will add the prefix automatically.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    /// <summary>
    /// Configures the application for the plugin.
    /// </summary>
    /// <param name="application">The application builder.</param>
    public void Configure(IApplicationBuilder application)
    {
        application.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        application.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? "";
            if (path.Equals("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/swagger/", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/swagger/html", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/swagger/index.html");
                return;
            }
            await next();
        });

        application.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            options.SwaggerEndpoint("/swagger/v1-product-admin-api/swagger.json", "Mon Plugin API v1");
        });
    }

    /// <summary>
    /// Gets the order of the startup configuration.
    /// </summary>
    public int Order => 10;
}