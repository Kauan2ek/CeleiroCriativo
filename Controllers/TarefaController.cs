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

    // abre o kanban com as tarefas de um projeto, pro gestor
    // GET /Tarefa/IndexGestor/2 (aqui o id da url é o código do PROJETO, não da tarefa)
    // segue a convenção: views/tarefa/indexgestor.cshtml
    public ActionResult IndexGestor(int id)
    {
        List<Tarefa> tarefas = repository.ReadAll(id);
        repository.Dispose();

        // o ReadAll não preenche o IdProjeto de cada tarefa,
        // então o código do projeto vai pelo ViewBag pros links da tela (voltar e criar tarefa)
        ViewBag.IdProjeto = id;

        return View(tarefas);
    }

    // tela de detalhes pro gestor, já com os dados reais da tarefa
    // GET /Tarefa/DetalhesGestor/5 (o 5 da url cai no parâmetro id)
    // o erro vem pela url quando outro controller redireciona pra cá (ex: o catch do ComentarioController)
    // o "= null" deixa o erro opcional: os catch daqui chamam DetalhesGestor(id, idProjeto) sem ele
    public ActionResult DetalhesGestor(int id, int idProjeto, string? erro = null)
    {
        // só preenche quando veio erro pela url, pra não apagar o ViewBag.Erro que o catch do Create, Update ou Delete já colocou
        if (erro != null)
        {
            ViewBag.Erro = erro;
        }

        // tarefa nova: o objeto vazio só carrega o projeto que veio pela url (?idProjeto=2),
        // que a view coloca no hidden IdProjeto pro Create saber em qual projeto gravar
        Tarefa? tarefa = new Tarefa();
        tarefa.IdProjeto = idProjeto;

        // tarefa existente: o Read troca o objeto pelo do banco (com categoria, equipe, comentários e documentos)
        // e o IdProjeto passa a vir da coluna projeto_codigo
        if (id != 0)
        {
            tarefa = repository.Read(id);
            repository.Dispose();

            // tarefa inexistente ou cancelada: responde 404
            if (tarefa == null)
            {
                return NotFound();
            }

        }
        else
        {
            // tarefa nova: não buscou nada no banco, mas a conexão aberta no construtor também precisa ser fechada
            repository.Dispose();
        }

        // todos os funcionários ativos, pra lista do modal de equipe (quem já está na tarefa aparece marcado)
        FuncionarioSqlRepository funcionarioSqlRepository = new FuncionarioSqlRepository();
        ViewBag.Funcionarios = funcionarioSqlRepository.ReadAll();
        funcionarioSqlRepository.Dispose();

        // todas as categorias, pras opções do select de categoria
        CategoriaSqlRepository categoriaSqlRepository = new CategoriaSqlRepository();
        ViewBag.Categorias = categoriaSqlRepository.ReadAll();
        categoriaSqlRepository.Dispose();
        // entrega a tarefa pra view, que acessa ela pelo @Model (por isso o "@model Tarefa" no topo do cshtml)
        // o nome da view vai escrito porque essa action também é chamada pelo catch do Create, Update e Delete:
        // sem o nome, o asp.net procuraria uma view com o nome da action que chamou (ex: Update.cshtml)
        return View("DetalhesGestor", tarefa);
    }

    // CRUD da tarefa
    // Create, Update e Delete são chamados pelos formulários da tela DetalhesGestor:
    // quando dá certo, redirecionam pra outra tela; quando o banco recusa, voltam pra tela da tarefa com o ViewBag.Erro
    // Read e ReadAll continuam devolvendo JSON (dá pra testar pelo Bruno ou pelo navegador)
    // o "return Json" antigo ficou comentado embaixo de cada redirecionamento, caso precise testar como API de novo
    // todas seguem o mesmo roteiro: chamam o repositório, fecham a conexão com o Dispose() e montam a resposta
    // o Dispose() é chamado antes de cada return, porque o que vem depois do return não executa

    // cria uma tarefa
    // POST /Tarefa/Create
    // o parâmetro "tarefa" é montado sozinho pelo asp.net (model binding):
    // ele cria o objeto e preenche cada propriedade com o campo de mesmo nome que veio na requisição
    // ex: Titulo=Teste, Categoria.Codigo=1 (objeto de dentro), Funcionarios[2].Codigo=2 (item de lista)
    // o IdProjeto vem do hidden do formulário, que a DetalhesGestor preencheu com o ?idProjeto= da url
    [HttpPost]
    public ActionResult Create(Tarefa tarefa)
    {
        try
        {
            bool resultado = repository.Create(tarefa);

            repository.Dispose();
            // volta pra tela da própria tarefa recém-criada
            // o new {id = ...} cria um objeto sem classe que preenche o {id} da rota: vira /Tarefa/DetalhesGestor/15
            // atenção: o tarefa.Codigo só vem com o código novo se o repository.Create preencher ele depois do insert
            // (tarefa.Codigo = IdTarefa); sem isso ele continua 0 e abre um formulário de tarefa nova vazio
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = tarefa.Codigo});
            // return Json(new {sucesso = resultado});
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        // outros tipos de erro (ex: categoria vazia) não entram nesse catch
        catch (SqlException e)
        {
            // volta pra tela da tarefa mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a DetalhesGestor
            ViewBag.Erro = e.Message;
            return DetalhesGestor(tarefa.Codigo, tarefa.IdProjeto);
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // busca uma tarefa pelo código, com a categoria e a equipe
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


    // lista as tarefas de um projeto (sem a equipe de cada uma)
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
    // o resultado volta false quando não existe tarefa com esse código (mesmo assim redireciona pra tela da tarefa)
    [HttpPost]
    public ActionResult Update(Tarefa tarefa)
    {
        try
        {
            bool resultado = repository.Update(tarefa);

            repository.Dispose();
            // volta pra tela da própria tarefa, já com os dados salvos: /Tarefa/DetalhesGestor/5
            // o Codigo chega preenchido porque o formulário da tarefa sempre envia o hidden Codigo
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = tarefa.Codigo});
            // return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            // volta pra tela da tarefa mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a DetalhesGestor, depois de buscar a tarefa de novo
            ViewBag.Erro = e.Message;
            return DetalhesGestor(tarefa.Codigo, tarefa.IdProjeto);
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // cancela uma tarefa (exclusão lógica: a linha continua no banco, só muda o status)
    // POST /Tarefa/Delete, enviado pelo formulário do modal de exclusão da tela da tarefa
    // recebe o código da tarefa (id) e o do projeto (idProjeto), que serve pra voltar pro kanban do projeto
    // (a tarefa foi cancelada, então não tem mais tela dela pra voltar)
    [HttpPost]
    public ActionResult Delete(int id, int idProjeto)
    {
        try
        {
            bool resultado = repository.Delete(id);

            repository.Dispose();
            // volta pro kanban do projeto: /Tarefa/IndexGestor/2
            return RedirectToAction("IndexGestor", "Tarefa", new {id = idProjeto});
            // return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            // volta pra tela da tarefa mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a DetalhesGestor
            ViewBag.Erro = e.Message;
            return DetalhesGestor(id, idProjeto);
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }
}