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
}