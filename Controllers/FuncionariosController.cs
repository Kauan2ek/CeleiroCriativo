using Microsoft.AspNetCore.Mvc;

public class FuncionariosController : Controller
{
    private static List<Funcionario> lista = new List<Funcionario>();
     // http://localhost:1234/Funcionarios/index
    public ActionResult Index()
    {
        return View(lista);
    }

     // GET: /Funcionarios/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Funcionarios/Create
    [HttpPost]
    public ActionResult Create(Funcionario model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }
}