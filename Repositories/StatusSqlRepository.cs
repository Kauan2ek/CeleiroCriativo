using System.Data;
using Microsoft.Data.SqlClient;

// Repository de Status de Projeto.
// Status de Tarefa NÃO usa esta tabela; na Tarefa o status é um texto simples.
public class StatusSqlRepository : DataConnection
{
    // Lista os status disponíveis para Projeto.
    // Por padrão o status "Excluído" não aparece em selects de edição.
    public List<Status> ReadAll(bool incluirExcluido = false)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT codigo, descricao
            FROM Status
            WHERE (@incluirExcluido = 1 OR descricao <> 'Excluído')
            ORDER BY codigo
        ";
        cmd.Parameters.AddWithValue("@incluirExcluido", incluirExcluido);

        List<Status> statusList = new List<Status>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            statusList.Add(new Status(
                reader.GetInt32("codigo"),
                reader.GetString("descricao")
            ));
        }

        reader.Close();
        return statusList;
    }

    // Busca um status pela descrição. É útil para localizar códigos sem
    // deixar números fixos espalhados pelo Controller/Repository.
    public Status? ReadByDescricao(string descricao)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT codigo, descricao FROM Status WHERE descricao = @descricao";
        cmd.Parameters.AddWithValue("@descricao", descricao);

        Status? status = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            status = new Status(
                reader.GetInt32("codigo"),
                reader.GetString("descricao")
            );
        }

        reader.Close();
        return status;
    }
}
