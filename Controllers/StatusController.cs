using Microsoft.AspNetCore.Mvc;

public class StatusController : Controller
{
    private static List<Status> lista = new List<Status>();
     // http://localhost:1234/Status/index
    public ActionResult Index()
    {
        return View(lista);
    }
}