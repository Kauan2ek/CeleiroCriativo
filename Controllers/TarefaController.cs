// importa as funcionalidades necessárias do asp.net core mvc
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// controller responsável pelas telas e ações relacionadas às tarefas
public class TarefaController : Controller
{
    // repositório que faz o acesso ao banco, o controller nunca escreve sql
    private TarefaSqlRepository repository;

    // o asp.net cria um controller novo a cada requisição, então esse construtor roda toda vez
    // criar o repositório já abre a conexão com o banco (é o construtor da DataConnection que faz isso),
    // inclusive nas actions que só devolvem tela e nem usam o banco
    public TarefaController()
    {
        repository = new TarefaSqlRepository();
    }

    // abre a tela de tarefas do gestor
    // segue a convenção: views/tarefa/gestortarefa.cshtml
    public ActionResult GestorTarefa()
    {
        return View();
    }

    // abre a tela de exibição de uma tarefa para o gestor
    // recebe o status e o nome da tarefa pela url
    public ActionResult ExTarefaGestor(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        // o caminho completo é usado porque a view não está na pasta padrão
        return View("~/Views/ExibicaoTarefa/ExTarefaGestor.cshtml");
    }

    // abre a tela de tarefas do funcionário
    public ActionResult FuncionarioTarefa()
    {
        // caminho completo, sem depender da convenção de pastas
        return View("~/Views/Tarefa/FuncionarioTarefa.cshtml");
    }

    // abre a tela de exibição de uma tarefa para o funcionário
    // recebe o status e o nome da tarefa pela url
    public ActionResult ExTarefaFuncionario(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        return View("~/Views/ExibicaoTarefa/ExTarefaFuncionario.cshtml");
    }

    // abre a tela de tarefas do cliente
    public ActionResult ClienteTarefa()
    {
        // retorna a view específica do cliente
        return View("~/Views/Tarefa/ClienteTarefa.cshtml");
    }

    // abre a tela de exibição de uma tarefa para o cliente
    // recebe o status e o nome da tarefa pela url
    public ActionResult ExTarefaCliente(string status, string tarefa)
    {
        // guarda o status da tarefa pra usar na view
        ViewBag.Status = status;

        // guarda o nome da tarefa pra usar na view
        ViewBag.Tarefa = tarefa;

        // retorna a view da pasta exibicaotarefa
        return View("~/Views/ExibicaoTarefa/ExTarefaCliente.cshtml");
    }


    // CRUD: por enquanto aplicado com API
    // as actions abaixo não devolvem tela, devolvem JSON (dá pra testar pelo Bruno ou pelo navegador)
    // todas seguem o mesmo roteiro: chamam o repositório, fecham a conexão com o Dispose() e montam a resposta
    // o Dispose() é chamado antes de cada return, porque o que vem depois do return não executa

    // cria uma tarefa
    // POST /Tarefa/Create
    // o parâmetro "tarefa" é montado sozinho pelo asp.net (model binding):
    // ele cria o objeto e preenche cada propriedade com o campo de mesmo nome que veio na requisição
    // ex: Titulo=Teste, Categoria.Codigo=1 (objeto de dentro), Funcionarios[0].Codigo=2 (item de lista)
    [HttpPost]
    public ActionResult Create(Tarefa tarefa)
    {
        try
        {
            bool resultado = repository.Create(tarefa);

            repository.Dispose();
            // new { ... } cria um objeto sem classe, só pra virar o JSON {"sucesso": true}
            return Json(new {sucesso = resultado});
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        // outros tipos de erro (ex: categoria vazia) não entram nesse catch
        catch (SqlException e)
        {

            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // busca uma tarefa pelo código
    // GET /Tarefa/Read/5 (o 5 da url cai no parâmetro id, por causa da rota padrão do Program.cs)
    public ActionResult Read(int id)
    {
        // Adicionar try catch depois
        // o resultado é guardado numa variável pra dar tempo de fechar a conexão antes do return
        Tarefa? tarefa = repository.Read(id);

        // o repositório devolve null quando não existe tarefa com esse código (ou ela está cancelada)
        if (tarefa == null)
        {
            repository.Dispose();
            // responde com o status http 404, sem JSON
            return NotFound();
        }
        repository.Dispose();
        // escrever só "tarefa" é o mesmo que "tarefa = tarefa": a propriedade do JSON pega o nome da variável
        return Json(new {sucesso = true, tarefa});
    }


    // lista as tarefas de um projeto
    // GET /Tarefa/ReadAll/2 (aqui o id da url é o código do PROJETO, não da tarefa)
    [HttpGet]
    public ActionResult ReadAll(int id)
    {
        // Adicionar try catch depois
        List<Tarefa> tarefas = repository.ReadAll(id);

        // lista vazia: o projeto não tem tarefa (ou o código do projeto não existe)
        if (tarefas.Count() == 0)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = "Nenhuma tarefa encontrada"});
        }
        repository.Dispose();
        // atenção: aqui as tarefas foram encontradas, então o sucesso deveria ser true
        return Json(new {sucesso = false, tarefas});
    }

    // altera uma tarefa que já existe
    // POST /Tarefa/Update
    // recebe os mesmos campos do Create, mais o Codigo da tarefa que vai ser alterada
    // sucesso volta false quando não existe tarefa com esse código
    [HttpPost]
    public ActionResult Update(Tarefa tarefa)
    {
        try
        {
            bool resultado = repository.Update(tarefa);

            repository.Dispose();
            return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // cancela uma tarefa (exclusão lógica: a linha continua no banco, só muda o status)
    // GET /Tarefa/Delete/5
    // está como GET pra facilitar o teste pelo navegador, mas apagar por GET não é boa prática:
    // na versão final o certo é [HttpPost]
    [HttpGet]
    public ActionResult Delete(int id)
    {
        try
        {
            bool resultado = repository.Delete(id);

            repository.Dispose();
            return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }
}