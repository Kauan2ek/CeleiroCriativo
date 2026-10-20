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
    // o Funcionario.Codigo é o primeiro funcionário da equipe da tarefa (autor provisório, enquanto não existe login)
    // se a tarefa não tem equipe, a tela desabilita o campo e o botão, então o formulário nem chega aqui
    [HttpPost]
    public ActionResult Create(Comentario comentario)
    {
        try
        {
            bool resultado = repository.Create(comentario);

            repository.Dispose();
            // volta pra tela da tarefa, que busca os comentários de novo e já mostra o novo no chat
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = comentario.IdTarefa});
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
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = comentario.IdTarefa, erro = e.Message});
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }
}