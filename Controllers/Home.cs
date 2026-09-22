using Microsoft.AspNetCore.Mvc;

public class Home : Controller
{
    public ActionResult Index()
    {
        return View();
    }

    public ActionResult Entrar()
    {
        return View();
        //return RedirectToAction();
    }
}