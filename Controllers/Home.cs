using Microsoft.AspNetCore.Mvc;

public class Home : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}