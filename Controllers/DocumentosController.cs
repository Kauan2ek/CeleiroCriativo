using System.Reflection.Metadata;
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

    // GET: /documentos/update
    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var documento in lista)
        {
            if (documento.Codigo == id)
            {
                return View(documento);
            }
        }
        return NotFound();
    }

    // POST: /documentos/update
    public ActionResult Update(int id, Documento model)
    {
        foreach (var documento in lista)
        {
            if (documento.Codigo == id)
            {
                documento.Descricao = model.Descricao;
                documento.Extensao = model.Descricao;
                documento.Diretorio = model.Diretorio;
            }
        }
        return RedirectToAction("Index");
    }

    // DELETE: /documentos/delete
    public ActionResult Delete(int id)
    {
        foreach (var documento in lista)
        {
            if (documento.Codigo == id)
            {
                lista.Remove(documento);
                break;
            }
        }
        return View("Index");
    }
}