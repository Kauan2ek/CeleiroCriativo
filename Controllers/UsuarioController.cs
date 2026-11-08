using System.Net.Http.Headers;
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

    [HttpGet]
    public ActionResult Recuperar()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Entrar(Usuario model)
    {
        Usuario? usuario = repository.Entrar(model);
        repository.Dispose();

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

    [HttpPost]
    public ActionResult Recuperar(string email)
    {
        if (!repository.EmailExiste(email))
        {
            ViewBag.Erro = "Este e-mail não está cadastrado.";
            repository.Dispose();
            return View("Recuperar");
        }
        repository.GerarCodigo(email);
        repository.Dispose();

        return View("Recuperar", email);
    }

    [HttpPost]
    public ActionResult RedefinirSenha(string codigoVerificacao, string email, string senha)
    {
        if (repository.ValidarCodigo(codigoVerificacao, email))
        {
            repository.RedefinirSenha(email, senha);
        } else
        {
            ViewBag.Erro = "Código de verificação incorreto!";
            repository.Dispose();
            return View("Recuperar", email);
        }
        repository.Dispose();
        ViewBag.Sucesso = "Senha alterada! Entre com sua nova senha.";

        return View("Entrar");
    }
}