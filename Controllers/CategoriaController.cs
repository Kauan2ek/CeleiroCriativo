
using Microsoft.AspNetCore.Mvc;

public class CategoriaController : Controller
{
    private static List<Categoria> lista = new List<Categoria>();
     // http://localhost:1234/tarefa/index
    public ActionResult Index()
    {
        //lista.Add(new Categoria {Descricao= "Teste 21", Cor="Sim"});
        //lista.Add(new Categoria {Descricao= "Teste 4", Cor="Azul"});

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