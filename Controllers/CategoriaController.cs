using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class CategoriaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
