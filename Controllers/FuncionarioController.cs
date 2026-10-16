using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// CRUD de Funcionário utilizado pelo Gestor.
public class FuncionarioController : Controller
{
    // Lista os funcionários ativos.
    [HttpGet]
    public ActionResult IndexGestor()
    {
        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        List<Funcionario> funcionarios = repository.ReadAll();
        repository.Dispose();

        return View(funcionarios);
    }

    // Abre o formulário para criar um novo funcionário.
    [HttpGet]
    public ActionResult Criar()
    {
        CarregarCargos();
        ViewBag.ModoEdicao = false;
        return View(new Funcionario());
    }

    // Abre o mesmo formulário preenchido para edição.
    [HttpGet]
    public ActionResult Editar(int id)
    {
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
        bool editando = funcionario.Codigo > 0;

        if (string.IsNullOrWhiteSpace(funcionario.Nome) ||
            string.IsNullOrWhiteSpace(funcionario.Documento) ||
            string.IsNullOrWhiteSpace(funcionario.Email))
        {
            TempData["Erro"] = "Preencha nome, CPF e e-mail.";
            return RedirectToAction(editando ? "Editar" : "Criar", editando ? new { id = funcionario.Codigo } : null);
        }

        if (CargoCodigo <= 0)
        {
            TempData["Erro"] = "Selecione um cargo.";
            return RedirectToAction(editando ? "Editar" : "Criar", editando ? new { id = funcionario.Codigo } : null);
        }

        // Na criação a senha é obrigatória. Na edição, deixar em branco mantém a senha atual.
        if (!editando && string.IsNullOrWhiteSpace(funcionario.Senha))
        {
            TempData["Erro"] = "Informe uma senha para o funcionário.";
            return RedirectToAction("Criar");
        }

        if (!string.IsNullOrWhiteSpace(funcionario.Senha) && funcionario.Senha != ConfirmarSenha)
        {
            TempData["Erro"] = "A confirmação de senha não corresponde à senha informada.";
            return RedirectToAction(editando ? "Editar" : "Criar", editando ? new { id = funcionario.Codigo } : null);
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

                TempData[atualizado ? "Sucesso" : "Erro"] = atualizado
                    ? "Funcionário atualizado com sucesso."
                    : "Funcionário não encontrado.";
            }
            else
            {
                int codigo = repository.Create(funcionario);
                repository.Dispose();

                TempData[codigo > 0 ? "Sucesso" : "Erro"] = codigo > 0
                    ? "Funcionário cadastrado com sucesso."
                    : "O funcionário não pôde ser cadastrado.";
            }
        }
        catch (SqlException e)
        {
            TempData["Erro"] = "Não foi possível salvar o funcionário: " + e.Message;
        }

        return RedirectToAction("IndexGestor");
    }

    // Exclusão lógica. A coluna Ativo já existia justamente para permitir
    // que o histórico do funcionário seja mantido.
    [HttpPost]
    public ActionResult Excluir(int id)
    {
        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        bool resultado = repository.Delete(id);
        repository.Dispose();

        TempData[resultado ? "Sucesso" : "Erro"] = resultado
            ? "Funcionário desativado com sucesso."
            : "O funcionário não pôde ser desativado.";

        return RedirectToAction("IndexGestor");
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
