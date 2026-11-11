using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// CRUD de Funcionário utilizado pelo Gestor.
// toda action começa conferindo o papel da sessão: quem não é gestor (ou não está logado) volta pra Projeto/Index,
// que manda cada papel pra sua tela (ou pro login)
// o if vem antes de criar o repositório, assim a conexão nem chega a ser aberta
public class FuncionarioController : Controller
{
    // Lista os funcionários ativos.
    [HttpGet]
    public ActionResult IndexGestor()
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return RedirectToAction("Index", "Projeto"); }

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        List<Funcionario> funcionarios = repository.ReadAll();
        repository.Dispose();

        return View(funcionarios);
    }

    // Abre o formulário para criar um novo funcionário.
    [HttpGet]
    public ActionResult Criar()
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return RedirectToAction("Index", "Projeto"); }

        CarregarCargos();
        ViewBag.ModoEdicao = false;
        return View(new Funcionario());
    }

    // Abre o mesmo formulário preenchido para edição.
    [HttpGet]
    public ActionResult Editar(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return RedirectToAction("Index", "Projeto"); }

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        Funcionario? funcionario = repository.Read(id);
        repository.Dispose();

        if (funcionario == null)
        {
            return NotFound();
        }

        CarregarCargos();
        ViewBag.ModoEdicao = true;
        return View("Criar", funcionario);
    }

    // Salva criação ou edição.
    [HttpPost]
    public ActionResult Salvar(Funcionario funcionario, int CargoCodigo, string? ConfirmarSenha)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return RedirectToAction("Index", "Projeto"); }

        bool editando = funcionario.Codigo > 0;

        if (string.IsNullOrWhiteSpace(funcionario.Nome) ||
            string.IsNullOrWhiteSpace(funcionario.Documento) ||
            string.IsNullOrWhiteSpace(funcionario.Email))
        {
            ViewBag.Erro = "Preencha nome, CPF e e-mail.";
            CarregarCargos();
            ViewBag.ModoEdicao = editando;
            return View("Criar", funcionario);
        }

        // o CPF tem 11 números (o mascara.js já manda só os números)
        // fica depois da checagem acima porque ali o documento nulo já foi tratado
        if (funcionario.Documento.Length != 11)
        {
            ViewBag.Erro = "Digite o CPF (11 números) corretamente!";
            CarregarCargos();
            ViewBag.ModoEdicao = editando;
            return View("Criar", funcionario);
        }

        // telefone é opcional, mas se vier preenchido precisa ter DDD + número: 10 (fixo) ou 11 (celular) números
        if (!string.IsNullOrWhiteSpace(funcionario.Telefone) && funcionario.Telefone.Count(char.IsDigit) != 10 && funcionario.Telefone.Count(char.IsDigit) != 11)
        {
            ViewBag.Erro = "Digite o telefone com DDD: 10 números (fixo) ou 11 números (celular).";
            CarregarCargos();
            ViewBag.ModoEdicao = editando;
            return View("Criar", funcionario);
        }

        if (CargoCodigo <= 0)
        {
            ViewBag.Erro = "Selecione um cargo.";
            CarregarCargos();
            ViewBag.ModoEdicao = editando;
            return View("Criar", funcionario);
        }

        // Na criação a senha é obrigatória. Na edição, deixar em branco mantém a senha atual.
        if (!editando && string.IsNullOrWhiteSpace(funcionario.Senha))
        {
            ViewBag.Erro = "Informe uma senha para o funcionário.";
            CarregarCargos();
            ViewBag.ModoEdicao = editando;
            return View("Criar", funcionario);
        }

        if (!string.IsNullOrWhiteSpace(funcionario.Senha) && funcionario.Senha != ConfirmarSenha)
        {
            ViewBag.Erro = "A confirmação de senha não corresponde à senha informada.";
            CarregarCargos();
            ViewBag.ModoEdicao = editando;
            return View("Criar", funcionario);
        }

        funcionario.TipoDocumento = string.IsNullOrWhiteSpace(funcionario.TipoDocumento) ? "cpf" : funcionario.TipoDocumento.ToLower();
        funcionario.Cargo = new Cargo { Codigo = CargoCodigo };
        funcionario.Ativo = true;

        try
        {
            FuncionarioSqlRepository repository = new FuncionarioSqlRepository();

            if (editando)
            {
                bool atualizado = repository.Update(funcionario);
                repository.Dispose();

                if (atualizado)
                {
                    ViewBag.Sucesso = "Funcionário atualizado com sucesso.";
                }
                else
                {
                    ViewBag.Erro = "Funcionário não encontrado.";
                }
            }
            else
            {
                int codigo = repository.Create(funcionario);
                repository.Dispose();

                if (codigo > 0)
                {
                    ViewBag.Sucesso = "Funcionário cadastrado com sucesso.";
                }
                else
                {
                    ViewBag.Erro = "O funcionário não pôde ser cadastrado.";
                }
            }
        }
        catch (SqlException e)
        {
            ViewBag.Erro = "Não foi possível salvar o funcionário: " + e.Message;
        }

        FuncionarioSqlRepository repositoryLista = new FuncionarioSqlRepository();
        List<Funcionario> funcionarios = repositoryLista.ReadAll();
        repositoryLista.Dispose();

        return View("IndexGestor", funcionarios);
    }

    // Exclusão lógica. A coluna Ativo já existia justamente para permitir
    // que o histórico do funcionário seja mantido.
    [HttpPost]
    public ActionResult Excluir(int id)
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return RedirectToAction("Index", "Projeto"); }

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        bool resultado = repository.Delete(id);

        if (resultado)
        {
            ViewBag.Sucesso = "Funcionário desativado com sucesso.";
        }
        else
        {
            ViewBag.Erro = "O funcionário não pôde ser desativado.";
        }

        List<Funcionario> funcionarios = repository.ReadAll();
        repository.Dispose();

        return View("IndexGestor", funcionarios);
    }

    // Carrega os cargos cadastrados via INSERT para o select do formulário.
    private void CarregarCargos()
    {
        CargoSqlRepository repository = new CargoSqlRepository();
        ViewBag.Cargos = repository.ReadAll();
        repository.Dispose();
    }
}

// Compatibilidade com o endereço antigo usado em alguns links do protótipo.
// Ele apenas redireciona para o novo CRUD e pode ser removido quando todas as
// telas antigas deixarem de usar /CadastroFuncionario/CadastroFuncionario.
public class CadastroFuncionarioController : Controller
{
    [HttpGet]
    public ActionResult CadastroFuncionario()
    {
        return RedirectToAction("Criar", "Funcionario");
    }
}
