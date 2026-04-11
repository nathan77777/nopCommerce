using Microsoft.OpenApi.Models;

namespace Nop.Plugin.Misc.Api.Infrastructure;

using System;
using Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Nop.Core.Infrastructure;

public class PluginNopStartup : INopStartup
{
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
                Description = "Entrez uniquement le token JWT brut (sans 'Bearer '). Swagger ajoutera le préfixe automatiquement.",
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

    public int Order => 10;
}