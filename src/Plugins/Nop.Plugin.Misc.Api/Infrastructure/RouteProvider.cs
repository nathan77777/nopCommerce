namespace Nop.Plugin.Misc.Api.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

/// <summary>
/// Provides route configuration for the plugin.
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Registers routes for the plugin.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("api/debug", () => "Plugin is alive!");
    }

    /// <summary>
    /// Gets the priority of the route provider.
    /// </summary>
    public int Priority => -1;
}