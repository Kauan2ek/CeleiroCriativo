using Microsoft.AspNetCore.Mvc;

// controller responsável pelas telas principais do site
// cada action só devolve uma view, sem lógica de negócio
public class HomeController : Controller
{
    // Página inicial
    // primeira tela que o usuário vê ao acessar o site
    public IActionResult Index()
    {
        return View();
    }

    // Página de entrar
    // renderiza o formulário de login
    public IActionResult Entrar()
    {
        return View();
    }

    // Página de cadastro
    // renderiza o formulário de cadastro de usuário
    public IActionResult Cadastro()
    {
        return View();
    }

}