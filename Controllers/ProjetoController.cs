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
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index(); }
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

            // trava da regra de negócio: projeto finalizado ou em análise abre só pra leitura
            // (em análise ainda dá pra Aprovar, Negar ou Excluir, que ficam fora do formulário)
            if (projeto.Status.Descricao == "Finalizado")
            {
                ViewBag.Aviso = "Projeto já finalizado. Para editá-lo, altere o status.";
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
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}

        // a data de início é o dia em que o projeto é criado: preenchida aqui no servidor
        // (o valor que veio do campo readonly da tela é só uma prévia e é ignorado, igual ao Create da tarefa)
        projeto.DataInicio = DateTime.Today;

        // Validações (as mesmas do Update, menos a da data de término, que no Create nunca vem da tela)
        if (string.IsNullOrWhiteSpace(projeto.Titulo))   { ViewBag.Erro = "Informe o título do projeto!";                          }
        if (projeto.DataInicio > projeto.DataPrevista)   { ViewBag.Erro = "A data prevista não pode ser antes da data de início."; }

        // o Codigo do projeto novo é 0, então a DetalhesGestor reabre o formulário de projeto novo mostrando o erro
        if (ViewBag.Erro is not null) { return DetalhesGestor(projeto.Codigo); }

        // projeto criado já como "Finalizado" (o select do projeto novo permite): a entrega é a data de hoje
        // mesma regra do Update: o select só envia o Status.Codigo, então o código do "Finalizado" é buscado pela descrição
        StatusSqlRepository statusSqlRepository = new StatusSqlRepository();
        Status? statusFinalizado = statusSqlRepository.ReadByDescricao("Finalizado");
        statusSqlRepository.Dispose();

        if (statusFinalizado != null && projeto.Status.Codigo == statusFinalizado.Codigo)
        {
            projeto.DataFim = DateTime.Today;
        }

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
        if (HttpContext.Session.GetString("papel") != "gestor") {
            ViewBag.Erro = "Você não tem permissão para acessar essa página!"; 
            return Index();
        }

        // busca o projeto como está NO BANCO: serve pra data de início (logo abaixo) e pra trava do projeto finalizado (mais embaixo)
        Projeto? projetoBanco = repository.Read(projeto.Codigo);

        // data de início bloqueada: vale a que já está no banco (o valor que veio do campo readonly da tela é ignorado)
        // projeto que ainda não tem data de início (ex.: veio da landing page): grava a data de hoje
        // fica antes das validações pra elas compararem com a data de início de verdade
        if (projetoBanco != null)
        {
            projeto.DataInicio = projetoBanco.DataInicio == null ? DateTime.Today : projetoBanco.DataInicio;
        }

        // Validações
        // DataInicio > DataFim quer dizer que o término ficou ANTES do início (por isso as mensagens dizem "antes")
        if (projeto.Titulo == "Aguardando aprovação..." || string.IsNullOrWhiteSpace(projeto.Titulo)) { ViewBag.Erro = "Altere o título do projeto!"; }
        if (projeto.DataInicio > projeto.DataFim)      { ViewBag.Erro = "A data de término não pode ser antes da data de início."; }
        if (projeto.DataInicio > projeto.DataPrevista) { ViewBag.Erro = "A data prevista não pode ser antes da data de início.";   }

        if (ViewBag.Erro is not null) { return DetalhesGestor(projeto.Codigo); }

        // código do status "Finalizado", buscado pela descrição (mesma ideia do statusInicial da DetalhesGestor)
        // precisa disso porque o select de status só envia o Status.Codigo: o projeto.Status.Descricao chega vazio
        StatusSqlRepository statusSqlRepository = new StatusSqlRepository();
        Status? statusFinalizado = statusSqlRepository.ReadByDescricao("Finalizado");
        statusSqlRepository.Dispose();

        // trava da regra de negócio: projeto finalizado só pode ter o STATUS alterado
        // (projeto em andamento ou em análise não entra nesse if: todos os campos são gravados normalmente)
        // confere o status que está NO BANCO (projetoBanco), e não o que veio do formulário (projeto):
        // senão o próprio ato de mudar o projeto pra "Finalizado" e clicar em Salvar já travaria os outros campos
        // (o projetoBanco já foi lido lá em cima, antes das validações)
        if (projetoBanco != null && projetoBanco.Status.Descricao == "Finalizado")
        {
            // na tela travada só o select de status fica liberado: os outros campos estão disabled,
            // e campo disabled não é enviado no formulário (chegariam vazios e o repository.Update apagaria título, cliente e datas)
            // por isso o que vai pro banco é o projeto do banco, trocando só o status pelo que veio da tela
            // (isso também impede que alguém altere os outros campos enviando o formulário por fora da tela)
            projetoBanco.Status = projeto.Status;
            projeto = projetoBanco;

            // projeto REABERTO (saiu de "Finalizado"): a data de entrega volta a ficar vazia
            // quando ele for finalizado de novo, cai no "else if" de baixo e ganha a data nova
            if (statusFinalizado != null && projeto.Status.Codigo != statusFinalizado.Codigo)
            {
                projeto.DataFim = null;
            }
        }
        // projeto que ACABOU de ser finalizado (no banco ainda não estava): a data de entrega é a data atual
        // como é "else if", um projeto que já estava finalizado não tem a data trocada a cada "Salvar Status"
        else if (statusFinalizado != null && projeto.Status.Codigo == statusFinalizado.Codigo)
        {
            projeto.DataFim = DateTime.Today;
        }

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

        // trava da regra de negócio: projeto finalizado não pode ser excluído
        // a tela já esconde o botão Excluir nesse caso, mas a conferência aqui impede que o formulário seja enviado mesmo assim
        // não dá Dispose antes de voltar: quem fecha a conexão é a DetalhesGestor (ela usa o mesmo repository)
        Projeto? projetoBanco = repository.Read(id);
        if (projetoBanco != null && projetoBanco.Status.Descricao == "Finalizado")
        {
            ViewBag.Erro = "Não é possível excluir um projeto finalizado.";
            return DetalhesGestor(id);
        }

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
