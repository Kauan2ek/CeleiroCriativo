
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
        if (lista.Count == 0)
            model.Codigo = 1; 
        else
            model.Codigo = lista[^1].Codigo + 1; // Pega o último ID cadastrado e incrementa 1
        lista.Add(model);
        return RedirectToAction("Index");
    }

    // GET: /Categorias/Update
    [HttpGet]
    public ActionResult Update(int id) 
    {
        foreach (var categoria in lista)
        {
            if (categoria.Codigo == id)
                return View(categoria);
        }
        return NotFound();
    }

    [HttpPost]
    public ActionResult Update(int id, Categoria model)
    {
        foreach (var categoria in lista)
        {
            if (categoria.Codigo == id)
            {
                categoria.Descricao = model.Descricao;
                categoria.Cor = model.Cor;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }

    [HttpPost]
    public ActionResult Delete(int id)
    {
        foreach (var categoria in lista)
        {
            if(categoria.Codigo == id)
            {
                lista.Remove(categoria);
                break;
            }
        }

        return RedirectToAction("Index");
    }
}