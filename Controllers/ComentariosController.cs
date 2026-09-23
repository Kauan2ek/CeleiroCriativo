
using Microsoft.AspNetCore.Mvc;

public class ComentariosController : Controller
{
    private static List<Comentario> lista = new List<Comentario>();
     // http://localhost:1234/comentarios/index
    public ActionResult Index()
    {
        return View(lista);
    }

     // GET: /Comentarios/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Comentarios/Create
    [HttpPost]
    public ActionResult Create(Comentario model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }
}