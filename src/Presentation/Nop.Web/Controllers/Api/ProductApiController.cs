namespace Nop.Web.Controllers.Api;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductApiController : ControllerBase
{
    [HttpGet("test")]
    public async Task<ActionResult> Test()
    {
        return Ok("Hello from API ladies gentlemen !");
    }
}