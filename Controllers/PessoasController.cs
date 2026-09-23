using Microsoft.AspNetCore.Mvc;

public class PessoasController : Controller
{
    private static List<Pessoa> lista = new List<Pessoa>();
     // http://localhost:1234/Pessoas/index
    public ActionResult Index()
    {
        return View(lista);
    }
}