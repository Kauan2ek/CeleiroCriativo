
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

    // GET: /clientes/update
    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var cliente in lista)
        {
            if (cliente.Codigo == id)
                return View(cliente);
        }
        return NotFound();
    }

    // POST: /clientes/update
    [HttpPost]
    public ActionResult Update(int id, Cliente model)
    {
        foreach (var cliente in lista)
        {
            cliente.Nome = model.Nome;
            cliente.Telefone = model.Telefone;
            cliente.TipoDocumento = model.TipoDocumento;
            cliente.Documento = model.Documento;
            cliente.Email = model.Email;
            cliente.Senha = model.Senha;
        }
        return RedirectToAction("Index");
    }

    // GET: /clientes/delete
    [HttpPost]
    public ActionResult Delete(int id)
    {
        foreach (var cliente in lista)
        {
            lista.Remove(cliente);
            break;
        }
        return RedirectToAction("Index");
    }
}