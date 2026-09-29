using Microsoft.AspNetCore.Mvc;

public class CargosController : Controller
{
    private static List<Cargo> lista = new List<Cargo>();
    // GET: /Cargos/index
    [HttpGet]
    public ActionResult Index()
    {
        return View(lista);
    }

    [HttpGet]
    // GET: /cargos/create   
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /cargos/create   
    [HttpPost]
    public ActionResult Create(Cargo model)
    {
        if (lista.Count == 0)
            model.Codigo = 1; 
        else
            model.Codigo = lista[^1].Codigo + 1; // Pega o último ID cadastrado e incrementa 1
        lista.Add(model);
        return RedirectToAction("Index");
    }

    // GET: /cargos/update
    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var cargo in lista)
        {
            if (cargo.Codigo == id)
                return View(cargo);
        }
        return NotFound();
    }

    // POST: /cargos/update
    [HttpPost]
    public ActionResult Update(int id, Cargo model)
    {
        foreach (var cargo in lista)
        {
            if (cargo.Codigo == id)
            {
                cargo.Descricao = model.Descricao;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }

    // POST: /cargos/delete
    [HttpPost]
    public ActionResult Delete(int id)
    {
        foreach (var cargo in lista)
        {
            if (cargo.Codigo == id)
            {
                lista.Remove(cargo);
                break;
            }
        }
        return RedirectToAction("Index");
    }
}   