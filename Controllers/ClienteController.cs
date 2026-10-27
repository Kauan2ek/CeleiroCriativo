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
    public ActionResult Create(Cliente cliente)
    {
        try
        {
            int codigoCliente = repository.Create(cliente);

            repository.Dispose();
            // return Json(new {sucesso = true, codigo = codigoCliente});
            return View();
        }
        // se o banco recusar o comando, o repositório lança SqlException e ela "sobe" até aqui
        // o e.Message traz a mensagem do SQL Server explicando o que deu errado
        catch (SqlException e)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // busca um cliente pelo código
    // GET /Cliente/Read/6 (o 6 da url cai no parâmetro id, por causa da rota padrão do Program.cs)
    public ActionResult Read(int id)
    {
        // Adicionar try catch depois
        // o resultado é guardado numa variável pra dar tempo de fechar a conexão antes do return
        Cliente? cliente = repository.Read(id);

        // o repositório devolve null quando não existe cliente com esse código
        if (cliente == null)
        {
            repository.Dispose();
            // responde com o status http 404, sem JSON
            return NotFound();
        }
        repository.Dispose();
        return Json(new {sucesso = true, cliente});
    }

    // lista todos os clientes
    // GET /Cliente/ReadAll
    [HttpGet]
    public ActionResult ReadAll()
    {
        // Adicionar try catch depois
        List<Cliente> clientes = repository.ReadAll();

        // lista vazia: nenhum cliente cadastrado
        if (clientes.Count() == 0)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = "Nenhum cliente encontrado"});
        }
        repository.Dispose();
        return Json(new {sucesso = true, clientes});
    }

    // altera um cliente que já existe
    // POST /Cliente/Update
    // recebe os mesmos campos do Create, mais o Codigo do cliente que vai ser alterado
    // o resultado volta false quando não existe cliente com esse código
    [HttpPost]
    public ActionResult Update(Cliente cliente)
    {
        try
        {
            bool resultado = repository.Update(cliente);

            repository.Dispose();
            return Json(new {sucesso = resultado});
        }
        catch (SqlException e)
        {
            repository.Dispose();
            return Json(new {sucesso = false, erro = e.Message});
        }
    }

    // exclui um cliente
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
