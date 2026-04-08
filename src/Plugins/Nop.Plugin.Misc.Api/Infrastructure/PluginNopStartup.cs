using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.Api.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Controllers;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        Console.WriteLine(">>> PluginNopStartup.ConfigureServices called <<<");

        services.AddMvcCore()
                .AddApplicationPart(typeof(PluginNopStartup).Assembly);
        // services.AddAutoMapper(typeof(AutoMapperConfiguration).Assembly);

        //services.AddMvc()
        //        .AddApplicationPart(typeof(ProductApiController).Assembly);

        //services.AddMvc()
        //    .AddApplicationPart(typeof(ProductApiAdminController).Assembly);

    }

    public void Configure(IApplicationBuilder application)
    {
        // Nothing to configure
    }

    public int Order => 3000;
}