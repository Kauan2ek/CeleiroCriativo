// importa as funcionalidades necessárias do asp.net core mvc
using Microsoft.AspNetCore.Mvc;

// controller responsável pelas telas e ações relacionadas às tarefas
public class TarefaController : Controller
{
    // abre a tela de tarefas do gestor
    // segue a convenção: views/tarefa/gestortarefa.cshtml
    public IActionResult GestorTarefa()
    {
        return View();
    }

    // abre a tela de exibição de uma tarefa para o gestor
    // recebe o status e o nome da tarefa pela url
    public IActionResult ExTarefaGestor(string status, string tarefa)
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
    public IActionResult FuncionarioTarefa()
    {
        // caminho completo, sem depender da convenção de pastas
        return View("~/Views/Tarefa/FuncionarioTarefa.cshtml");
    }

    // abre a tela de exibição de uma tarefa para o funcionário
    // recebe o status e o nome da tarefa pela url
    public IActionResult ExTarefaFuncionario(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        return View("~/Views/ExibicaoTarefa/ExTarefaFuncionario.cshtml");
    }

    // abre a tela de tarefas do cliente
    public IActionResult ClienteTarefa()
    {
        // retorna a view específica do cliente
        return View("~/Views/Tarefa/ClienteTarefa.cshtml");
    }

    // abre a tela de exibição de uma tarefa para o cliente
    // recebe o status e o nome da tarefa pela url
    public IActionResult ExTarefaCliente(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        return View("~/Views/ExibicaoTarefa/ExTarefaCliente.cshtml");
    }
}