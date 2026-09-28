using System.Net.Http.Headers;
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

    // GET: /funcionarios/update
    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var funcionario in lista)
        {
            if (funcionario.Codigo == id)
                return View(funcionario);
        }
        return NotFound();
    }

    // POST: /funcionarios/update
    [HttpPost]
    public ActionResult Update(int id, Funcionario model)
    {
        foreach (var funcionario in lista)
        {
            if (funcionario.Codigo == id)
            {
                funcionario.Nome = model.Nome;
                funcionario.Telefone = model.Telefone;
                funcionario.TipoDocumento = model.TipoDocumento;
                funcionario.Documento = model.Documento;
                funcionario.Email = model.Email;
                funcionario.Senha = model.Senha;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }

    // POST: /funcionarios/delete
    public ActionResult Delete(int id)
    {
        foreach (var funcionario in lista)
        {
            if (funcionario.Codigo == id)
            {
                lista.Remove(funcionario);
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }
}