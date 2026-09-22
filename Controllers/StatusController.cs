
using Microsoft.AspNetCore.Mvc;

public class StatusController : Controller
{
     // GET: /Status/Create
    [HttpGet]
    public ActionResult Create()
    {
        return View(); // retorna a view com um formulário vazio
    }

    // POST: /Status/Create
    [HttpPost]
    public ActionResult Create(Status status)
    {
        return View(status); // devolve o form preenchido, com os erros
    }
}