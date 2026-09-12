using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class TarefaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
