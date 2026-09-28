
using Microsoft.AspNetCore.Mvc;

public class ComentariosController : Controller
{
    private static List<Comentario> lista = new List<Comentario>();
     // http://localhost:1234/comentarios/index
    public ActionResult Index()
    {
        return View(lista);
    }

     // GET: /Comentarios/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Comentarios/Create
    [HttpPost]
    public ActionResult Create(Comentario model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }

    // GET: /comentarios/update
    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach(var comentario in lista)
        {
            if (comentario.Codigo == id)
            {
                return View(comentario);
            }
        }
        return NotFound();
    }

    // POST: /comentarios/update
    [HttpPost]
    public ActionResult Update(int id, Comentario model)
    {
        foreach (var comentario in lista)
        {
            if (comentario.Codigo == id)
            {
                comentario.Titulo = model.Titulo;
                comentario.Descricao = model.Descricao;
                comentario.DataHora = model.DataHora;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }

    [HttpPost]
    public ActionResult Delete(int id, Comentario model)
    {
        foreach (var comentario in lista)
        {
            if (comentario.Codigo == id)
            {
                lista.Remove(comentario);
                break;
            }
        }
        return RedirectToAction("Index");
    }
}