using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class CargoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
