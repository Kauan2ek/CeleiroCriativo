using System.Data;
using Microsoft.Data.SqlClient;

// Repository de Cliente.
// Nesta etapa ele é usado principalmente para preencher o select de clientes
// na criação/edição de um Projeto.
public class ClienteSqlRepository : DataConnection
{
    // Cadastra Pessoa + Cliente e devolve o código gerado.
    // TODO: implementar (dá pra usar a procedure CadastrarPessoa com @tipo_pessoa = 'CLIENTE').
    public bool Create(Cliente cliente, Projeto projeto)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "CadastrarCliente";
        cmd.Parameters.AddWithValue("@nome", cliente.Nome);
        cmd.Parameters.AddWithValue("@documento", cliente.Documento);
        cmd.Parameters.AddWithValue("@tipo_doc", cliente.TipoDocumento);
        cmd.Parameters.AddWithValue("@email", cliente.Email);
        cmd.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(cliente.Telefone) ? DBNull.Value : cliente.Telefone);
        cmd.Parameters.AddWithValue("@senha", cliente.Senha);

        SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            cliente.Codigo = reader.GetInt32("pessoa_codigo");
            projeto.Cliente = cliente;

            ProjetoSqlRepository repProjeto = new ProjetoSqlRepository();
            repProjeto.Create(projeto);
            repProjeto.Dispose();
        } else
        {
            reader.Close();
            return false;
        }

        reader.Close();
        return true;
    }

    // Altera os dados de Pessoa + Cliente pela procedure AtualizarCliente.
    // A transação fica dentro da procedure, e a senha em branco mantém a senha atual (COALESCE(NULLIF(@senha, ''), senha)).
    // A procedure devolve a linha do cliente alterado; se não devolver nada, o Update retorna false.
    public bool Update(Cliente cliente)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "AtualizarCliente";
        cmd.Parameters.AddWithValue("@cliente_codigo", cliente.Codigo);
        cmd.Parameters.AddWithValue("@nome", cliente.Nome);
        cmd.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(cliente.Telefone) ? DBNull.Value : cliente.Telefone);
        cmd.Parameters.AddWithValue("@documento", cliente.Documento);
        cmd.Parameters.AddWithValue("@tipo_doc", cliente.TipoDocumento);
        cmd.Parameters.AddWithValue("@email", cliente.Email);
        cmd.Parameters.AddWithValue("@codigo_verificacao", cliente.CodigoVerificacao == null ? DBNull.Value : cliente.CodigoVerificacao);
        cmd.Parameters.AddWithValue("@senha", string.IsNullOrWhiteSpace(cliente.Senha) ? DBNull.Value : cliente.Senha);

        SqlDataReader reader = cmd.ExecuteReader();
        bool resultado = reader.Read();

        reader.Close();
        return resultado;
    }

    // Exclui o cliente. Devolve false se o cliente não existir.
    // TODO: implementar (decidir entre exclusão física ou lógica; Clientes não tem coluna "ativo").
    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    // Lista todos os clientes cadastrados no banco.
    public List<Cliente> ReadAll()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT P.codigo, P.nome, P.telefone, P.documento, P.tipo_doc,
                   C.codigo_verificacao, C.email, C.senha
            FROM Clientes C
            INNER JOIN Pessoas P ON P.codigo = C.pessoa_codigo
            ORDER BY P.nome
        ";

        List<Cliente> clientes = new List<Cliente>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            clientes.Add(MontarCliente(reader));
        }

        reader.Close();
        return clientes;
    }

    // Busca um cliente específico pelo código da Pessoa/Cliente.
    public Cliente? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT P.codigo, P.nome, P.telefone, P.documento, P.tipo_doc,
                   C.codigo_verificacao, C.email, C.senha
            FROM Clientes C
            INNER JOIN Pessoas P ON P.codigo = C.pessoa_codigo
            WHERE P.codigo = @codigo
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        Cliente? cliente = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            cliente = MontarCliente(reader);
        }

        reader.Close();
        return cliente;
    }

    // Método auxiliar para não repetir a montagem do objeto em todos os SELECTs.
    private Cliente MontarCliente(SqlDataReader reader)
    {
        Cliente cliente = new Cliente();
        cliente.Codigo = reader.GetInt32("codigo");
        cliente.Nome = reader.GetString("nome");
        cliente.Telefone = reader.IsDBNull("telefone") ? null : reader.GetString("telefone");
        cliente.Documento = reader.GetString("documento");
        cliente.TipoDocumento = reader.GetString("tipo_doc");
        cliente.CodigoVerificacao = reader.IsDBNull("codigo_verificacao") ? null : reader.GetString("codigo_verificacao");
        cliente.Email = reader.GetString("email");
        cliente.Senha = reader.IsDBNull("senha") ? null : reader.GetString("senha");
        return cliente;
    }

    // confere se o e-mail já está em uso por algum cliente OU funcionário
    // o login procura nas duas tabelas, então o e-mail não pode se repetir em nenhuma delas
    public bool EmailExiste(string email)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT COUNT(*)
            FROM (SELECT email FROM Clientes UNION ALL SELECT email FROM Funcionarios) E
            WHERE E.email = @email
        ";
        cmd.Parameters.AddWithValue("@email", email);

        return (int) cmd.ExecuteScalar() > 0;
    }
}
