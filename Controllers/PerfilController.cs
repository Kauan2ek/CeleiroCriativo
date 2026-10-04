using Microsoft.AspNetCore.Mvc;

// controller responsável pelas telas de perfil
// cada action mostra o perfil de um tipo de usuário diferente
public class PerfilController : Controller
{
    // exibe o perfil do gestor
    public IActionResult GestorPerfil()
    {
        return View();
    }

    // exibe o perfil do funcionário
    public IActionResult FuncionarioPerfil()
    {
        return View();
    }

    // exibe o perfil do cliente
    public IActionResult ClientePerfil()
    {
        return View();
    }
}