using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// Controller das Tarefas.
// Para o Gestor, toda tela de tarefa trabalha dentro do contexto de um Projeto.
public class TarefaController : Controller
{
    // Página própria de Tarefas de um Projeto.
    // O id recebido aqui é o código do PROJETO.
    [HttpGet]
    public ActionResult GestorTarefa(int id)
    {
        if (id <= 0)
        {
            TempData["Erro"] = "Selecione um projeto para visualizar suas tarefas.";
            return RedirectToAction("GestorProjeto", "Projeto");
        }

        ProjetoSqlRepository projetoRepository = new ProjetoSqlRepository();
        Projeto? projeto = projetoRepository.Read(id);
        projetoRepository.Dispose();

        if (projeto == null)
        {
            return NotFound();
        }

        // A View recebe a lista de tarefas e usa o Projeto pelo ViewBag
        // para montar título, links de voltar e criação.
        ViewBag.Projeto = projeto;
        return View("IndexGestor", projeto.tarefas);
    }

    // Alias mais próximo do nome da View.
    [HttpGet]
    public ActionResult IndexGestor(int id)
    {
        return GestorTarefa(id);
    }

    // Abre uma tarefa existente ou o formulário de criação.
    // id = código da Tarefa; projetoCodigo = Projeto usado quando a tarefa é nova.
    [HttpGet]
    public ActionResult ExTarefaGestor(int? id, int projetoCodigo = 0)
    {
        Tarefa tarefa;

        if (id.HasValue)
        {
            TarefaSqlRepository repository = new TarefaSqlRepository();
            Tarefa? tarefaEncontrada = repository.Read(id.Value);
            repository.Dispose();

            if (tarefaEncontrada == null)
            {
                return NotFound();
            }

            tarefa = tarefaEncontrada;
        }
        else
        {
            if (projetoCodigo <= 0)
            {
                TempData["Erro"] = "Não foi possível identificar o projeto da nova tarefa.";
                return RedirectToAction("GestorProjeto", "Projeto");
            }

            // O fluxo do banco considera que projetos "Em análise" ainda não possuem tarefas.
            // Por isso a criação só é liberada depois da aprovação.
            ProjetoSqlRepository projetoRepository = new ProjetoSqlRepository();
            Projeto? projeto = projetoRepository.Read(projetoCodigo);
            projetoRepository.Dispose();

            if (projeto == null)
            {
                return NotFound();
            }

            if (projeto.Status.Descricao == "Em análise")
            {
                TempData["Erro"] = "Aprove o projeto antes de cadastrar tarefas.";
                return RedirectToAction("Detalhes", "Projeto", new { id = projetoCodigo });
            }

            tarefa = new Tarefa
            {
                IdProjeto = projetoCodigo,
                Status = "A fazer",
                Visibilidade = true
            };
        }

        CarregarListasAuxiliares();
        return View("DetalhesGestor", tarefa);
    }

    // Alias para abrir a tarefa por /Tarefa/DetalhesGestor/5.
    [HttpGet]
    public ActionResult DetalhesGestor(int id)
    {
        return ExTarefaGestor(id, 0);
    }

    // Salva criação e edição da tarefa.
    [HttpPost]
    public ActionResult Salvar(Tarefa tarefa, int CategoriaCodigo, int[]? FuncionariosSelecionados)
    {
        if (tarefa.IdProjeto <= 0)
        {
            TempData["Erro"] = "A tarefa precisa estar vinculada a um projeto.";
            return RedirectToAction("GestorProjeto", "Projeto");
        }

        if (string.IsNullOrWhiteSpace(tarefa.Titulo) || string.IsNullOrWhiteSpace(tarefa.Descricao))
        {
            TempData["Erro"] = "Preencha o título e a descrição da tarefa.";
            return RedirectToAction("ExTarefaGestor", new { id = tarefa.Codigo > 0 ? tarefa.Codigo : (int?)null, projetoCodigo = tarefa.IdProjeto });
        }

        if (CategoriaCodigo <= 0)
        {
            TempData["Erro"] = "Selecione uma categoria para a tarefa.";
            return RedirectToAction("ExTarefaGestor", new { id = tarefa.Codigo > 0 ? tarefa.Codigo : (int?)null, projetoCodigo = tarefa.IdProjeto });
        }

        if (tarefa.DataHoraInicio.HasValue && tarefa.DataHoraPrevista.HasValue && tarefa.DataHoraPrevista.Value < tarefa.DataHoraInicio.Value)
        {
            TempData["Erro"] = "A data prevista não pode ser anterior à data de início.";
            return RedirectToAction("ExTarefaGestor", new { id = tarefa.Codigo > 0 ? tarefa.Codigo : (int?)null, projetoCodigo = tarefa.IdProjeto });
        }

        if (tarefa.DataHoraInicio.HasValue && tarefa.DataHoraFim.HasValue && tarefa.DataHoraFim.Value < tarefa.DataHoraInicio.Value)
        {
            TempData["Erro"] = "A data de fim não pode ser anterior à data de início.";
            return RedirectToAction("ExTarefaGestor", new { id = tarefa.Codigo > 0 ? tarefa.Codigo : (int?)null, projetoCodigo = tarefa.IdProjeto });
        }

        tarefa.Categoria = new Categoria { Codigo = CategoriaCodigo };
        tarefa.Status = string.IsNullOrWhiteSpace(tarefa.Status) ? "A fazer" : tarefa.Status;

        // Converte os códigos marcados no formulário para a lista de Funcionário
        // esperada pelo Repository já existente.
        tarefa.Funcionarios = new List<Funcionario>();
        if (FuncionariosSelecionados != null)
        {
            foreach (int codigoFuncionario in FuncionariosSelecionados)
            {
                tarefa.Funcionarios.Add(new Funcionario { Codigo = codigoFuncionario });
            }
        }

        try
        {
            TarefaSqlRepository repository = new TarefaSqlRepository();
            bool resultado;

            if (tarefa.Codigo == 0)
            {
                resultado = repository.Create(tarefa);
            }
            else
            {
                resultado = repository.Update(tarefa);
            }

            repository.Dispose();

            TempData[resultado ? "Sucesso" : "Erro"] = resultado
                ? (tarefa.Codigo == 0 ? "Tarefa criada com sucesso." : "Tarefa atualizada com sucesso.")
                : "A tarefa não pôde ser salva.";

            return RedirectToAction("GestorTarefa", new { id = tarefa.IdProjeto });
        }
        catch (SqlException e)
        {
            TempData["Erro"] = "Não foi possível salvar a tarefa: " + e.Message;
            return RedirectToAction("GestorTarefa", new { id = tarefa.IdProjeto });
        }
    }

    // Exclusão lógica usada pela interface do Gestor.
    [HttpPost]
    public ActionResult Excluir(int id, int projetoCodigo)
    {
        TarefaSqlRepository repository = new TarefaSqlRepository();
        bool resultado = repository.Delete(id);
        repository.Dispose();

        TempData[resultado ? "Sucesso" : "Erro"] = resultado
            ? "Tarefa excluída com sucesso."
            : "A tarefa não pôde ser excluída.";

        return RedirectToAction("GestorTarefa", new { id = projetoCodigo });
    }

    // Carrega os dados auxiliares do formulário de tarefa.
    private void CarregarListasAuxiliares()
    {
        CategoriaSqlRepository categoriaRepository = new CategoriaSqlRepository();
        ViewBag.Categorias = categoriaRepository.ReadAll();
        categoriaRepository.Dispose();

        FuncionarioSqlRepository funcionarioRepository = new FuncionarioSqlRepository();
        // Inclui inativos para preservar funcionários que já estejam ligados à tarefa.
        // A View esconde inativos que não estejam atualmente selecionados.
        ViewBag.FuncionariosDisponiveis = funcionarioRepository.ReadAll(true);
        funcionarioRepository.Dispose();
    }

    // -----------------------------------------------------------------
    // Actions em JSON mantidas para compatibilidade e testes do CRUD.
    // -----------------------------------------------------------------

    [HttpPost]
    public ActionResult Create(Tarefa tarefa)
    {
        try
        {
            TarefaSqlRepository repository = new TarefaSqlRepository();
            bool resultado = repository.Create(tarefa);
            repository.Dispose();
            return Json(new { sucesso = resultado });
        }
        catch (SqlException e)
        {
            return Json(new { sucesso = false, erro = e.Message });
        }
    }

    [HttpGet]
    public ActionResult Read(int id)
    {
        TarefaSqlRepository repository = new TarefaSqlRepository();
        Tarefa? tarefa = repository.Read(id);
        repository.Dispose();

        if (tarefa == null)
        {
            return NotFound();
        }

        return Json(new { sucesso = true, tarefa });
    }

    // O id desta action é o código do Projeto.
    [HttpGet]
    public ActionResult ReadAll(int id)
    {
        TarefaSqlRepository repository = new TarefaSqlRepository();
        List<Tarefa> tarefas = repository.ReadAll(id);
        repository.Dispose();

        // Corrige o erro antigo que devolvia sucesso=false mesmo encontrando tarefas.
        return Json(new { sucesso = true, tarefas });
    }

    [HttpPost]
    public ActionResult Update(Tarefa tarefa)
    {
        try
        {
            TarefaSqlRepository repository = new TarefaSqlRepository();
            bool resultado = repository.Update(tarefa);
            repository.Dispose();
            return Json(new { sucesso = resultado });
        }
        catch (SqlException e)
        {
            return Json(new { sucesso = false, erro = e.Message });
        }
    }

    [HttpPost]
    public ActionResult Delete(int id)
    {
        try
        {
            TarefaSqlRepository repository = new TarefaSqlRepository();
            bool resultado = repository.Delete(id);
            repository.Dispose();
            return Json(new { sucesso = resultado });
        }
        catch (SqlException e)
        {
            return Json(new { sucesso = false, erro = e.Message });
        }
    }

    // Demais perfis ficam fora do escopo atual, mas as rotas existentes são mantidas.
    public ActionResult FuncionarioTarefa()
    {
        return View("IndexFuncionario");
    }

    public ActionResult ClienteTarefa()
    {
        return View("IndexCliente");
    }

    public ActionResult DetalhesFuncionario()
    {
        return View();
    }

    public ActionResult DetalhesCliente()
    {
        return View();
    }
}
