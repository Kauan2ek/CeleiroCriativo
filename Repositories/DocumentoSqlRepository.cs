using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;

// repositório dos documentos (arquivos) de uma tarefa
// aqui só tem sql: salvar e apagar o arquivo no disco é trabalho do DocumentoController
public class DocumentoSqlRepository : DataConnection
{
    // cadastra o documento pela procedure AdicionarDocumento
    // o arquivo já foi salvo em disco pelo controller; aqui vai só o "endereço" dele (diretorio)
    // a transação fica dentro da procedure (BEGIN TRAN / COMMIT / ROLLBACK), por isso aqui não abrimos SqlTransaction
    // se a tarefa não existir ou o banco recusar, a procedure dá THROW e chega no controller como SqlException
    public bool Create(Documento documento)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        // avisa que o CommandText é o nome de uma procedure, e não um comando sql
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"AdicionarDocumento";
        // os nomes dos parâmetros precisam ser iguais aos da procedure
        cmd.Parameters.AddWithValue("@tarefa_codigo", documento.IdTarefa);
        cmd.Parameters.AddWithValue("@descricao", documento.Descricao);
        cmd.Parameters.AddWithValue("@extensao", documento.Extensao);
        cmd.Parameters.AddWithValue("@diretorio", documento.Diretorio);

        // obs: se a procedure tiver SET NOCOUNT ON, o ExecuteNonQuery devolve -1 e isso dá false mesmo gravando
        // hoje o controller não usa esse retorno, então não atrapalha
        return cmd.ExecuteNonQuery() > 0;
    }




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

    // busca um documento pelo código
    // usado pelo Delete do controller pra descobrir o caminho do arquivo no disco antes de apagar
    // retorna null se o documento não existir
    public Documento? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM Documentos WHERE codigo = @codigo";
        cmd.Parameters.AddWithValue("@codigo", id);

        Documento? documento = null;
        SqlDataReader reader = cmd.ExecuteReader();

        // if em vez de while: o código é a chave, então vem no máximo uma linha
        if (reader.Read())
        {
            documento = new Documento();
            documento.Codigo = reader.GetInt32("codigo");
            documento.IdTarefa = reader.GetInt32("tarefa_codigo");
            documento.Descricao = reader.GetString("descricao");
            documento.Extensao = reader.GetString("extensao");
            documento.Diretorio = reader.GetString("diretorio");
        }

        // fecha o reader antes de sair: a conexão só aceita um reader aberto por vez,
        // e o controller usa a mesma conexão logo depois no Delete
        reader.Close();
        return documento;
    }

    // apaga a linha do documento na tabela Documentos
    // o arquivo em si é apagado do disco pelo controller
    // retorna true se alguma linha foi apagada
    public bool Delete(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "DELETE FROM Documentos WHERE codigo = @codigo";
        cmd.Parameters.AddWithValue("@codigo", id);

        // ExecuteNonQuery devolve quantas linhas foram apagadas
        return cmd.ExecuteNonQuery() > 0;
    }
}
