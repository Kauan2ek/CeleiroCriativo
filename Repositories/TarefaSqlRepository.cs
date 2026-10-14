// o System.Data traz métodos extras pro reader (GetInt32, GetString, GetDateTime...)
// que recebem o NOME da coluna em vez do índice dela
// por baixo ele procura a posição da coluna pelo nome e lê o valor dessa posição
// aqui é usado no Read() e no ReadAll(), pra ler cada coluna sem depender da ordem delas na tabela
// o nome passado tem que ser o nome da coluna no banco (ou no SELECT), não o da propriedade da model
using System.Data;
using Microsoft.Data.SqlClient;

// repositório das tarefas: cuida da tabela Tarefas (criar, ler, alterar e cancelar)
// a equipe da tarefa (tabela TarefaFuncionarios) fica por conta do FuncionarioSqlRepository
// herda de DataConnection, que abre a conexão no construtor e deixa ela disponível em "conn"
// quem usa o repositório (o controller) é quem chama o Dispose() pra fechar a conexão
public class TarefaSqlRepository: DataConnection
{
    // cadastra uma tarefa nova e, se vier equipe, já vincula os funcionários a ela
    // retorna true se a tarefa foi criada
    public bool Create(Tarefa tarefa)
    {
        // o comando tem duas partes: o insert da tarefa e um select que devolve o código gerado
        // o codigo não entra no insert porque é IDENTITY, o próprio banco gera
        // SCOPE_IDENTITY() devolve o último código gerado por esse comando
        // o @ antes das aspas deixa a string ocupar várias linhas
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


        // cada @nome do sql recebe o seu valor aqui
        // usar parâmetro (em vez de juntar texto) evita sql injection
        // da categoria só vai o código, porque ela já existe no banco
        cmd.Parameters.AddWithValue("@projeto", tarefa.IdProjeto);
        cmd.Parameters.AddWithValue("@categoria", tarefa.Categoria.Codigo);
        cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
        cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
        // se o status não vier preenchido, a tarefa nasce como "A Fazer"
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
        // é o resultado do SELECT SCOPE_IDENTITY() lá do comando: o código da tarefa que acabou de ser criada
        int IdTarefa = (int) cmd.ExecuteScalar();

        // a equipe não fica na tabela Tarefas, cada funcionário vira uma linha em TarefaFuncionarios
        // por isso precisa do código da tarefa, que só existe depois do insert acima
        // quem grava essas linhas é o FuncionarioSqlRepository
        // o new cria um repositório só pra essa chamada, e ele abre uma conexão própria (separada da "conn" daqui)
        // atenção: como ele não fica guardado em variável, ninguém chama o Dispose() dele e essa conexão não é fechada
        if (tarefa.Funcionarios != null)
        {
            new FuncionarioSqlRepository().AtualizarFuncionarios(IdTarefa, tarefa.Funcionarios);
        }

        // comentários e documentos ainda não são gravados aqui (ficam pros repositórios deles)
        if (tarefa.Comentarios != null)
        {
            // Seguir a mesma lógica da lista de funcionários (pendente)
        }

        if (tarefa.Documentos != null)
        {
            // Seguir a mesma lógica da lista de funcionários (pendente)
        }

        return IdTarefa > 0; // Se retornou o id da tarefa, é porque deu certo, então retorna true, senão false
    }

    

    // busca uma tarefa pelo código, já com a categoria e a equipe preenchidas
    // retorna null se não encontrar (por isso o ? em Tarefa?)
    public Tarefa? Read(int id)
    {
        // a consulta é feita na view V_Tarefas, e não direto na tabela
        // a view já traz os dados da categoria juntos e esconde as tarefas com status 'Cancelada'
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM V_Tarefas WHERE codigo = @id";
        cmd.Parameters.AddWithValue("@id", id);

        Tarefa? tarefa = null;
        SqlDataReader reader = cmd.ExecuteReader();

        // aqui é if e não while porque o código é único, vem no máximo uma linha
        if (reader.Read()) // verificar se o registro
        {
            // passa cada coluna da linha pra propriedade correspondente da model
            tarefa = new Tarefa();
            tarefa.Codigo = reader.GetInt32("codigo");
            tarefa.Titulo = reader.GetString("titulo");
            tarefa.Descricao = reader.GetString("descricao");
            // é o caminho inverso do Create: o banco devolve DBNull, e aqui vira o null do C#
            // IsDBNull verifica antes, porque o GetDateTime dá erro se a coluna estiver nula
            tarefa.DataHoraInicio = reader.IsDBNull("datahora_inicio") ? null : reader.GetDateTime("DataHora_Inicio");
            tarefa.DataHoraPrevista = reader.IsDBNull("datahora_prevista") ? null : reader.GetDateTime("DataHora_Prevista");
            tarefa.DataHoraFim = reader.IsDBNull("datahora_fim") ? null : reader.GetDateTime("DataHora_Fim");
            tarefa.Visibilidade = reader.GetBoolean("visibilidade");
            tarefa.Status = reader.GetString("status");
            // a categoria é um objeto dentro da tarefa, montado com as colunas categoria_* da view
            tarefa.Categoria = new Categoria(){
                Codigo = reader.GetInt32("categoria_codigo"),
                Descricao = reader.GetString("categoria_descricao"),
                Cor = reader.GetString("categoria_cor")
            };
            tarefa.IdProjeto = reader.GetInt32("projeto_codigo");
            // os funcionários não são buscados aqui dentro, só depois do reader.Close() lá embaixo

            //Implementar as outras classes
            // a lista de funcionários começa vazia e é trocada pela lista de verdade depois do reader.Close()
            tarefa.Funcionarios = new List<Funcionario>();
            // Exemplo: tarefa.Funcionarios = FuncionarioSqlRepository.ListarPorTarefa(tarefa.Codigo);
            // comentários e documentos ainda não são buscados, por enquanto voltam vazios
            tarefa.Comentarios = new List<Comentario>();
            tarefa.Documentos = new List<Documento>();
        }

        reader.Close();
        // busca a equipe da tarefa pelo FuncionarioSqlRepository, que devolve os funcionários completos (nome, cargo...)
        // o ?. faz a linha ser pulada quando a tarefa não foi encontrada (tarefa == null)
        // atenção: esse repositório abre uma conexão própria e ninguém chama o Dispose() dele, então ela não é fechada
        tarefa?.Funcionarios = new FuncionarioSqlRepository().ListarFuncionario(id);
        return tarefa;
    }

    // lista todas as tarefas de um projeto, com a categoria de cada uma (a equipe não vem, diferente do Read)
    // se o projeto não tiver tarefa, retorna uma lista vazia (nunca null)
    public List<Tarefa> ReadAll(int idProjeto)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM V_Tarefas WHERE projeto_codigo = @IdProjeto";
        cmd.Parameters.AddWithValue("@IdProjeto", idProjeto);

        List<Tarefa> tarefas = new List<Tarefa>();
        SqlDataReader reader = cmd.ExecuteReader();

        // aqui é while porque podem vir várias linhas, uma por tarefa
        // a leitura das colunas é igual à do Read
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
            // o IdProjeto não é preenchido aqui (fica 0), diferente do Read

            //Implementar as outras classes
            // a busca dos funcionários ainda não é feita na listagem (a linha abaixo ficou comentada)
            // por enquanto as três listas voltam vazias
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

    // altera os dados de uma tarefa que já existe e atualiza a equipe dela
    // retorna false se não existir tarefa com o código informado
    public bool Update(Tarefa tarefa)
    {
        // o WHERE garante que só a tarefa desse código é alterada
        // o projeto_codigo fica de fora: a tarefa não muda de projeto
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
        // diferente do Create, aqui o status não tem valor padrão: se vier nulo, o comando dá erro
        cmd.Parameters.AddWithValue("@status", tarefa.Status);
        // mesmo tratamento de data nula do Create
        cmd.Parameters.AddWithValue("@inicio", tarefa.DataHoraInicio.HasValue ? (object) tarefa.DataHoraInicio : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", tarefa.DataHoraPrevista.HasValue ? (object) tarefa.DataHoraPrevista : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", tarefa.DataHoraFim.HasValue ? (object) tarefa.DataHoraFim : DBNull.Value);
        cmd.Parameters.AddWithValue("@visibilidade", tarefa.Visibilidade);

        // ExecuteNonQuery devolve quantas linhas foram alteradas
        // se for 0, é porque não existe tarefa com esse código
        bool resultado = cmd.ExecuteNonQuery() > 0;

        // se a lista vier nula (nenhum funcionário enviado), a equipe fica como está
        // quem compara a lista nova com a atual e grava a diferença é o FuncionarioSqlRepository
        // atenção: a equipe é atualizada mesmo quando resultado é false (tarefa inexistente),
        // e nesse caso o insert em TarefaFuncionarios dá erro de FK
        // atenção: o repositório criado aqui abre uma conexão própria que não é fechada (igual no Create)
        if (tarefa.Funcionarios != null)
        {
            // Teste: TarefaFuncionariosSqlRepository tarefaFuncionarios = new TarefaFuncionariosSqlRepository();
            new FuncionarioSqlRepository().AtualizarFuncionarios(tarefa.Codigo, tarefa.Funcionarios);
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

    // "apaga" uma tarefa sem tirar a linha do banco (exclusão lógica)
    // só troca o status pra 'Cancelada', e como a view V_Tarefas esconde esse status,
    // a tarefa some do Read e do ReadAll, mas os comentários, documentos e a equipe dela continuam guardados
    // retorna false se não existir tarefa com o código informado
    public bool Delete(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "UPDATE Tarefas SET status = 'Cancelada' WHERE codigo = @codigo";

        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0;
    }
}
