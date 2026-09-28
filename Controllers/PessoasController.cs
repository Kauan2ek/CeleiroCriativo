using Microsoft.AspNetCore.Mvc;

public class PessoasController : Controller
{
    private static List<Pessoa> lista = new List<Pessoa>();
     // http://localhost:1234/Pessoas/index
    public ActionResult Index()
    {
        return View(lista);
    }
    
     // GET: /pessoas/create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /pessoas/create
    [HttpPost]
    public ActionResult Create(Pessoa model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }

    // GET: /pessoas/update
    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var pessoa in lista)
        {
            if (pessoa.Codigo == id)
                return View(pessoa);
        }
        return NotFound();
    }

    // POST: /pessoas/update
    [HttpPost]
    public ActionResult Update(int id, Pessoa model)
    {
        foreach (var pessoa in lista)
        {
            if (pessoa.Codigo == id)
            {
                pessoa.Nome = model.Nome;
                pessoa.Telefone = model.Telefone;
                pessoa.TipoDocumento = model.TipoDocumento;
                pessoa.Documento = model.Documento;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }

    // GET: /pessoas/delete
    public ActionResult Delete(int id)
    {
        foreach (var pessoa in lista)
        {
            if (pessoa.Codigo == id)
            {
                lista.Remove(pessoa);
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }
}