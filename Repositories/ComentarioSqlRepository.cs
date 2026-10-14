using System.Data;
using Microsoft.Data.SqlClient;

public class ComentarioSqlRepository : DataConnection
{
    public List<Comentario> ComentariosTarefa(int idTarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM V_Comentarios WHERE tarefa_codigo = @tarefa";
        cmd.Parameters.AddWithValue("@tarefa", idTarefa);
        
        SqlDataReader reader = cmd.ExecuteReader();
        List<Comentario> comentarios = new List<Comentario>();

        while (reader.Read())
        {
            Comentario comentario = new Comentario();
            comentario.Codigo     = reader.GetInt32("codigo");
            comentario.Descricao  = reader.GetString("descricao");
            comentario.DataHora   = reader.GetDateTime("data_hora");
            comentario.Funcionario = new Funcionario()
            {
                Codigo    = reader.GetInt32("funcionario_codigo"),
                Nome      = reader.GetString("funcionario_nome"),
                Telefone  = reader.IsDBNull("funcionario_telefone") ? null : reader.GetString("funcionario_telefone"),
                Email     = reader.GetString("funcionario_email"),
                Cargo     = new Cargo() {
                    Codigo    = reader.GetInt32("cargo_codigo"),
                    Descricao = reader.GetString("cargo_descricao") 
                }
            };

            comentarios.Add(comentario);
        }

        reader.Close();
        return comentarios;
    }
}