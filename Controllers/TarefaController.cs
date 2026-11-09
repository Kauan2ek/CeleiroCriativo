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

    // porta de entrada do kanban: cada papel vê a sua tela (igual à Index do ProjetoController)
    // GET /Tarefa/Index/2 (o id é o código do PROJETO)
    // o Create e o Delete voltam pra cá, assim cada um cai no kanban do seu papel
    public ActionResult Index(int id)
    {
        string? papel = HttpContext.Session.GetString("papel");

        if (papel == "gestor")      { return IndexGestor(id);      }
        if (papel == "funcionario") { return IndexFuncionario(id); }
        if (papel == "cliente")     { return IndexCliente(id);     }

        return RedirectToAction("Entrar", "Usuario");
    }

    // porta de entrada da tela da tarefa: cada papel vê a sua tela
    // GET /Tarefa/Detalhes/5 (o id é o código da TAREFA)
    // o Update, o ComentarioController e o DocumentoController voltam pra cá
    public ActionResult Detalhes(int id, int idProjeto, string? erro = null)
    {
        string? papel = HttpContext.Session.GetString("papel");

        if (papel == "gestor")      { return DetalhesGestor(id, idProjeto, erro);      }
        if (papel == "funcionario") { return DetalhesFuncionario(id, idProjeto, erro); }
        if (papel == "cliente")     { return DetalhesCliente(id);                      }

        return RedirectToAction("Entrar", "Usuario");
    }

    // abre o kanban com as tarefas de um projeto, pro gestor
    // GET /Tarefa/IndexGestor/2 (aqui o id da url é o código do PROJETO, não da tarefa)
    // segue a convenção: views/tarefa/indexgestor.cshtml
    public ActionResult IndexGestor(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index(id); }

        List<Tarefa> tarefas = repository.ReadAll(id);
        repository.Dispose();

        // o ReadAll não preenche o IdProjeto de cada tarefa,
        // então o código do projeto vai pelo ViewBag pros links da tela (voltar e criar tarefa)
        ViewBag.IdProjeto = id;

        // o nome da view vai escrito porque essa action também é chamada pela Index:
        // sem o nome, o asp.net procuraria uma view com o nome da action que chamou (Index.cshtml)
        return View("IndexGestor", tarefas);
    }

    // kanban pro funcionário: vê (e edita) todas as tarefas do projeto (decisão do grupo)
    // GET /Tarefa/IndexFuncionario/2 (o id é o código do PROJETO)
    public ActionResult IndexFuncionario(int id)
    {
        if (HttpContext.Session.GetString("papel") != "funcionario") { return Index(id); }

        List<Tarefa> tarefas = repository.ReadAll(id);
        repository.Dispose();

        ViewBag.IdProjeto = id;

        return View("IndexFuncionario", tarefas);
    }

    // kanban pro cliente: só do projeto dele, e só as tarefas marcadas como visíveis
    // GET /Tarefa/IndexCliente/2 (o id é o código do PROJETO)
    public ActionResult IndexCliente(int id)
    {
        if (HttpContext.Session.GetString("papel") != "cliente") { return Index(id); }

        // cliente não pode abrir o kanban de projeto de outro cliente trocando o número na url
        if (!ProjetoDoCliente(id))
        {
            repository.Dispose();
            return RedirectToAction("Index", "Projeto");
        }

        List<Tarefa> tarefas = repository.ReadAll(id);
        repository.Dispose();

        // o Where filtra a lista: fica só quem tem Visibilidade = true (o mesmo Where que a view do gestor usa nas colunas)
        // o ToList transforma o resultado do Where de volta em List<Tarefa>
        tarefas = tarefas.Where(t => t.Visibilidade).ToList();

        ViewBag.IdProjeto = id;

        return View("IndexCliente", tarefas);
    }

    // tela de detalhes pro gestor, já com os dados reais da tarefa
    // GET /Tarefa/DetalhesGestor/5 (o 5 da url cai no parâmetro id)
    // o erro vem pela url quando outro controller redireciona pra cá (ex: o catch do ComentarioController)
    // o "= null" deixa o erro opcional: os catch daqui chamam DetalhesGestor(id, idProjeto) sem ele
    public ActionResult DetalhesGestor(int id, int idProjeto, string? erro = null)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Detalhes(id, idProjeto, erro); }

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

        ProjetoSqlRepository projetoSqlRepository = new ProjetoSqlRepository();
        Status? statusProjeto = projetoSqlRepository.GetStatus(tarefa.IdProjeto); 
        projetoSqlRepository.Dispose();

        ViewBag.StatusProjeto = statusProjeto?.Descricao;

        if (statusProjeto?.Descricao == "Finalizado")
        {
            ViewBag.Aviso = "Não é possível editar tarefa com projeto finalizado.";
        } 
        
        if (tarefa.Status == "Finalizado")
        {
            ViewBag.Aviso = "Não é possível editar tarefa finalizada.";
        }

        // entrega a tarefa pra view, que acessa ela pelo @Model (por isso o "@model Tarefa" no topo do cshtml)
        // o nome da view vai escrito porque essa action também é chamada pelo catch do Create, Update e Delete:
        // sem o nome, o asp.net procuraria uma view com o nome da action que chamou (ex: Update.cshtml)
        return View("DetalhesGestor", tarefa);
    }

    // tela da tarefa pro funcionário: igual à do gestor, mas sem mexer na equipe (equipe é só do gestor)
    // ele edita, exclui e sobe arquivo em qualquer tarefa do projeto
    // GET /Tarefa/DetalhesFuncionario/5
    // GET /Tarefa/DetalhesFuncionario?idProjeto=2 (sem id, abre o formulário de tarefa nova)
    public ActionResult DetalhesFuncionario(int id, int idProjeto, string? erro = null)
    {
        if (HttpContext.Session.GetString("papel") != "funcionario") { return Detalhes(id, idProjeto, erro); }

        if (erro != null)
        {
            ViewBag.Erro = erro;
        }

        Tarefa? tarefa = new Tarefa();
        tarefa.IdProjeto = idProjeto;

        if (id != 0)
        {
            tarefa = repository.Read(id);
            repository.Dispose();

            if (tarefa == null)
            {
                return NotFound();
            }
        }
        else
        {
            repository.Dispose();
        }

        // todas as categorias, pras opções do select de categoria
        // (o ViewBag.Funcionarios não vem aqui: o funcionário não tem o modal de equipe)
        CategoriaSqlRepository categoriaSqlRepository = new CategoriaSqlRepository();
        ViewBag.Categorias = categoriaSqlRepository.ReadAll();
        categoriaSqlRepository.Dispose();

        // mesma trava da DetalhesGestor: tarefa finalizada, ou de projeto finalizado, abre só pra leitura
        // usa o tarefa.IdProjeto (vem do banco), porque o idProjeto da url chega 0 quando a tela abre por /Tarefa/DetalhesFuncionario/5
        ProjetoSqlRepository projetoSqlRepository = new ProjetoSqlRepository();
        Status? statusProjeto = projetoSqlRepository.GetStatus(tarefa.IdProjeto);
        projetoSqlRepository.Dispose();

        // a view usa esse valor pra montar o bool editavel
        ViewBag.StatusProjeto = statusProjeto?.Descricao;

        if (statusProjeto?.Descricao == "Finalizado")
        {
            ViewBag.Aviso = "Não é possível editar tarefa com projeto finalizado.";
        }

        if (tarefa.Status == "Finalizado")
        {
            ViewBag.Aviso = "Não é possível editar tarefa finalizada.";
        }

        return View("DetalhesFuncionario", tarefa);
    }

    // tela da tarefa pro cliente: só leitura, mas com tudo da tarefa (equipe, comentários e arquivos)
    // GET /Tarefa/DetalhesCliente/5
    public ActionResult DetalhesCliente(int id)
    {
        if (HttpContext.Session.GetString("papel") != "cliente") { return Detalhes(id, 0); }

        Tarefa? tarefa = repository.Read(id);
        repository.Dispose();

        if (tarefa == null)
        {
            return NotFound();
        }

        // tarefa escondida do cliente, ou de projeto de outro cliente: volta pra lista de projetos dele
        if (!tarefa.Visibilidade || !ProjetoDoCliente(tarefa.IdProjeto))
        {
            return RedirectToAction("Index", "Projeto");
        }

        return View("DetalhesCliente", tarefa);
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
        string? papel = HttpContext.Session.GetString("papel");

        // só gestor e funcionário criam tarefa (o cliente só visualiza)
        // a tarefa criada pelo funcionário nasce sem equipe: quem monta a equipe é o gestor
        if (papel != "gestor" && papel != "funcionario") { return Index(tarefa.IdProjeto); }

        try
        {
            int codigoTarefa = repository.Create(tarefa);

            repository.Dispose();
            // volta pro kanban do projeto, pela Index, que escolhe a tela do papel de quem está logado
            // o new {id = ...} cria um objeto sem classe que preenche o {id} da rota: vira /Tarefa/Index/2
            return RedirectToAction("Index", "Tarefa", new {id = tarefa.IdProjeto});
            // return Json(new {sucesso = resultado});
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        // outros tipos de erro (ex: categoria vazia) não entram nesse catch
        catch (SqlException e)
        {
            // volta pra tela da tarefa mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a tela de detalhes do papel (chamada pela Detalhes)
            ViewBag.Erro = e.Message;
            return Detalhes(tarefa.Codigo, tarefa.IdProjeto);
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // altera uma tarefa que já existe
    // POST /Tarefa/Update
    // recebe os mesmos campos do Create, mais o Codigo da tarefa que vai ser alterada
    // o resultado volta false quando não existe tarefa com esse código (mesmo assim redireciona pra tela da tarefa)
    [HttpPost]
    public ActionResult Update(Tarefa tarefa)
    {
        string? papel = HttpContext.Session.GetString("papel");

        // só gestor e funcionário alteram tarefa
        if (papel != "gestor" && papel != "funcionario") { return Detalhes(tarefa.Codigo, tarefa.IdProjeto); }

        // a tela do funcionário não tem o modal de equipe (equipe é só do gestor), então a equipe chega vazia no formulário
        // e o repository.Update apagaria todo mundo da tarefa: por isso a equipe que vale é a que já está no banco
        if (papel == "funcionario")
        {
            Tarefa? atual = repository.Read(tarefa.Codigo);

            if (atual == null)
            {
                return Detalhes(tarefa.Codigo, tarefa.IdProjeto);
            }

            tarefa.Funcionarios = atual.Funcionarios;
        }

        // trava da regra de negócio: tarefa finalizada, ou de projeto finalizado, não pode ser alterada
        // a tela já esconde o botão Salvar nesses casos, mas a conferência aqui impede que o formulário seja enviado mesmo assim
        // o status conferido é o que está NO BANCO (tarefaBanco), e não o que veio do formulário (tarefa):
        // senão o próprio ato de mudar a tarefa pra "Finalizado" e clicar em Salvar seria bloqueado
        Tarefa? tarefaBanco = repository.Read(tarefa.Codigo);

        if (tarefaBanco == null)
        {
            return Detalhes(tarefa.Codigo, tarefa.IdProjeto);
        }

        ProjetoSqlRepository projetoSqlRepository = new ProjetoSqlRepository();
        Status? statusProjeto = projetoSqlRepository.GetStatus(tarefaBanco.IdProjeto);
        projetoSqlRepository.Dispose();

        if (statusProjeto?.Descricao == "Finalizado") { ViewBag.Erro = "Não é possível alterar tarefa de projeto finalizado."; }
        if (tarefaBanco.Status == "Finalizado")       { ViewBag.Erro = "Não é possível alterar uma tarefa finalizada.";        }
        // não dá Dispose antes de voltar: quem fecha a conexão é a tela de detalhes do papel (ela usa o mesmo repository)
        if (ViewBag.Erro is not null) { return Detalhes(tarefa.Codigo, tarefa.IdProjeto); }

        try
        {
            bool resultado = repository.Update(tarefa);

            repository.Dispose();
            // volta pra tela da própria tarefa, já com os dados salvos: /Tarefa/Detalhes/5
            // o Codigo chega preenchido porque o formulário da tarefa sempre envia o hidden Codigo
            return RedirectToAction("Detalhes", "Tarefa", new {id = tarefa.Codigo});
            // return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            // volta pra tela da tarefa mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a tela de detalhes do papel, depois de buscar a tarefa de novo
            ViewBag.Erro = e.Message;
            return Detalhes(tarefa.Codigo, tarefa.IdProjeto);
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
        string? papel = HttpContext.Session.GetString("papel");

        // só gestor e funcionário excluem tarefa
        if (papel != "gestor" && papel != "funcionario") { return Detalhes(id, idProjeto); }

        try
        {
            bool resultado = repository.Delete(id);

            repository.Dispose();
            // volta pro kanban do projeto, pela Index, que escolhe a tela do papel: /Tarefa/Index/2
            return RedirectToAction("Index", "Tarefa", new {id = idProjeto});
            // return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            // volta pra tela da tarefa mostrando o erro (o ViewBag chega porque não tem redirecionamento)
            // quem fecha a conexão é a tela de detalhes do papel
            ViewBag.Erro = e.Message;
            return Detalhes(id, idProjeto);
            // return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // ==================== MÉTODOS AUXILIARES ====================

    // confere se o projeto é do cliente logado (mesma regra da DetalhesCliente do ProjetoController)
    private bool ProjetoDoCliente(int idProjeto)
    {
        ProjetoSqlRepository projetoSqlRepository = new ProjetoSqlRepository();
        Projeto? projeto = projetoSqlRepository.Read(idProjeto);
        projetoSqlRepository.Dispose();

        if (projeto == null)
        {
            return false;
        }

        return projeto.Cliente.Codigo == HttpContext.Session.GetInt32("codigo");
    }
}