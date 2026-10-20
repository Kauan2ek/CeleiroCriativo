using System.Data;
using Microsoft.Data.SqlClient;

// Repository dos comentários de uma tarefa: grava um comentário novo (Create) e lista os comentários da tarefa (ComentariosTarefa).
// Comentário não possui uma tela própria no fluxo atual do Gestor: ele é carregado
// dentro da própria Tarefa (TarefaSqlRepository.Read) e exibido no chat da tela da tarefa.
// Quem chama o Create é o ComentarioController.
public class ComentarioSqlRepository : DataConnection
{
    // cadastra um comentário novo numa tarefa
    // retorna true se o comentário foi gravado
    public bool Create(Comentario comentario)
    {
        // o codigo não entra no insert porque é IDENTITY, o próprio banco gera
        // a data_hora também fica de fora: a coluna tem DEFAULT GETDATE(), então o banco grava a hora atual
        // não tem titulo: a coluna saiu da tabela Comentarios no DDL v4
        // o @ antes das aspas deixa a string ocupar várias linhas
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
        insert into Comentarios
            (tarefa_codigo, funcionario_codigo, descricao)
            values
            (@tarefa, @funcionario, @descricao)
        ";

        // cada @nome do sql recebe o seu valor aqui
        // usar parâmetro (em vez de juntar texto) evita sql injection
        // do autor só vai o código, porque o funcionário já existe no banco
        // atenção: a FK FK_Comentarios_TarefaFuncionario só aceita um funcionário que esteja na equipe dessa tarefa
        cmd.Parameters.AddWithValue("@tarefa", comentario.IdTarefa);
        cmd.Parameters.AddWithValue("@funcionario", comentario.Funcionario.Codigo);
        cmd.Parameters.AddWithValue("@descricao", comentario.Descricao);

        // ExecuteNonQuery devolve quantas linhas foram inseridas
        // se for maior que 0, é porque o comentário foi gravado
        return cmd.ExecuteNonQuery() > 0;
    }

    // Retorna todos os comentários pertencentes à tarefa informada, do mais antigo pro mais novo
    // (ORDER BY data_hora), que é a ordem em que aparecem no chat.
    // Como todas as informações necessárias já existem nas tabelas, fazemos os
    // JOINs diretamente aqui, mantendo o mesmo padrão usado nos outros repositories.
    // O IdTarefa de cada comentário não é preenchido (fica 0): quem chama já sabe de qual tarefa é.
    public List<Comentario> ComentariosTarefa(int idTarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT
                CO.codigo,
                CO.descricao,
                CO.data_hora,
                CO.funcionario_codigo,
                P.nome AS funcionario_nome,
                P.telefone AS funcionario_telefone,
                F.email AS funcionario_email,
                C.codigo AS cargo_codigo,
                C.descricao AS cargo_descricao
            FROM Comentarios CO
            INNER JOIN Funcionarios F ON F.pessoa_codigo = CO.funcionario_codigo
            INNER JOIN Pessoas P ON P.codigo = F.pessoa_codigo
            INNER JOIN Cargos C ON C.codigo = F.cargo_codigo
            WHERE CO.tarefa_codigo = @tarefa
            ORDER BY CO.data_hora
        ";
        cmd.Parameters.AddWithValue("@tarefa", idTarefa);

        SqlDataReader reader = cmd.ExecuteReader();
        List<Comentario> comentarios = new List<Comentario>();

        while (reader.Read())
        {
            // Cada comentário continua pertencendo à Tarefa.
            // O objeto Funcionario é preenchido apenas para identificar o autor
            // do comentário na tela de detalhes da tarefa.
            Comentario comentario = new Comentario();
            comentario.Codigo = reader.GetInt32("codigo");
            comentario.Descricao = reader.GetString("descricao");
            comentario.DataHora = reader.GetDateTime("data_hora");

            comentario.Funcionario = new Funcionario()
            {
                Codigo = reader.GetInt32("funcionario_codigo"),
                Nome = reader.GetString("funcionario_nome"),
                Telefone = reader.IsDBNull("funcionario_telefone")
                    ? null
                    : reader.GetString("funcionario_telefone"),
                Email = reader.GetString("funcionario_email"),
                Cargo = new Cargo()
                {
                    Codigo = reader.GetInt32("cargo_codigo"),
                    Descricao = reader.GetString("cargo_descricao")
                }
            };

            comentarios.Add(comentario);
        }

        reader.Close();
        return comentarios;
    }
}
