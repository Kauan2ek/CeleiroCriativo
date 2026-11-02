// importa as funcionalidades necessárias do asp.net core mvc
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// controller responsável pelas telas e ações relacionadas aos projetos
public class ProjetoController : Controller
{
    // repositório que faz o acesso ao banco, o controller nunca escreve sql
    private ProjetoSqlRepository repository;

    // o asp.net cria um controller novo a cada requisição, então esse construtor roda toda vez
    // criar o repositório já abre a conexão com o banco (é o construtor da DataConnection que faz isso),
    // inclusive nas actions que só devolvem tela e nem usam o banco
    public ProjetoController()
    {
        repository = new ProjetoSqlRepository();
    }

    // rota padrão, definida no middleware (Program.cs)
    public ActionResult Index()
    {
        string? papel = HttpContext.Session.GetString("papel");

        if (papel == "gestor")      { return IndexGestor();        }
        if (papel == "funcionario") { return FuncionarioProjeto(); }
        if (papel == "cliente")     { return ClienteProjeto();     }

        return RedirectToAction("Entrar", "Usuario");
    }

    // lista todos os projetos que não foram excluídos, pro gestor
    // GET /Projeto/IndexGestor
    // segue a convenção: views/projeto/indexgestor.cshtml
    public ActionResult IndexGestor()
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}

        List<Projeto> projetos = repository.ReadAll();
        repository.Dispose();

        // o nome da view vai escrito porque essa action também é chamada pela Index:
        // sem o nome, o asp.net procuraria uma view com o nome da action que chamou (Index.cshtml)
        return View("IndexGestor", projetos);
    }



    // tela de detalhes pro gestor, já com os dados reais do projeto
    // GET /Projeto/DetalhesGestor/5 (o 5 da url cai no parâmetro id)
    // GET /Projeto/DetalhesGestor (sem id, o id fica 0 e abre o formulário de projeto novo)
    public ActionResult DetalhesGestor(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}
        // projeto novo: o objeto vazio só carrega o status inicial
        // um projeto criado pelo gestor começa como "Em andamento" (o código é buscado pela descrição no banco)
        Projeto? projeto = new Projeto();

        // projeto existente: o Read troca o objeto pelo do banco (com cliente, status e tarefas)
        if (id != 0)
        {
            projeto = repository.Read(id);
            repository.Dispose();

            // projeto inexistente ou excluído: responde 404
            if (projeto == null)
            {
                return NotFound();
            }

        }
        else
        {
            // projeto novo: não buscou nada no banco, mas a conexão aberta no construtor também precisa ser fechada
            repository.Dispose();

            StatusSqlRepository statusInicialSqlRepository = new StatusSqlRepository();
            Status? statusInicial = statusInicialSqlRepository.ReadByDescricao("Em andamento");
            statusInicialSqlRepository.Dispose();

            if (statusInicial != null)
            {
                projeto.Status = statusInicial;
            }
        }

        // todos os clientes, pras opções do select de cliente
        ClienteSqlRepository clienteSqlRepository = new ClienteSqlRepository();
        ViewBag.Clientes = clienteSqlRepository.ReadAll();
        clienteSqlRepository.Dispose();

        // todos os status, pras opções do select de situação
        StatusSqlRepository statusSqlRepository = new StatusSqlRepository();
        ViewBag.StatusList = statusSqlRepository.ReadAll();
        statusSqlRepository.Dispose();
        // entrega o projeto pra view, que acessa ele pelo @Model (por isso o "@model Projeto" no topo do cshtml)
        // o nome da view vai escrito porque essa action também é chamada pelo catch do Create, Update e Delete:
        // sem o nome, o asp.net procuraria uma view com o nome da action que chamou (ex: Update.cshtml)
        return View("DetalhesGestor", projeto);
    }

    // CRUD do projeto
    // Create, Update e Delete são chamados pelos formulários da tela DetalhesGestor:
    // quando dá certo, devolvem a tela com o ViewBag.Sucesso; quando o banco recusa, voltam pra tela do projeto com o ViewBag.Erro
    // não usam RedirectToAction porque o ViewBag se perde no redirecionamento:
    // chamam a DetalhesGestor ou a IndexGestor direto, e quem fecha a conexão (Dispose) é a action chamada

    // cria um projeto
    // POST /Projeto/Create
    // o parâmetro "projeto" é montado sozinho pelo asp.net (model binding):
    // ele cria o objeto e preenche cada propriedade com o campo de mesmo nome que veio na requisição
    // ex: Titulo=Teste, Cliente.Codigo=3 e Status.Codigo=2 (objetos de dentro, vindos dos selects)
    [HttpPost]
    public ActionResult Create(Projeto projeto)
    {
        try
        {
            int codigoProjeto = repository.Create(projeto);

            // abre a tela do próprio projeto recém-criado mostrando a mensagem de sucesso
            // quem fecha a conexão é a DetalhesGestor, depois de buscar o projeto novo
            ViewBag.Sucesso = "Projeto criado com sucesso.";
            return DetalhesGestor(codigoProjeto);
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        catch (SqlException e)
        {
            // volta pra tela do projeto mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a DetalhesGestor
            ViewBag.Erro = e.Message;
            return DetalhesGestor(projeto.Codigo);
        }
    }

    // altera um projeto que já existe
    // POST /Projeto/Update
    // recebe os mesmos campos do Create, mais o Codigo do projeto que vai ser alterado
    // o resultado volta false quando não existe projeto com esse código (mesmo assim redireciona pra tela do projeto)
    [HttpPost]
    public ActionResult Update(Projeto projeto)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}

        try
        {
            bool resultado = repository.Update(projeto);

            if (resultado)
            {
                ViewBag.Sucesso = "Projeto atualizado com sucesso.";
            }
            // volta pra tela do próprio projeto, já com os dados salvos
            // o Codigo chega preenchido porque o formulário do projeto sempre envia o hidden Codigo
            // quem fecha a conexão é a DetalhesGestor, depois de buscar o projeto de novo
            return DetalhesGestor(projeto.Codigo);
        }
        catch (SqlException e)
        {
            // volta pra tela do projeto mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a DetalhesGestor, depois de buscar o projeto de novo
            ViewBag.Erro = e.Message;
            return DetalhesGestor(projeto.Codigo);
        }
    }

    // exclui um projeto (exclusão lógica: a linha continua no banco, só muda o status pra "Excluído")
    // POST /Projeto/Delete, enviado pelo formulário de exclusão da tela do projeto
    // (o projeto foi excluído, então não tem mais tela dele pra voltar: volta pra lista)
    [HttpPost]
    public ActionResult Delete(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}
        try
        {
            bool resultado = repository.Delete(id);

            if (resultado)
            {
                ViewBag.Sucesso = "Projeto excluído com sucesso.";
            }
            // volta pra lista de projetos, e quem fecha a conexão é a IndexGestor
            return IndexGestor();
        }
        catch (SqlException e)
        {
            // volta pra tela do projeto mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a DetalhesGestor
            ViewBag.Erro = e.Message;
            return DetalhesGestor(id);
        }
    }

    // aprova uma solicitação enviada pela landing page (o status muda de "Em análise" pra "Em andamento")
    // POST /Projeto/Aprovar, enviado pelo botão Aprovar da tela do projeto
    [HttpPost]
    public ActionResult Aprovar(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}

        try
        {
            bool resultado = repository.Aprovar(id);

            if (resultado)
            {
                ViewBag.Sucesso = "Projeto aprovado e movido para Em andamento.";
            }
            // volta pra tela do próprio projeto, que agora já libera o cadastro de tarefas
            // quem fecha a conexão é a DetalhesGestor
            return DetalhesGestor(id);
        }
        catch (SqlException e)
        {
            ViewBag.Erro = e.Message;
            return DetalhesGestor(id);
        }
    }

    // nega uma solicitação: diferente do Delete, o projeto ainda "Em análise" é apagado de verdade do banco
    // POST /Projeto/Negar, enviado pelo botão Negar da tela do projeto
    [HttpPost]
    public ActionResult Negar(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}

        try
        {
            bool resultado = repository.Negar(id);

            if (resultado)
            {
                ViewBag.Sucesso = "Projeto negado e removido.";
            }
            // o projeto foi apagado, então volta pra lista, e quem fecha a conexão é a IndexGestor
            return IndexGestor();
        }
        catch (SqlException e)
        {
            ViewBag.Erro = e.Message;
            return DetalhesGestor(id);
        }
    }

    // lista os projetos em que o funcionário logado tem alguma tarefa
    // GET /Projeto/FuncionarioProjeto
    public ActionResult FuncionarioProjeto()
    {
        if (HttpContext.Session.GetString("papel") != "funcionario") { return Index();}

        // o código da sessão é o código da pessoa, o mesmo usado em TarefaFuncionarios.funcionario_codigo
        int codigoFuncionario = (int)HttpContext.Session.GetInt32("codigo");

        List<Projeto> projetos = repository.ReadAllFuncionario(codigoFuncionario);
        repository.Dispose();

        // o nome da view vai escrito porque essa action também é chamada pela Index
        return View("IndexFuncionario", projetos);
    }

    // lista os projetos do cliente logado
    // GET /Projeto/ClienteProjeto
    public ActionResult ClienteProjeto()
    {
        if (HttpContext.Session.GetString("papel") != "cliente") { return Index();}

        // o código da sessão é o código da pessoa, o mesmo usado em Projetos.cliente_codigo
        int codigoCliente = (int)HttpContext.Session.GetInt32("codigo");

        List<Projeto> projetos = repository.ReadAllCliente(codigoCliente);
        repository.Dispose();

        // o nome da view vai escrito porque essa action também é chamada pela Index
        return View("IndexCliente", projetos);
    }

    // tela de detalhes pro funcionário, só de leitura (quem altera o projeto é o gestor)
    // GET /Projeto/DetalhesFuncionario/5
    public ActionResult DetalhesFuncionario(int id)
    {
        if (HttpContext.Session.GetString("papel") != "funcionario") { return Index();}

        Projeto? projeto = repository.Read(id);
        repository.Dispose();

        // projeto inexistente ou excluído: responde 404
        if (projeto == null)
        {
            return NotFound();
        }

        // entrega o projeto pra view, que acessa ele pelo @Model (por isso o "@model Projeto" no topo do cshtml)
        return View("DetalhesFuncionario", projeto);
    }

    // tela de detalhes pro cliente, só de leitura (quem altera o projeto é o gestor)
    // GET /Projeto/DetalhesCliente/5
    public ActionResult DetalhesCliente(int id)
    {
        if (HttpContext.Session.GetString("papel") != "cliente") { return Index(); }

        Projeto? projeto = repository.Read(id);
        repository.Dispose();

        // projeto inexistente ou excluído: responde 404
        if (projeto == null)
        {
            return NotFound();
        }

        // Cliente pode ver apenas seus próprios projetos
        if (projeto.Cliente.Codigo != HttpContext.Session.GetInt32("codigo"))
        {
            return Index(); // Se não estiver vinculado ao projeto, o cliente volta pro Index
        }

        // o cliente só vê as tarefas marcadas como visíveis (mesmo filtro da IndexCliente do TarefaController)
        projeto.tarefas = projeto.tarefas.Where(t => t.Visibilidade).ToList();

        // entrega o projeto pra view, que acessa ele pelo @Model (por isso o "@model Projeto" no topo do cshtml)
        return View("DetalhesCliente", projeto);
    }
}
