using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class ComentarioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
