using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Api.Filters;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AuthController : BasePluginController
{

    protected readonly ICustomerService _customerService;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected IStaticCacheManager _staticCacheManager;

    public AuthController(ICustomerService customerService,
        ICustomerRegistrationService customerRegistrationService,
        IStaticCacheManager staticCacheManager)
    {
        _customerService = customerService;
        _customerRegistrationService = customerRegistrationService;
        _staticCacheManager = staticCacheManager;
    }

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

        return Ok(new { Token = token, ExpiresInMinutes = 120 });
    }


    [HttpPost("logout")]
    [AdminApiAuthorize]
    public async Task<IActionResult> LogoutAsync()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token)) return Ok(new { message = "Logout successful." });

        // Delete the token from cache
        var cacheKey = new CacheKey($"Nop.Plugin.Api.Token-{token}");
        await _staticCacheManager.RemoveAsync(cacheKey);

        return Ok(new { message = "Logout successful." });
    }
}
