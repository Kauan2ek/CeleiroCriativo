using Microsoft.AspNetCore.Mvc;

// controller responsável pelas telas relacionadas a projeto
// aqui tem tanto views próprias quanto views de outras pastas sendo reaproveitadas
public class ProjetoController : Controller
{
    // exibe a tela de projeto na visão do gestor
    // usa o caminho padrão, ou seja, procura em Views/Projeto/GestorProjeto.cshtml
    public ActionResult GestorProjeto()
    {
        return View();
    }

    // reaproveita a view que já existe no controller ExibicaoProjeto
    // em vez de duplicar a tela, só aponta pro caminho dela
    // por isso o "~/Views/ExibicaoProjeto/ExProjetoGestor.cshtml"
    public ActionResult ExProjetoGestor()
    {
        return View("~/Views/ExibicaoProjeto/ExProjetoGestor.cshtml");
    }

    // aqui muda a rota padrão do MVC
    // por padrão seria /Projeto/FuncionarioProjeto mesmo, mas foi definida explicitamente
    // [HttpGet] garante que só responde a requisições GET
    // e a view é passada com caminho completo, ignorando a convenção
    [HttpGet("/Projeto/FuncionarioProjeto")]
    public ActionResult FuncionarioProjeto()
    {
        return View("~/Views/Projeto/FuncionarioProjeto.cshtml");
    }

    // mesma ideia das outras, só aponta o caminho completo da view
    // serve pra deixar explícito onde está o arquivo, sem depender da convenção de pastas
    public ActionResult ClienteProjeto()
    {
        return View("~/Views/Projeto/ClienteProjeto.cshtml");
    }

    /*Para implementar depois*/
    // exibe o projeto na visão do gestor
    /*
    public ActionResult ExProjetoGestor()
    {
        return View();
    }

    // exibe o projeto na visão do funcionário
    public ActionResult ExProjetoFuncionario()
    {
        return View();
    }

    // exibe o projeto na visão do cliente
    public ActionResult ExProjetoCliente()
    {
        return View();
    }
    */
}