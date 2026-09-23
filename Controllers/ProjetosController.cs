using Microsoft.AspNetCore.Mvc;

public class ProjetosController : Controller
{
    private static List<Projeto> lista = new List<Projeto>();
     // http://localhost:1234/Projetos/index
    public ActionResult Index()
    {
        return View(lista);
    }

    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }
    
    [HttpPost]
    public ActionResult Create(Projeto model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }
}