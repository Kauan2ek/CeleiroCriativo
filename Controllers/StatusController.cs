using Microsoft.AspNetCore.Mvc;

namespace APICeleiroCriativo.Controllers;

public class StatusController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
