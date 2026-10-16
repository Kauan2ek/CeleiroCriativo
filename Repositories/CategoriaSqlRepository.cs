using Microsoft.Data.SqlClient;
using System.Data;

// Repository auxiliar das Categorias de Tarefa.
public class CategoriaSqlRepository : DataConnection
{
    // Categorias são cadastradas via INSERT nesta etapa; o CRUD de Tarefa
    // apenas precisa listá-las para o select.
    public List<Categoria> ReadAll()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT codigo, descricao, cor FROM Categorias ORDER BY descricao";

        List<Categoria> categorias = new List<Categoria>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            categorias.Add(new Categoria
            {
                Codigo = reader.GetInt32("codigo"),
                Descricao = reader.GetString("descricao"),
                Cor = reader.IsDBNull("cor") ? null : reader.GetString("cor")
            });
        }

        reader.Close();
        return categorias;
    }

    // Busca uma categoria pelo próprio código.
    // Corrige o método antigo, que consultava a tabela Comentarios por engano.
    public Categoria? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT codigo, descricao, cor FROM Categorias WHERE codigo = @codigo";
        cmd.Parameters.AddWithValue("@codigo", id);

        Categoria? categoria = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            categoria = new Categoria
            {
                Codigo = reader.GetInt32("codigo"),
                Descricao = reader.GetString("descricao"),
                Cor = reader.IsDBNull("cor") ? null : reader.GetString("cor")
            };
        }

        reader.Close();
        return categoria;
    }
}
