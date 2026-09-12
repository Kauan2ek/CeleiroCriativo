using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class ProjetoTarefaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
