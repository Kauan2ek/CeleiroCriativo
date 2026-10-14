using Microsoft.Data.SqlClient;
using System.Data;

public class CategoriaSqlRepository : DataConnection
{
    public List<Categoria> ReadAll()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM Categorias";

        List<Categoria>? categorias = new List<Categoria>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Categoria categoria = new Categoria();
            categoria.Codigo = reader.GetInt32("codigo");
            categoria.Descricao = reader.GetString("descricao");
            categoria.Cor = reader.IsDBNull("cor") ? null : reader.GetString("cor");

            categorias.Add(categoria);
        }

        reader.Close();
        return categorias;
    }

    // Listar por tarefa - porém na tarefa ele já traz pela VIEW (sugestão de excluir caso não utilize)
    public Categoria? Read(int idTarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM Comentarios WHERE tarefa_codigo = @tarefa";
        cmd.Parameters.AddWithValue("@tarefa", idTarefa);

        Categoria? categoria = null;
        SqlDataReader reader = cmd.ExecuteReader();
        
        if (reader.Read())
        {
            categoria = new Categoria();
            categoria.Codigo = reader.GetInt32("codigo");
            categoria.Descricao = reader.GetString("Descricao");
            categoria.Cor = reader.IsDBNull("cor") ? null : reader.GetString("cor");
        }

        reader.Close();
        return categoria;
    }
}