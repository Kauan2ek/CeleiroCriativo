using System.Data;
using Microsoft.Data.SqlClient;

// Repository de Cliente.
// Nesta etapa ele é usado principalmente para preencher o select de clientes
// na criação/edição de um Projeto.
public class ClienteSqlRepository : DataConnection
{
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
}
