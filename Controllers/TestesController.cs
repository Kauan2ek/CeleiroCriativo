using Microsoft.AspNetCore.Mvc;

public class TestesController : Controller
{
    // http://localhost:1234/Testes/index
    public ActionResult Index()
    {
        return View();
    }
}
