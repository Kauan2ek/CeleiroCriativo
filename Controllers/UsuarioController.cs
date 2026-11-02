using Microsoft.AspNetCore.Mvc;

public class UsuarioController : Controller
{
    private UsuarioSqlRepository repository; 

    public UsuarioController ()
    {
        repository = new UsuarioSqlRepository();
    }

    // Página de entrar
    // renderiza o formulário de login
    public ActionResult Entrar()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Entrar(Usuario model)
    {
        Usuario? usuario = repository.Entrar(model);

        if (usuario == null)
        {
            ViewBag.Erro = "Email ou senha incorreta!";
            return View();
        }

        HttpContext.Session.SetInt32("codigo", usuario.Codigo);
        HttpContext.Session.SetString("papel", usuario.Papel);
        HttpContext.Session.SetString("email", usuario.Email);
        HttpContext.Session.SetString("nome",  usuario.Nome);
        // HttpContext.Session.SetString("senha", usuario.Senha); -- Não guarda senha na sessão (segurança)
        return RedirectToAction("Index", "Projeto");
    }

    public ActionResult Sair()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}