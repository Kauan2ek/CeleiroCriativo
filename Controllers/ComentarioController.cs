using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// controller dos comentários: não tem tela própria, o chat fica dentro da tela da tarefa (DetalhesGestor)
// a leitura dos comentários é feita pelo TarefaSqlRepository.Read, aqui só é gravado o comentário novo
public class ComentarioController : Controller
{
    // repositório que faz o acesso ao banco, o controller nunca escreve sql
    private ComentarioSqlRepository repository;

    // igual ao TarefaController: criar o repositório já abre a conexão com o banco
    public ComentarioController ()
    {
        repository = new ComentarioSqlRepository();
    }

    // cria um comentário numa tarefa
    // POST /Comentario/Create, enviado pelo formulário do chat da tela da tarefa
    // o parâmetro "comentario" é montado sozinho pelo asp.net (model binding):
    // ex: Descricao=Texto, IdTarefa=10, Funcionario.Codigo=2 (objeto de dentro)
    // o autor (Funcionario.Codigo) é trocado pelo código de quem está logado, lá embaixo
    // quem não está na equipe da tarefa tem o campo e o botão desabilitados na tela, então o formulário nem chega aqui
    [HttpPost]
    public ActionResult Create(Comentario comentario)
    {
        string? papel = HttpContext.Session.GetString("papel");

        // o cliente só lê os comentários, quem comenta é gestor e funcionário
        if (papel != "gestor" && papel != "funcionario")
        {
            repository.Dispose();
            return RedirectToAction("Detalhes", "Tarefa", new {id = comentario.IdTarefa});
        }

        // o autor do comentário é sempre quem está logado (gestor ou funcionário): o código vem da sessão, não do formulário,
        // pra ninguém comentar no nome de outra pessoa trocando o hidden da tela
        // se ele não estiver na equipe da tarefa, o banco recusa (FK_Comentarios_TarefaFuncionario) e cai no catch
        comentario.Funcionario.Codigo = (int)HttpContext.Session.GetInt32("codigo");

        try
        {
            bool resultado = repository.Create(comentario);

            repository.Dispose();
            // volta pra tela da tarefa, que busca os comentários de novo e já mostra o novo no chat
            // vai pela Detalhes, que escolhe a tela do papel de quem está logado
            return RedirectToAction("Detalhes", "Tarefa", new {id = comentario.IdTarefa});
            // return Json(new {sucesso = resultado});
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // ex: funcionário fora da equipe da tarefa (FK_Comentarios_TarefaFuncionario)
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        // outros tipos de erro não entram nesse catch
        catch (SqlException e)
        {
            // a tela da tarefa é de outro controller, então aqui precisa redirecionar,
            // e o ViewBag se perde no redirecionamento: por isso a mensagem vai pela url (?erro=...)
            // e a action DetalhesGestor do TarefaController coloca ela no ViewBag.Erro
            repository.Dispose();
            return RedirectToAction("Detalhes", "Tarefa", new {id = comentario.IdTarefa, erro = e.Message});
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }
}