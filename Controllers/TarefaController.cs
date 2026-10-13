// importa as funcionalidades necessárias do asp.net core mvc
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// controller responsável pelas telas e ações relacionadas às tarefas
public class TarefaController : Controller
{
    private TarefaSqlRepository repository;
    public TarefaController()
    {
        repository = new TarefaSqlRepository();
    }

    // abre a tela de tarefas do gestor
    // segue a convenção: views/tarefa/gestortarefa.cshtml
    public ActionResult GestorTarefa()
    {
        return View();
    }

    // abre a tela de exibição de uma tarefa para o gestor
    // recebe o status e o nome da tarefa pela url
    public ActionResult ExTarefaGestor(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        // o caminho completo é usado porque a view não está na pasta padrão
        return View("~/Views/ExibicaoTarefa/ExTarefaGestor.cshtml");
    }

    // abre a tela de tarefas do funcionário
    public ActionResult FuncionarioTarefa()
    {
        // caminho completo, sem depender da convenção de pastas
        return View("~/Views/Tarefa/FuncionarioTarefa.cshtml");
    }

    // abre a tela de exibição de uma tarefa para o funcionário
    // recebe o status e o nome da tarefa pela url
    public ActionResult ExTarefaFuncionario(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        return View("~/Views/ExibicaoTarefa/ExTarefaFuncionario.cshtml");
    }

    // abre a tela de tarefas do cliente
    public ActionResult ClienteTarefa()
    {
        // retorna a view específica do cliente
        return View("~/Views/Tarefa/ClienteTarefa.cshtml");
    }

    // abre a tela de exibição de uma tarefa para o cliente
    // recebe o status e o nome da tarefa pela url
    public ActionResult ExTarefaCliente(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        return View("~/Views/ExibicaoTarefa/ExTarefaCliente.cshtml");
    }


    // CRUD: por enquanto aplicado com API
    [HttpPost]
    public ActionResult Create(Tarefa tarefa)
    {
        try
        {
            bool resultado = repository.Create(tarefa);

            repository.Dispose();
            return Json(new {sucesso = resultado});        
        }
        catch (SqlException e)
        {

            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }

    public ActionResult Read(int id)
    {
        // Adicionar try catch depois 
        Tarefa? tarefa = repository.Read(id);

        if (tarefa == null)
        {
            repository.Dispose();
            return NotFound();
        }
        repository.Dispose();
        return Json(new {sucesso = true, tarefa});
    }


    // /Tarefa/TesteListarPorProjeto/2
    [HttpGet]
    public ActionResult ReadAll(int id)
    {
        // Adicionar try catch depois 
        List<Tarefa> tarefas = repository.ReadAll(id);

        if (tarefas.Count() == 0)
        {   
            repository.Dispose();
            return Json(new {sucesso = false, erro = "Nenhuma tarefa encontrada"});
        }
        repository.Dispose();
        return Json(new {sucesso = false, tarefas});
    }

    [HttpPost]
    public ActionResult Update(Tarefa tarefa)
    {
        try
        {
            bool resultado = repository.Update(tarefa);

            repository.Dispose();
            return Json(new {sucesso = resultado});        
        }
        catch (SqlException e)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }

    [HttpGet]
    public ActionResult Delete(int id)
    {
        try
        {
            bool resultado = repository.Delete(id);

            repository.Dispose();
            return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }
}