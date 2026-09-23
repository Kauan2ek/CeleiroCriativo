using Microsoft.AspNetCore.Mvc;

public class DocumentosController : Controller
{
    private static List<Documento> lista = new List<Documento>();
     // http://localhost:1234/Documentos/index
    public ActionResult Index()
    {
        return View(lista);
    }

     // GET: /Documentos/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Documentos/Create
    [HttpPost]
    public ActionResult Create(Documento model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }
}