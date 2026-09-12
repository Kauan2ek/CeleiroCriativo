using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class FuncionarioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
