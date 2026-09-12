using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class ProjetoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
