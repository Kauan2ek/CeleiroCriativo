using Microsoft.AspNetCore.Mvc;

// controller responsável pelas telas principais do site
// cada action só devolve uma view, sem lógica de negócio
public class HomeController : Controller
{
    // Página inicial
    // primeira tela que o usuário vê ao acessar o site
    public ActionResult Index()
    {
        return View();
    }

    // O cadastro fica dentro da própria landing page.
    // Enquanto o envio ao banco não faz parte desta etapa, esta rota apenas
    // leva o usuário até a seção correta em vez de procurar uma View inexistente.
    public ActionResult Cadastro()
    {
        return Redirect("/Home/Index#cadastro");
    }

}