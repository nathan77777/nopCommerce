namespace Nop.Plugin.Misc.Api.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        Console.WriteLine(">>> RouteProvider.RegisterRoutes called <<<");

        endpointRouteBuilder.MapGet("api/debug", () => "Plugin is alive!");

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Api.Products",
            pattern: "api/products",
            defaults: new
            {
                controller = "ProductApi",
                action = "GetRecentlyViewedProductsAsync",
                area = string.Empty
            }
        );
    }

    public int Priority => -1;
}