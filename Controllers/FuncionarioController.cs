using Microsoft.AspNetCore.Mvc;

// controller responsável pela tela de cadastro de funcionário
public class CadastroFuncionarioController : Controller
{
    // método que apenas retorna a view de cadastro
    public ActionResult CadastroFuncionario()
    {
        return View();
    }

    // ação chamada no post do formulário, quando o usuário clica em salvar
    // recebe a TelaAnterior para saber para onde voltar depois
    [HttpPost]
    public ActionResult Salvar(string TelaAnterior)
    {
        // Aqui fica o código para salvar o funcionário

        // se a tela anterior foi informada, redireciona para ela
        // útil quando o usuário veio de outra tela e quer voltar pra lá
        if (!string.IsNullOrEmpty(TelaAnterior))
        {
            return Redirect(TelaAnterior);
        }

        // caso contrário, redireciona para o gestor de projeto
        // como fallback, pra não deixar o usuário sem destino
        return RedirectToAction("GestorProjeto", "Projeto");
    }
}