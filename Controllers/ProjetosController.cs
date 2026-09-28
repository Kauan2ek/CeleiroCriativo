using Microsoft.AspNetCore.Mvc;

public class ProjetosController : Controller
{
    private static List<Projeto> lista = new List<Projeto>();
     // http://localhost:1234/Projetos/index
    public ActionResult Index()
    {
        return View(lista);
    }

    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }
    
    [HttpPost]
    public ActionResult Create(Projeto model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var projeto in lista)
        {
            if (projeto.Codigo == id)
                return View(projeto);
        }
        return NotFound();
    }

    [HttpPost]
    public ActionResult Update(int id, Projeto model)
    {
        foreach (var projeto in lista)
        {
            if (projeto.Codigo == id)
            {
                projeto.Titulo = model.Titulo;
                projeto.Descricao = model.Descricao;
                projeto.DataInicio = model.DataInicio;
                projeto.DataFim = model.DataFim;
                projeto.DataPrevista = model.DataPrevista;
                projeto.Status = model.Status;
                projeto.tarefas = model.tarefas;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }
}