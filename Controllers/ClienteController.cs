// importa as funcionalidades necessárias do asp.net core mvc
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// controller responsável pelas ações relacionadas aos clientes
public class ClienteController : Controller
{
    // repositório que faz o acesso ao banco, o controller nunca escreve sql
    private ClienteSqlRepository repository;

    // o asp.net cria um controller novo a cada requisição, então esse construtor roda toda vez
    // criar o repositório já abre a conexão com o banco (é o construtor da DataConnection que faz isso)
    public ClienteController()
    {
        repository = new ClienteSqlRepository();
    }

    // CRUD do cliente
    // ainda não existem telas de cliente, então todas as actions devolvem JSON (dá pra testar pelo Bruno ou pelo navegador)
    // quando as telas existirem, o Create, Update e Delete passam a redirecionar, como no TarefaController
    // todas seguem o mesmo roteiro: chamam o repositório, fecham a conexão com o Dispose() e montam a resposta
    // o Dispose() é chamado antes de cada return, porque o que vem depois do return não executa

    // cria um cliente
    // POST /Cliente/Create
    // o parâmetro "cliente" é montado sozinho pelo asp.net (model binding):
    // ex: Nome=Café Aurora, Documento=11.111.111/0001-11, TipoDocumento=cnpj, Email=contato@cafe.com
    [HttpPost]
    public ActionResult Create(Cliente cliente, string ideia)
    {
        try
        {
            if (repository.EmailExiste(cliente.Email))
            {
                repository.Dispose();
                ViewBag.Erro = "Este e-mail já está cadastrado. Entre com ele ou use outro e-mail.";
                return View("~/Views/Usuario/Entrar.cshtml");
            }

            // documento nulo (formulário enviado sem o campo) vira texto vazio, senão o Replace abaixo daria erro
            if (cliente.Documento == null) { cliente.Documento = ""; }

            // Limpar o documento (deixar só números)
            cliente.Documento = cliente.Documento.Replace(".", "").Replace("-", "")
                                                 .Replace("/", "").Replace(" ", "");

            // validação feita DEPOIS da limpeza, pra contar só os números: CPF tem 11 e CNPJ tem 14
            if (cliente.Documento.Length != 11 && cliente.Documento.Length != 14)
            {
                repository.Dispose();
                ViewBag.Erro = "Digite o CPF (11 números) ou o CNPJ (14 números) corretamente!";
                return View("~/Views/Home/Index.chtml#cadastro");
            }

            // telefone é opcional, mas se vier preenchido precisa ter DDD + número: 10 (fixo) ou 11 (celular) números
            if (!string.IsNullOrWhiteSpace(cliente.Telefone) && cliente.Telefone.Count(char.IsDigit) != 10 && cliente.Telefone.Count(char.IsDigit) != 11)
            {
                repository.Dispose();
                ViewBag.Erro = "Digite o telefone com DDD: 10 números (fixo) ou 11 números (celular).";
                return View("~/Views/Usuario/Entrar.cshtml");
            }

            cliente.TipoDocumento = cliente.Documento.Length > 11 ? "cnpj" : "cpf";

            // Montando o projeto
            Projeto projeto = new Projeto
            {
                Titulo = "Aguardando aprovação...",
                Descricao = ideia,
                DataInicio = null,
                DataFim = null,
                DataPrevista = null,
                Status = {Codigo=1, Descricao="Em Análise"},
                tarefas = new List<Tarefa>(),
                Cliente = cliente
            };
 
            repository.Create(cliente, projeto);
            

            repository.Dispose();
            // return Json(new {sucesso = true, codigo = codigoCliente});
            ViewBag.Sucesso = "Cadastro realizado com sucesso! Sua ideia já foi enviada para análise. Entre para acompanhar seu projeto.";
            return View("~/Views/Usuario/Entrar.cshtml");
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        catch (SqlException e)
        {
            repository.Dispose();
            ViewBag.Erro = "Não foi possível concluir o cadastro. Verifique se o CPF/CNPJ já não está cadastrado.";
            return View("~/Views/Usuario/Entrar.cshtml");
        }
    }

    // exclui um cliente // Atualmente não está implementado
    // POST /Cliente/Delete/6
    // atenção: cliente com projeto não pode ser apagado (FK_Projetos_Cliente), aí o banco recusa e cai no catch
    [HttpPost]
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
