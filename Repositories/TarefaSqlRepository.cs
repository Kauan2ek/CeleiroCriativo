// o System.Data traz métodos extras pro reader (GetInt32, GetString, GetDateTime...)
// que recebem o NOME da coluna em vez do índice dela
// por baixo ele procura a posição da coluna pelo nome e lê o valor dessa posição
// aqui é usado no Listar(), pra ler cada coluna sem depender da ordem delas na tabela
// o nome passado tem que ser o nome da coluna no banco (ou no SELECT), não o da propriedade da model
using System.Data;
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
             @inicio, @fim, @prevista, @visibilidade);

        SELECT CAST(SCOPE_IDENTITY() AS INT);
        ";
        
        
        cmd.Parameters.AddWithValue("@projeto", tarefa.IdProjeto);
        cmd.Parameters.AddWithValue("@categoria", tarefa.Categoria.Codigo);
        cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
        cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
        cmd.Parameters.AddWithValue("@status", tarefa.Status == null ? "A Fazer" : tarefa.Status);
        
        // datas podem ser nulas, e o banco não entende o null do C#, precisa usar o DBNull.Value
        // HasValue verifica se a data foi preenchida: se sim manda a data, se não manda DBNull.Value
        // o (object) é porque os dois lados do ? : precisam ser do mesmo tipo,
        // e DateTime? e DBNull não têm nada em comum, então os dois viram object
        cmd.Parameters.AddWithValue("@inicio", tarefa.DataHoraInicio.HasValue ? (object) tarefa.DataHoraInicio : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", tarefa.DataHoraPrevista.HasValue ? (object) tarefa.DataHoraPrevista : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", tarefa.DataHoraFim.HasValue ? (object) tarefa.DataHoraFim : DBNull.Value);

        cmd.Parameters.AddWithValue("@visibilidade", tarefa.Visibilidade);

        // ExecuteScalar retorna valor único (Codigo - Tarefa)
        int IdTarefa = (int) cmd.ExecuteScalar(); // Retorna a quantidade de linhas afetadas (se for maior que 0, é porque foi criado)
        
        if (tarefa.Funcionarios != null)
        {
            // Teste: TarefaFuncionariosSqlRepository tarefaFuncionarios = new TarefaFuncionariosSqlRepository();
            AtualizarFuncionarios(IdTarefa, tarefa.Funcionarios);
        }

        if (tarefa.Comentarios != null)
        {
            // Chamar o método ComentarioRepository.Create();
        }

        if (tarefa.Documentos != null)
        {
            // Chamar o método DocumentoRepository.Create();
        }

        return IdTarefa > 0; // Se retornou o id da tarefa, é porque deu certo, então retorna true, senão false
    }

    public List<Funcionario> ListarFuncionario(int idTarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT funcionario_codigo FROM TarefaFuncionarios WHERE tarefa_codigo = @id";

        cmd.Parameters.AddWithValue("id", idTarefa);

        SqlDataReader reader = cmd.ExecuteReader();

        List<Funcionario> funcionarios = new List<Funcionario>();
        while (reader.Read())
        {
            Funcionario funcionario = new Funcionario();
            funcionario.Codigo = reader.GetInt32(0);

            funcionarios.Add(funcionario);
        }
        
        reader.Close();
        return funcionarios;
    }

    public void AtualizarFuncionarios (int idTarefa, List<Funcionario> funcionarios)
    {
        List<int> ListaAtual = new List<int>();
        foreach (var funcionario in ListarFuncionario(idTarefa))
        {
            ListaAtual.Add(funcionario.Codigo);
        }

        List<int> ListaNova = new List<int>();
        foreach (var funcionario in funcionarios)
        {
            ListaNova.Add(funcionario.Codigo);    
        }

        List<int> adicionar = ListaNova.Except(ListaAtual).ToList();
        List<int> remover = ListaAtual.Except(ListaNova).ToList();

        foreach (var codigo in adicionar)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"
            insert into TarefaFuncionarios
                (funcionario_codigo, tarefa_codigo)
            values 
                (@funcionario, @tarefa)
            ";
            cmd.Parameters.AddWithValue("@funcionario", codigo);
            cmd.Parameters.AddWithValue("@tarefa", idTarefa);

            cmd.ExecuteNonQuery();
        }

        foreach (var codigo in remover)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"
            DELETE FROM TarefaFuncionarios WHERE funcionario_codigo = @codigo AND tarefa_codigo = @tarefa;
            DELETE FROM Comentarios WHERE funcionario_codigo = @codigo AND tarefa_codigo = @tarefa;
            ";
            cmd.Parameters.AddWithValue("@codigo", codigo);
            cmd.Parameters.AddWithValue("@tarefa", idTarefa);

            cmd.ExecuteNonQuery();
        }
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
            tarefa.Categoria = new Categoria(){
                Codigo = reader.GetInt32("categoria_codigo"), 
                Descricao = reader.GetString("categoria_descricao"), 
                Cor = reader.GetString("categoria_cor")
            };
            tarefa.IdProjeto = reader.GetInt32("projeto_codigo");
            tarefa.Funcionarios = ListarFuncionario(id);
            
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
            tarefa.Categoria = new Categoria(){
                Codigo = reader.GetInt32("categoria_codigo"), 
                Descricao = reader.GetString("categoria_descricao"), 
                Cor = reader.GetString("categoria_cor")
            };
            
            //Implementar as outras classes
            //tarefa.Funcionarios = ListarFuncionario(tarefa.Codigo);
            tarefa.Funcionarios = new List<Funcionario>();
            // Exemplo: tarefa.Funcionarios = FuncionarioSqlRepository.ListarPorTarefa(tarefa.Codigo);
            tarefa.Comentarios = new List<Comentario>();
            tarefa.Documentos = new List<Documento>();

            tarefas.Add(tarefa);
        }

        reader.Close();
        return tarefas;
    }

    public bool Update(Tarefa tarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            UPDATE Tarefas 
            SET categoria_codigo = @categoria, titulo = @titulo, descricao = @descricao,
                status = @status, datahora_inicio = @inicio, datahora_fim = @fim,
                datahora_prevista = @prevista, visibilidade = @visibilidade
            WHERE codigo = @codigo
        ";
        cmd.Parameters.AddWithValue("@codigo", tarefa.Codigo);
        cmd.Parameters.AddWithValue("@categoria", tarefa.Categoria.Codigo);
        cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
        cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
        cmd.Parameters.AddWithValue("@status", tarefa.Status);
        cmd.Parameters.AddWithValue("@inicio", tarefa.DataHoraInicio.HasValue ? (object) tarefa.DataHoraInicio : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", tarefa.DataHoraPrevista.HasValue ? (object) tarefa.DataHoraPrevista : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", tarefa.DataHoraFim.HasValue ? (object) tarefa.DataHoraFim : DBNull.Value);
        cmd.Parameters.AddWithValue("@visibilidade", tarefa.Visibilidade);

        bool resultado = cmd.ExecuteNonQuery() > 0;

        if (tarefa.Funcionarios != null)
        {
            // Teste: TarefaFuncionariosSqlRepository tarefaFuncionarios = new TarefaFuncionariosSqlRepository();
            AtualizarFuncionarios(tarefa.Codigo, tarefa.Funcionarios);
        }

        if (tarefa.Comentarios != null)
        {
            // Chamar o método ComentarioRepository.Create();
        }

        if (tarefa.Documentos != null)
        {
            // Chamar o método DocumentoRepository.Create();
        }

        return resultado;
    }

    public bool Delete(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "UPDATE Tarefas SET status = 'Cancelada' WHERE codigo = @codigo";

        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0; 
    }
}