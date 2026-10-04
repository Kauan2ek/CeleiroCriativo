using Microsoft.AspNetCore.Mvc;

// controller responsável pelas telas de exibição de projeto
// cada action mostra o projeto pra um tipo de usuário diferente
public class ExibicaoProjetoController : Controller
{
    // exibe o projeto na visão do gestor
    public IActionResult ExProjetoGestor()
    {
        return View();
    }

    // exibe o projeto na visão do funcionário
    public IActionResult ExProjetoFuncionario()
    {
        return View();
    }

    // exibe o projeto na visão do cliente
    public IActionResult ExProjetoCliente()
    {
        return View();
    }
}