
using Microsoft.AspNetCore.Mvc;

public class CategoriasController : Controller
{
    private static List<Categoria> lista = new List<Categoria>();
     // http://localhost:1234/Categorias/index
    public ActionResult Index()
    {
        return View(lista);
    }

     // GET: /Categorias/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Categorias/Create
    [HttpPost]
    public ActionResult Create(Categoria model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }
}