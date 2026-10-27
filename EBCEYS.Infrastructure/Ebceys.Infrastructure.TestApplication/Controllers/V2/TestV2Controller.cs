using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.TestApplication.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TestV2Controller : Controller
{
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }
}