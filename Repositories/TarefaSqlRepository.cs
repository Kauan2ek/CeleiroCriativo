// o System.Data traz métodos extras pro reader (GetInt32, GetString, GetDateTime...)
// que recebem o NOME da coluna em vez do índice dela
// por baixo ele procura a posição da coluna pelo nome e lê o valor dessa posição
// aqui é usado no Listar(), pra ler cada coluna sem depender da ordem delas na tabela
// o nome passado tem que ser o nome da coluna no banco (ou no SELECT), não o da propriedade da model
using System.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public class TarefaSqlRepository: DataConnection
{
    public bool Create(Tarefa tarefa)
    {
        // executenonquery
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
        insert into Tarefas
            (projeto_codigo, categoria_codigo, titulo, descricao, status,
             datahora_inicio, datahora_fim, datahora_prevista, visibilidade)
            values 
            (@projeto, @categoria, @titulo, @descricao, @status,
             @inicio, @fim, @prevista, @visibilidade)
        ";
        
        cmd.Parameters.AddWithValue("@projeto", tarefa.IdProjeto);
        cmd.Parameters.AddWithValue("@categoria", tarefa.Categoria.Codigo);
        cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
        cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
        cmd.Parameters.AddWithValue("@status", tarefa.Status);
        
        // datas podem ser nulas, e o banco não entende o null do C#, precisa usar o DBNull.Value
        // HasValue verifica se a data foi preenchida: se sim manda a data, se não manda DBNull.Value
        // o (object) é porque os dois lados do ? : precisam ser do mesmo tipo,
        // e DateTime? e DBNull não têm nada em comum, então os dois viram object
        cmd.Parameters.AddWithValue("@inicio", tarefa.DataHoraInicio.HasValue ? (object) tarefa.DataHoraInicio : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", tarefa.DataHoraPrevista.HasValue ? (object) tarefa.DataHoraPrevista : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", tarefa.DataHoraFim.HasValue ? (object) tarefa.DataHoraFim : DBNull.Value);

        cmd.Parameters.AddWithValue("@visibilidade", tarefa.Visibilidade);

        // ExecuteNonQuery é pra comandos que não devolvem linhas (INSERT, UPDATE, DELETE)
        bool resultado = cmd.ExecuteNonQuery() > 0; // Retorna a quantidade de linhas afetadas (se for maior que 0, é porque foi criado)
        
        return resultado;
    }

    public Tarefa? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM V_Tarefas WHERE codigo = @id";
        cmd.Parameters.AddWithValue("@id", id);

        Tarefa? tarefa = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read()) // verificar se o registro
        {   
            tarefa = new Tarefa();
            tarefa.Codigo = reader.GetInt32("codigo");
            tarefa.Titulo = reader.GetString("titulo");
            tarefa.Descricao = reader.GetString("descricao");
            tarefa.DataHoraInicio = reader.IsDBNull("datahora_inicio") ? null : reader.GetDateTime("DataHora_Inicio");
            tarefa.DataHoraPrevista = reader.IsDBNull("datahora_prevista") ? null : reader.GetDateTime("DataHora_Prevista");
            tarefa.DataHoraFim = reader.IsDBNull("datahora_fim") ? null : reader.GetDateTime("DataHora_Fim");
            tarefa.Visibilidade = reader.GetBoolean("visibilidade");
            tarefa.Status = reader.GetString("status");
            
            //Implementar as outras classes
            tarefa.Funcionarios = new List<Funcionario>();
            // Exemplo: tarefa.Funcionarios = FuncionarioSqlRepository.ListarPorTarefa(tarefa.Codigo);
            tarefa.Comentarios = new List<Comentario>();
            tarefa.Documentos = new List<Documento>();
        }

        reader.Close();
        return tarefa;
    }

    public List<Tarefa> ReadAll(int idProjeto)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM V_Tarefas WHERE projeto_codigo = @IdProjeto";
        cmd.Parameters.AddWithValue("@IdProjeto", idProjeto);

        List<Tarefa> tarefas = new List<Tarefa>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Tarefa tarefa = new Tarefa();
            tarefa.Codigo = reader.GetInt32("codigo");
            tarefa.Titulo = reader.GetString("titulo");
            tarefa.Descricao = reader.GetString("descricao");
            tarefa.DataHoraInicio = reader.IsDBNull("datahora_inicio") ? null : reader.GetDateTime("DataHora_Inicio");
            tarefa.DataHoraPrevista = reader.IsDBNull("datahora_prevista") ? null : reader.GetDateTime("DataHora_Prevista");
            tarefa.DataHoraFim = reader.IsDBNull("datahora_fim") ? null : reader.GetDateTime("DataHora_Fim");
            tarefa.Visibilidade = reader.GetBoolean("visibilidade");
            tarefa.Status = reader.GetString("status");
            
            //Implementar as outras classes
            tarefa.Funcionarios = new List<Funcionario>();
            // Exemplo: tarefa.Funcionarios = FuncionarioSqlRepository.ListarPorTarefa(tarefa.Codigo);
            tarefa.Comentarios = new List<Comentario>();
            tarefa.Documentos = new List<Documento>();

            tarefas.Add(tarefa);
        }

        reader.Close();
        return tarefas;
    }


    public void Delete(int id)
    {
        throw new NotImplementedException();
    }
}