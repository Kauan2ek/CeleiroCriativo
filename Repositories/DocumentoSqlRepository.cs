using System.Data;
using Microsoft.Data.SqlClient;

public class DocumentoSqlRepository : DataConnection
{
    public List<Documento>? DocumentosTarefa(int idTarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM Documentos WHERE tarefa_codigo = @tarefa";
        cmd.Parameters.AddWithValue("@tarefa", idTarefa);

        SqlDataReader reader = cmd.ExecuteReader();
        List<Documento> documentos = new List<Documento>();

        while (reader.Read())
        {
            Documento documento = new Documento();
            documento.Codigo = reader.GetInt32("codigo");
            documento.Descricao = reader.GetString("descricao");
            documento.Extensao = reader.GetString("extensao");
            documento.Diretorio = reader.GetString("diretorio");

            documentos.Add(documento);
        }

        reader.Close();
        return documentos;
    }
}