using System.Data;
using Microsoft.Data.SqlClient;

// Repository responsável somente por buscar os comentários ligados a uma tarefa.
// Comentário não possui uma tela própria no fluxo atual do Gestor: ele é carregado
// dentro da própria Tarefa e exibido junto com os demais dados dela.
public class ComentarioSqlRepository : DataConnection
{
    // Retorna todos os comentários pertencentes à tarefa informada.
    //
    // Antes este método dependia da View SQL V_Comentarios. Isso fazia a abertura
    // da tarefa falhar quando a View ainda não tinha sido criada no banco local.
    // Como todas as informações necessárias já existem nas tabelas, fazemos os
    // JOINs diretamente aqui, mantendo o mesmo padrão usado nos outros repositories.
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
