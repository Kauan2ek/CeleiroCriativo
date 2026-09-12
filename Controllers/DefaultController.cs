using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("")]
public class DefaultController : ControllerBase
{
    [HttpGet]
    public IActionResult Index() => Ok(new { status = "ok", api = "APICeleiroCriativo" });
}
