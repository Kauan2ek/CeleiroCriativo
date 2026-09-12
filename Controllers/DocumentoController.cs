using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class DocumentoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
