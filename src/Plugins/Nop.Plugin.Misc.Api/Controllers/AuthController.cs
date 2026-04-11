#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Api.Filters;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.Api.Controllers;

/// <summary>
/// Authenticates and manages API tokens for admin users.
/// </summary>
[ApiController]
[Route("api/admin/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    #region Properties

    /// <summary>
    /// NopCommerce customer service
    /// </summary>
    protected readonly ICustomerService _customerService;
    /// <summary>
    /// NopCommerce customer registration service
    /// </summary>
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    /// <summary>
    /// NopCommerce static cache manager
    /// </summary>
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="customerService">The customer service</param>
    /// <param name="customerRegistrationService">The customer registration service</param>
    /// <param name="staticCacheManager">The static cache manager</param>
    public AuthController(ICustomerService customerService,
        ICustomerRegistrationService customerRegistrationService,
        IStaticCacheManager staticCacheManager)
    {
        _customerService = customerService;
        _customerRegistrationService = customerRegistrationService;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Extracts the raw API token from the HTTP context,
    /// stripping any "Bearer " prefix and surrounding quotes.
    /// Falls back to the X-Api-Token header if Authorization is absent.
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <returns>The raw token string, or <c>null</c> if none was found</returns>
    internal static string? ExtractToken(HttpContext httpContext)
    {
        const string bearerPrefix = "Bearer ";

        var token = httpContext.Request.Headers.Authorization.ToString().Trim();

        // Strip all "Bearer " prefixes (handles Swagger double-prefix edge case)
        while (token.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            token = token[bearerPrefix.Length..].Trim();

        // Remove surrounding quotes if any
        token = token.Trim('"');

        // Fallback to custom header
        if (string.IsNullOrWhiteSpace(token))
            token = httpContext.Request.Headers["X-Api-Token"].ToString().Trim().Trim('"');

        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Authenticates a user and returns an API token.
    /// </summary>
    /// <param name="model">Login credentials model</param>
    /// <returns>An object containing the token and its expiration information</returns>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
    {
        var result = await _customerRegistrationService.ValidateCustomerAsync(model.Email, model.Password);

        if (result != CustomerLoginResults.Successful)
            return Unauthorized("Invalid credentials.");

        var customer = await _customerService.GetCustomerByEmailAsync(model.Email);

        // Admin role verification
        if (!await _customerService.IsAdminAsync(customer))
            return Unauthorized("Insufficient permissions.");

        var token = Guid.NewGuid().ToString();
        // Create a specific cache key (Nop style)
        var cacheKey = new CacheKey($"Nop.Plugin.Api.Token-{token}");
        // Store the customer ID in the cache for 120 minutes
        await _staticCacheManager.SetAsync<int?>(cacheKey, customer.Id);
        Console.WriteLine($">>> API Token generated: {token} <<<");

        return Ok(new { Token = token, ExpiresInMinutes = 120 });
    }

    /// <summary>
    /// Logs out the current user by invalidating their API token.
    /// </summary>
    /// <returns>A message indicating the success of the logout operation</returns>
    [HttpPost("logout")]
    [AdminApiAuthorize]
    public async Task<IActionResult> LogoutAsync()
    {
        var token = ExtractToken(HttpContext);

        Console.WriteLine($">>> API Token invalidated: {token} <<<");

        // Token is absent or already invalid — treat as a successful logout
        if (string.IsNullOrWhiteSpace(token))
            return Ok(new { message = "Logout successful." });

        // Delete the token from cache
        var cacheKey = new CacheKey($"Nop.Plugin.Api.Token-{token}");
        await _staticCacheManager.RemoveAsync(cacheKey);

        return Ok(new { message = "Logout successful." });
    }

    #endregion
}