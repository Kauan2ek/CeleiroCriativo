using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// Controller das telas de Projeto utilizadas pelo Gestor.
// O Controller recebe os dados da View e delega o acesso ao banco aos Repositories.
public class ProjetoController : Controller
{
    // Página principal do Gestor: lista todos os projetos não excluídos.
    public ActionResult GestorProjeto()
    {
        ProjetoSqlRepository repository = new ProjetoSqlRepository();
        List<Projeto> projetos = repository.ReadAll();
        repository.Dispose();

        return View("IndexGestor", projetos);
    }

    // Alias com o nome da View, útil para manter uma rota MVC mais previsível.
    public ActionResult IndexGestor()
    {
        return GestorProjeto();
    }

    // ==================== CRIAÇÃO DE PROJETO ====================
    //
    // Rota simples para abrir o formulário de um novo Projeto:
    // /Projeto/Criar
    //
    // Antes havia apenas ExProjetoGestor?modo=criar. Apesar de funcionar em teoria,
    // uma Action própria deixa a navegação mais direta e evita ambiguidades de rota.
    [HttpGet]
    public ActionResult Criar()
    {
        Projeto projeto = new Projeto();

        // Um projeto criado internamente pelo Gestor começa como "Em andamento".
        // O código não fica fixo: ele é localizado pela descrição no banco.
        StatusSqlRepository statusRepository = new StatusSqlRepository();
        Status? statusInicial = statusRepository.ReadByDescricao("Em andamento");
        statusRepository.Dispose();

        if (statusInicial != null)
        {
            projeto.Status = statusInicial;
        }

        // A tela usa estas listas para montar os selects de Cliente e Status.
        CarregarListasAuxiliares();

        return View("DetalhesGestor", projeto);
    }

    // ==================== DETALHES DE PROJETO ====================
    //
    // Rota simples para abrir um Projeto existente:
    // /Projeto/Detalhes/5
    [HttpGet]
    public ActionResult Detalhes(int id)
    {
        ProjetoSqlRepository repository = new ProjetoSqlRepository();
        Projeto? projeto = repository.Read(id);
        repository.Dispose();

        // Em vez de devolver uma página 404 sem saída para o usuário,
        // voltamos à listagem e mostramos uma mensagem clara.
        if (projeto == null)
        {
            TempData["Erro"] = "O projeto informado não foi encontrado.";
            return RedirectToAction("GestorProjeto");
        }

        CarregarListasAuxiliares();
        return View("DetalhesGestor", projeto);
    }

    // ==================== ROTAS ANTIGAS ====================
    //
    // Mantemos estas Actions para não quebrar links antigos do projeto.
    // Elas apenas redirecionam para as rotas novas e mais simples.
    [HttpGet]
    public ActionResult ExProjetoGestor(int? id, string? modo)
    {
        if (id.HasValue)
        {
            return RedirectToAction("Detalhes", new { id = id.Value });
        }

        return RedirectToAction("Criar");
    }

    [HttpGet]
    public ActionResult DetalhesGestor(int id)
    {
        return RedirectToAction("Detalhes", new { id });
    }

    // ==================== SALVAR ====================
    // Salva tanto criação quanto edição.
    // O formulário de Projeto sempre envia para esta Action.
    // Codigo = 0 cria; Codigo > 0 atualiza.
    [HttpPost]
    public ActionResult Salvar(Projeto projeto, int ClienteCodigo, int StatusCodigo)
    {
        // Validação simples mantida no mesmo estilo do restante do projeto.
        if (string.IsNullOrWhiteSpace(projeto.Titulo))
        {
            TempData["Erro"] = "Informe o título do projeto.";
            return RedirecionarParaFormulario(projeto.Codigo);
        }

        if (ClienteCodigo <= 0)
        {
            TempData["Erro"] = "Selecione o cliente do projeto.";
            return RedirecionarParaFormulario(projeto.Codigo);
        }

        // Datas previstas/finais não podem ficar antes da data de início.
        if (projeto.DataInicio.HasValue && projeto.DataPrevista.HasValue && projeto.DataPrevista.Value < projeto.DataInicio.Value)
        {
            TempData["Erro"] = "A data prevista não pode ser anterior à data de início.";
            return RedirecionarParaFormulario(projeto.Codigo);
        }

        if (projeto.DataInicio.HasValue && projeto.DataFim.HasValue && projeto.DataFim.Value < projeto.DataInicio.Value)
        {
            TempData["Erro"] = "A data de fim não pode ser anterior à data de início.";
            return RedirecionarParaFormulario(projeto.Codigo);
        }

        // O select envia somente o código do Cliente. Criamos o objeto mínimo
        // necessário para que o Repository grave a chave estrangeira.
        projeto.Cliente = new Cliente { Codigo = ClienteCodigo };

        // Caso a tela não envie status na criação, usa "Em andamento".
        if (StatusCodigo <= 0)
        {
            StatusSqlRepository statusRepository = new StatusSqlRepository();
            Status? statusPadrao = statusRepository.ReadByDescricao("Em andamento");
            statusRepository.Dispose();

            if (statusPadrao == null)
            {
                TempData["Erro"] = "O status 'Em andamento' não foi encontrado no banco.";
                return RedirectToAction("GestorProjeto");
            }

            projeto.Status = statusPadrao;
        }
        else
        {
            projeto.Status = new Status { Codigo = StatusCodigo };
        }

        try
        {
            ProjetoSqlRepository repository = new ProjetoSqlRepository();

            // Código 0 significa que é um novo Projeto.
            if (projeto.Codigo == 0)
            {
                int codigo = repository.Create(projeto);
                repository.Dispose();

                TempData["Sucesso"] = "Projeto criado com sucesso.";

                // Depois de criar, abre o próprio Projeto recém-cadastrado.
                return RedirectToAction("Detalhes", new { id = codigo });
            }

            bool atualizado = repository.Update(projeto);
            repository.Dispose();

            TempData[atualizado ? "Sucesso" : "Erro"] = atualizado
                ? "Projeto atualizado com sucesso."
                : "Projeto não encontrado.";

            return RedirectToAction("Detalhes", new { id = projeto.Codigo });
        }
        catch (SqlException e)
        {
            // Mantemos a mensagem SQL nesta fase do projeto para facilitar os testes.
            TempData["Erro"] = "Não foi possível salvar o projeto: " + e.Message;
            return RedirecionarParaFormulario(projeto.Codigo);
        }
    }

    // Aprovação de uma solicitação enviada pela landing page.
    [HttpPost]
    public ActionResult Aprovar(int id)
    {
        ProjetoSqlRepository repository = new ProjetoSqlRepository();
        bool resultado = repository.Aprovar(id);
        repository.Dispose();

        TempData[resultado ? "Sucesso" : "Erro"] = resultado
            ? "Projeto aprovado e movido para Em andamento."
            : "O projeto não pôde ser aprovado.";

        return RedirectToAction("Detalhes", new { id });
    }

    // Negar é diferente de Excluir: por decisão do projeto, uma solicitação
    // ainda em análise é removida fisicamente do banco.
    [HttpPost]
    public ActionResult Negar(int id)
    {
        ProjetoSqlRepository repository = new ProjetoSqlRepository();
        bool resultado = repository.Negar(id);
        repository.Dispose();

        TempData[resultado ? "Sucesso" : "Erro"] = resultado
            ? "Projeto negado e removido."
            : "O projeto não pôde ser removido. Verifique se ele ainda está Em análise e sem tarefas.";

        return RedirectToAction("GestorProjeto");
    }

    // Exclusão normal do CRUD: lógica, alterando o status para "Excluído".
    [HttpPost]
    public ActionResult Excluir(int id)
    {
        ProjetoSqlRepository repository = new ProjetoSqlRepository();
        bool resultado = repository.Delete(id);
        repository.Dispose();

        TempData[resultado ? "Sucesso" : "Erro"] = resultado
            ? "Projeto excluído com sucesso."
            : "O projeto não pôde ser excluído.";

        return RedirectToAction("GestorProjeto");
    }

    // ==================== MÉTODOS AUXILIARES ====================

    // Carrega dados auxiliares usados pelos selects da View.
    private void CarregarListasAuxiliares()
    {
        ClienteSqlRepository clienteRepository = new ClienteSqlRepository();
        ViewBag.Clientes = clienteRepository.ReadAll();
        clienteRepository.Dispose();

        StatusSqlRepository statusRepository = new StatusSqlRepository();
        ViewBag.StatusList = statusRepository.ReadAll();
        statusRepository.Dispose();
    }

    // Se houve erro de validação, volta para a tela correta:
    // Código 0 -> criação; Código maior que 0 -> detalhes/edição.
    private ActionResult RedirecionarParaFormulario(int codigoProjeto)
    {
        if (codigoProjeto > 0)
        {
            return RedirectToAction("Detalhes", new { id = codigoProjeto });
        }

        return RedirectToAction("Criar");
    }

    // As telas dos demais perfis foram mantidas para não quebrar o projeto,
    // mas não fazem parte desta etapa de desenvolvimento.
    public ActionResult FuncionarioProjeto()
    {
        return View("IndexFuncionario");
    }

    public ActionResult ClienteProjeto()
    {
        return View("IndexCliente");
    }
}
