using System.Data;
using Microsoft.Data.SqlClient;

// Repository das Tarefas.
// A tabela Tarefas guarda os dados principais. Equipe, comentários e documentos
// ficam em tabelas relacionadas e são carregados quando necessário.
public class TarefaSqlRepository : DataConnection
{
    // Cria uma tarefa e, depois de obter o código gerado pelo banco,
    // grava os funcionários selecionados na tabela TarefaFuncionarios.
    public bool Create(Tarefa tarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            INSERT INTO Tarefas
                (projeto_codigo, categoria_codigo, titulo, descricao, status,
                 datahora_inicio, datahora_fim, datahora_prevista, visibilidade)
            VALUES
                (@projeto, @categoria, @titulo, @descricao, @status,
                 @inicio, @fim, @prevista, @visibilidade);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
        ";

        cmd.Parameters.AddWithValue("@projeto", tarefa.IdProjeto);
        cmd.Parameters.AddWithValue("@categoria", tarefa.Categoria.Codigo);
        cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
        cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
        cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(tarefa.Status) ? "A fazer" : tarefa.Status);
        cmd.Parameters.AddWithValue("@inicio", tarefa.DataHoraInicio.HasValue ? (object)tarefa.DataHoraInicio.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", tarefa.DataHoraFim.HasValue ? (object)tarefa.DataHoraFim.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", tarefa.DataHoraPrevista.HasValue ? (object)tarefa.DataHoraPrevista.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@visibilidade", tarefa.Visibilidade);

        int idTarefa = (int)cmd.ExecuteScalar();

        // Atualiza a equipe somente quando a tarefa foi realmente criada.
        if (idTarefa > 0 && tarefa.Funcionarios != null)
        {
            FuncionarioSqlRepository funcionarioRepository = new FuncionarioSqlRepository();
            funcionarioRepository.AtualizarFuncionarios(idTarefa, tarefa.Funcionarios);
            funcionarioRepository.Dispose();
        }

        return idTarefa > 0;
    }

    // Busca uma tarefa pelo código, já trazendo categoria, funcionários,
    // comentários e documentos para a tela detalhada.
    public Tarefa? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        // Consulta diretamente as tabelas em vez de depender da View V_Tarefas.
        // Assim a abertura do Projeto/Tarefa continua funcionando mesmo se a View
        // ainda não tiver sido atualizada no banco local do desenvolvedor.
        cmd.CommandText = @"
            SELECT A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
                   A.datahora_fim, A.visibilidade, A.status,
                   A.projeto_codigo, A.categoria_codigo,
                   B.descricao AS categoria_descricao, B.cor AS categoria_cor
            FROM Tarefas A
            INNER JOIN Categorias B ON B.codigo = A.categoria_codigo
            WHERE A.codigo = @codigo
              AND A.status <> 'Cancelada'
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        Tarefa? tarefa = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            tarefa = MontarTarefa(reader);
        }

        reader.Close();

        // Os outros repositories usam conexões próprias. Todos são fechados
        // logo após a leitura para evitar conexões abertas sem necessidade.
        if (tarefa != null)
        {
            FuncionarioSqlRepository funcionarioRepository = new FuncionarioSqlRepository();
            tarefa.Funcionarios = funcionarioRepository.FuncionariosTarefa(id);
            funcionarioRepository.Dispose();

            ComentarioSqlRepository comentarioRepository = new ComentarioSqlRepository();
            tarefa.Comentarios = comentarioRepository.ComentariosTarefa(id);
            comentarioRepository.Dispose();

            DocumentoSqlRepository documentoRepository = new DocumentoSqlRepository();
            tarefa.Documentos = documentoRepository.DocumentosTarefa(id);
            documentoRepository.Dispose();
        }

        return tarefa;
    }

    // Lista todas as tarefas de um projeto.
    // Essa leitura é mais leve que Read(): não carrega comentários e documentos
    // de cada card do Kanban.
    public List<Tarefa> ReadAll(int idProjeto)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        // A consulta é feita diretamente nas tabelas pelo mesmo motivo do Read():
        // não queremos que a tela de Projeto dependa da existência/versão de V_Tarefas.
        cmd.CommandText = @"
            SELECT A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
                   A.datahora_fim, A.visibilidade, A.status,
                   A.projeto_codigo, A.categoria_codigo,
                   B.descricao AS categoria_descricao, B.cor AS categoria_cor
            FROM Tarefas A
            INNER JOIN Categorias B ON B.codigo = A.categoria_codigo
            WHERE A.projeto_codigo = @projeto
              AND A.status <> 'Cancelada'
            ORDER BY A.codigo DESC
        ";
        cmd.Parameters.AddWithValue("@projeto", idProjeto);

        List<Tarefa> tarefas = new List<Tarefa>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            tarefas.Add(MontarTarefa(reader));
        }

        reader.Close();
        return tarefas;
    }

    // Atualiza os dados principais da tarefa e sincroniza sua equipe.
    // projeto_codigo não é alterado: uma tarefa permanece no projeto onde nasceu.
    public bool Update(Tarefa tarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            UPDATE Tarefas
            SET categoria_codigo = @categoria,
                titulo = @titulo,
                descricao = @descricao,
                status = @status,
                datahora_inicio = @inicio,
                datahora_fim = @fim,
                datahora_prevista = @prevista,
                visibilidade = @visibilidade
            WHERE codigo = @codigo
              AND status <> 'Cancelada'
        ";

        cmd.Parameters.AddWithValue("@codigo", tarefa.Codigo);
        cmd.Parameters.AddWithValue("@categoria", tarefa.Categoria.Codigo);
        cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
        cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
        cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(tarefa.Status) ? "A fazer" : tarefa.Status);
        cmd.Parameters.AddWithValue("@inicio", tarefa.DataHoraInicio.HasValue ? (object)tarefa.DataHoraInicio.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", tarefa.DataHoraFim.HasValue ? (object)tarefa.DataHoraFim.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", tarefa.DataHoraPrevista.HasValue ? (object)tarefa.DataHoraPrevista.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@visibilidade", tarefa.Visibilidade);

        bool resultado = cmd.ExecuteNonQuery() > 0;

        // Corrige o comportamento anterior: a equipe só é alterada se a tarefa
        // realmente existir e tiver sido atualizada.
        if (resultado && tarefa.Funcionarios != null)
        {
            FuncionarioSqlRepository funcionarioRepository = new FuncionarioSqlRepository();
            funcionarioRepository.AtualizarFuncionarios(tarefa.Codigo, tarefa.Funcionarios);
            funcionarioRepository.Dispose();
        }

        return resultado;
    }

    // Exclusão lógica da tarefa.
    // Mantemos a exclusão lógica usando o status "Cancelada".
    public bool Delete(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            UPDATE Tarefas
            SET status = 'Cancelada'
            WHERE codigo = @codigo
              AND status <> 'Cancelada'
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Monta a parte principal da Tarefa a partir do SELECT com Tarefas + Categorias.
    private Tarefa MontarTarefa(SqlDataReader reader)
    {
        Tarefa tarefa = new Tarefa();
        tarefa.Codigo = reader.GetInt32("codigo");
        tarefa.Titulo = reader.GetString("titulo");
        tarefa.Descricao = reader.GetString("descricao");
        tarefa.DataHoraInicio = reader.IsDBNull("datahora_inicio") ? null : reader.GetDateTime("datahora_inicio");
        tarefa.DataHoraPrevista = reader.IsDBNull("datahora_prevista") ? null : reader.GetDateTime("datahora_prevista");
        tarefa.DataHoraFim = reader.IsDBNull("datahora_fim") ? null : reader.GetDateTime("datahora_fim");
        tarefa.Visibilidade = reader.GetBoolean("visibilidade");
        tarefa.Status = reader.GetString("status");
        tarefa.IdProjeto = reader.GetInt32("projeto_codigo");

        tarefa.Categoria = new Categoria
        {
            Codigo = reader.GetInt32("categoria_codigo"),
            Descricao = reader.GetString("categoria_descricao"),
            Cor = reader.IsDBNull("categoria_cor") ? null : reader.GetString("categoria_cor")
        };

        // As listas são inicializadas para a View poder percorrê-las com segurança.
        tarefa.Funcionarios = new List<Funcionario>();
        tarefa.Comentarios = new List<Comentario>();
        tarefa.Documentos = new List<Documento>();

        return tarefa;
    }
}
