using Microsoft.AspNetCore.Mvc;

public class ClientesController : Controller
{
    private static List<Cliente> lista = new List<Cliente>();
     // http://localhost:1234/Clientes/index
    public ActionResult Index()
    {
        return View(lista);
    }

     // GET: /Clientes/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Clientes/Create
    [HttpPost]
    public ActionResult Create(Cliente model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }
}