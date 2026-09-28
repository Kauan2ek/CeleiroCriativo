using Microsoft.AspNetCore.Mvc;

public class StatusController : Controller
{
    public static List<Status> Lista = new List<Status>
    {
        new Status(1, "Pendente"),
        new Status(2, "Em Andamento"),
        new Status(3, "Concluído")
    };
     // http://localhost:1234/Status/index
    public ActionResult Index()
    {
        return View(Lista);
    }


    // Status é apenas lido, não é criado, atualizado ou deletado
}