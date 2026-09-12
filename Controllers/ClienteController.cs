using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class ClienteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
