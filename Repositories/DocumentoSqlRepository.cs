using System.Data;
using Microsoft.Data.SqlClient;

// repositório dos documentos (arquivos) de uma tarefa
// por enquanto só lê: o upload de arquivo ainda está pendente
public class DocumentoSqlRepository : DataConnection
{
    // lista os documentos anexados à tarefa informada
    // usado pelo TarefaSqlRepository.Read pra mostrar a coluna "Arquivos" da tela da tarefa
    // se a tarefa não tiver documento, retorna uma lista vazia (nunca null, apesar do ? no tipo)
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