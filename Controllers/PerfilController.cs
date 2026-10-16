using Microsoft.AspNetCore.Mvc;

// Controller responsável pelas telas de perfil.
// As Views existentes se chamam Gestor.cshtml, Funcionario.cshtml e Cliente.cshtml,
// então os nomes são informados explicitamente para evitar procurar arquivos inexistentes.
public class PerfilController : Controller
{
    public ActionResult GestorPerfil()
    {
        return View("Gestor");
    }

    public ActionResult FuncionarioPerfil()
    {
        return View("Funcionario");
    }

    public ActionResult ClientePerfil()
    {
        return View("Cliente");
    }
}
