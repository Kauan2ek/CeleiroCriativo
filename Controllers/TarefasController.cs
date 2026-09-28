using Microsoft.AspNetCore.Mvc;

public class TarefasController : Controller
{
    private static List<Tarefa> lista = new List<Tarefa>();
     // http://localhost:1234/Tarefas/index
    public ActionResult Index()
    {
        return View(lista);
    }

    [HttpGet]
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Create(Tarefa model)
    {
        lista.Add(model);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Update(int id)
    {
        foreach (var tarefa in lista)
        {
            if (tarefa.Codigo == id)
                return View(tarefa);
        }
        return NotFound();
    }

    [HttpPost]
    public ActionResult Update(int id, Tarefa model)
    {
        foreach (var tarefa in lista)
        {
            if (tarefa.Codigo == id)
            {
                tarefa.Titulo = model.Titulo;
                tarefa.Descricao = model.Descricao;
                tarefa.DataHoraInicio = model.DataHoraInicio;
                tarefa.DataHoraFim = model.DataHoraFim;
                tarefa.DataHoraPrevista = model.DataHoraPrevista;
                tarefa.Visibilidade = model.Visibilidade;
                tarefa.Funcionarios = model.Funcionarios;
                tarefa.Status = model.Status;
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }
}