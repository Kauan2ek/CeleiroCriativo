// importa as funcionalidades necessárias do asp.net core mvc
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// controller responsável pelas telas de perfil
// as views se chamam Gestor.cshtml, Funcionario.cshtml e Cliente.cshtml
// cada tela mostra os dados de quem está logado: o código vem sempre da sessão, nunca da url,
// então um usuário não consegue abrir nem alterar o perfil de outro
// o gestor também é um funcionário no banco, por isso as telas Gestor e Funcionario usam o FuncionarioSqlRepository
public class PerfilController : Controller
{
    // GET /Perfil
    // abre o perfil certo de acordo com o papel guardado na sessão (mesma ideia do ProjetoController.Index)
    public ActionResult Index()
    {
        string? papel = HttpContext.Session.GetString("papel");

        if (papel == "gestor")      { return Gestor();      }
        if (papel == "funcionario") { return Funcionario(); }
        if (papel == "cliente")     { return Cliente();     }

        return RedirectToAction("Entrar", "Usuario");
    }

    // endereços usados pelos ícones de perfil das outras telas
    // GET /Perfil/GestorPerfil, /Perfil/FuncionarioPerfil e /Perfil/ClientePerfil
    public ActionResult GestorPerfil()
    {
        return Gestor();
    }

    public ActionResult FuncionarioPerfil()
    {
        return Funcionario();
    }

    public ActionResult ClientePerfil()
    {
        return Cliente();
    }

    // perfil do gestor
    // GET /Perfil/Gestor
    public ActionResult Gestor()
    {
        if (HttpContext.Session.GetString("papel") != "gestor") { return Index();}

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        Funcionario? funcionario = repository.Read((int)HttpContext.Session.GetInt32("codigo"));
        repository.Dispose();

        // usuário da sessão não existe mais no banco: responde 404
        if (funcionario == null)
        {
            return NotFound();
        }

        // o nome da view vai escrito porque essa action também é chamada pela Index, pela GestorPerfil e pelo Salvar:
        // sem o nome, o asp.net procuraria uma view com o nome da action que chamou (ex: SalvarFuncionario.cshtml)
        return View("Gestor", funcionario);
    }

    // perfil do funcionário
    // GET /Perfil/Funcionario
    public ActionResult Funcionario()
    {
        if (HttpContext.Session.GetString("papel") != "funcionario") { return Index();}

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();
        Funcionario? funcionario = repository.Read((int)HttpContext.Session.GetInt32("codigo"));
        repository.Dispose();

        if (funcionario == null)
        {
            return NotFound();
        }

        return View("Funcionario", funcionario);
    }

    // perfil do cliente
    // GET /Perfil/Cliente
    public ActionResult Cliente()
    {
        if (HttpContext.Session.GetString("papel") != "cliente") { return Index();}

        ClienteSqlRepository repository = new ClienteSqlRepository();
        Cliente? cliente = repository.Read((int)HttpContext.Session.GetInt32("codigo"));
        repository.Dispose();

        if (cliente == null)
        {
            return NotFound();
        }

        return View("Cliente", cliente);
    }

    // Salvar e Desativar são chamados pelos formulários das telas de perfil:
    // devolvem a própria tela de perfil com o ViewBag.Sucesso ou o ViewBag.Erro
    // não usam RedirectToAction porque o ViewBag se perde no redirecionamento:
    // chamam a Index direto, que abre o perfil certo pelo papel da sessão

    // salva o perfil do gestor ou do funcionário
    // POST /Perfil/SalvarFuncionario
    // o parâmetro "funcionario" é montado sozinho pelo asp.net (model binding): Nome, Telefone, Email e Senha
    // o documento e o cargo não podem ser alterados pelo perfil, então vêm do banco, e não da tela
    // senha em branco mantém a senha atual (o Update só grava a senha quando ela vem preenchida)
    [HttpPost]
    public ActionResult SalvarFuncionario(Funcionario funcionario, string? ConfirmarSenha)
    {
        string? papel = HttpContext.Session.GetString("papel");
        if (papel != "gestor" && papel != "funcionario") { return Index();}

        if (string.IsNullOrWhiteSpace(funcionario.Nome) || string.IsNullOrWhiteSpace(funcionario.Email))
        {
            ViewBag.Erro = "Preencha nome e e-mail.";
            return Index();
        }

        if (!string.IsNullOrWhiteSpace(funcionario.Senha) && funcionario.Senha != ConfirmarSenha)
        {
            ViewBag.Erro = "A confirmação de senha não corresponde à senha informada.";
            return Index();
        }

        // o e-mail só é conferido se mudou, porque o EmailExiste também encontraria o e-mail do próprio usuário
        // o EmailExiste fica no ClienteSqlRepository, mas procura em Clientes e em Funcionarios
        if (funcionario.Email != HttpContext.Session.GetString("email"))
        {
            ClienteSqlRepository clienteSqlRepository = new ClienteSqlRepository();
            bool emailExiste = clienteSqlRepository.EmailExiste(funcionario.Email);
            clienteSqlRepository.Dispose();

            if (emailExiste)
            {
                ViewBag.Erro = "Este e-mail já está sendo usado por outro usuário.";
                return Index();
            }
        }

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();

        try
        {
            // busca o funcionário no banco pra manter o código, o documento e o cargo atuais
            Funcionario? atual = repository.Read((int)HttpContext.Session.GetInt32("codigo"));

            if (atual == null)
            {
                repository.Dispose();
                return NotFound();
            }

            funcionario.Codigo = atual.Codigo;
            funcionario.Documento = atual.Documento;
            funcionario.TipoDocumento = atual.TipoDocumento;
            funcionario.Cargo = atual.Cargo;

            bool resultado = repository.Update(funcionario);
            repository.Dispose();

            if (resultado)
            {
                // a sessão guarda o nome e o e-mail, então atualiza também
                HttpContext.Session.SetString("nome", funcionario.Nome);
                HttpContext.Session.SetString("email", funcionario.Email);
                ViewBag.Sucesso = "Perfil atualizado com sucesso.";
            }
            // volta pra tela de perfil, já com os dados salvos
            return Index();
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        catch (SqlException e)
        {
            repository.Dispose();
            ViewBag.Erro = e.Message;
            return Index();
        }
    }

    // salva o perfil do cliente
    // POST /Perfil/SalvarCliente
    // mesmo roteiro do SalvarFuncionario (o documento e o código de verificação vêm do banco)
    [HttpPost]
    public ActionResult SalvarCliente(Cliente cliente, string? ConfirmarSenha)
    {
        if (HttpContext.Session.GetString("papel") != "cliente") { return Index();}

        if (string.IsNullOrWhiteSpace(cliente.Nome) || string.IsNullOrWhiteSpace(cliente.Email))
        {
            ViewBag.Erro = "Preencha nome e e-mail.";
            return Index();
        }

        if (!string.IsNullOrWhiteSpace(cliente.Senha) && cliente.Senha != ConfirmarSenha)
        {
            ViewBag.Erro = "A confirmação de senha não corresponde à senha informada.";
            return Index();
        }

        ClienteSqlRepository repository = new ClienteSqlRepository();

        try
        {
            if (cliente.Email != HttpContext.Session.GetString("email") && repository.EmailExiste(cliente.Email))
            {
                repository.Dispose();
                ViewBag.Erro = "Este e-mail já está sendo usado por outro usuário.";
                return Index();
            }

            Cliente? atual = repository.Read((int)HttpContext.Session.GetInt32("codigo"));

            if (atual == null)
            {
                repository.Dispose();
                return NotFound();
            }

            cliente.Codigo = atual.Codigo;
            cliente.Documento = atual.Documento;
            cliente.TipoDocumento = atual.TipoDocumento;
            cliente.CodigoVerificacao = atual.CodigoVerificacao;

            bool resultado = repository.Update(cliente);
            repository.Dispose();

            if (resultado)
            {
                HttpContext.Session.SetString("nome", cliente.Nome);
                HttpContext.Session.SetString("email", cliente.Email);
                ViewBag.Sucesso = "Perfil atualizado com sucesso.";
            }
            return Index();
        }
        catch (SqlException e)
        {
            repository.Dispose();
            ViewBag.Erro = e.Message;
            return Index();
        }
    }

    // desativa o próprio funcionário ou gestor (exclusão lógica: ativo = 0, igual ao Excluir do FuncionarioController)
    // POST /Perfil/Desativar, enviado pelo botão Desativar das telas Gestor e Funcionario
    // o cliente não tem essa opção
    // depois de desativar, o login já não aceita mais esse usuário, então limpa a sessão igual ao Sair
    [HttpPost]
    public ActionResult Desativar()
    {
        string? papel = HttpContext.Session.GetString("papel");
        if (papel != "gestor" && papel != "funcionario") { return Index();}

        FuncionarioSqlRepository repository = new FuncionarioSqlRepository();

        try
        {
            bool resultado = repository.Delete((int)HttpContext.Session.GetInt32("codigo"));
            repository.Dispose();

            if (!resultado)
            {
                ViewBag.Erro = "Não foi possível desativar o perfil.";
                return Index();
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        catch (SqlException e)
        {
            repository.Dispose();
            ViewBag.Erro = e.Message;
            return Index();
        }
    }
}
