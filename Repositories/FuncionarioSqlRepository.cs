// o System.Data traz métodos extras pro reader (GetInt32, GetString, GetDateTime...)
// que recebem o NOME da coluna em vez do índice dela
// por baixo ele procura a posição da coluna pelo nome e lê o valor dessa posição
// aqui é usado no Listar(), pra ler cada coluna sem depender da ordem delas na tabela
// o nome passado tem que ser o nome da coluna no banco (ou no SELECT), não o da propriedade da model
using System.Data;
using Microsoft.Data.SqlClient;

// repositório das tarefas: é a única classe que conversa com o banco sobre tarefa
// herda de DataConnection, que abre a conexão no construtor e deixa ela disponível em "conn"
// quem usa o repositório (o controller) é quem chama o Dispose() pra fechar a conexão
public class FuncionarioSqlRepository: DataConnection
{
    // lista os funcionários que estão alocados numa tarefa
    // cada Funcionario volta só com o Codigo preenchido (nome, email e cargo ficam vazios),
    // porque a consulta é feita só na tabela associativa TarefaFuncionarios
    public List<Funcionario> ListarFuncionario(int id) // Isso daqui vai pra FuncionarioSqlRepository (pendente)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT * FROM F_FuncionariosDaTarefa (@tarefa)";

        cmd.Parameters.AddWithValue("tarefa", id);

        // ExecuteReader é pra comandos que devolvem linhas (SELECT)
        SqlDataReader reader = cmd.ExecuteReader();

        // cada volta do while é uma linha do resultado
        // o Read() anda pra próxima linha e devolve false quando acabam
        List<Funcionario> funcionarios = new List<Funcionario>();
        while (reader.Read())
        {
            Funcionario funcionario = new Funcionario();
            // o select só traz uma coluna, então ela está na posição 0
            funcionario.Codigo        = reader.GetInt32("codigo");
            funcionario.Nome          = reader.GetString("nome");
            funcionario.Email         = reader.GetString("email");
            funcionario.Senha         = reader.GetString("senha");
            funcionario.Telefone      = reader.IsDBNull("telefone") ? null : reader.GetString("telefone");
            funcionario.TipoDocumento = reader.GetString("tipo_documento");
            funcionario.Documento     = reader.GetString("documento");
            funcionario.Ativo         = reader.GetBoolean("ativo");
            funcionario.Cargo         = new Cargo {
                Codigo    = reader.GetInt32("cargo_codigo"),
                Descricao = reader.GetString("cargo_descricao"),
            };

            funcionarios.Add(funcionario);
        }

        // a conexão só aceita um reader aberto por vez, tem que fechar antes de rodar outro comando
        reader.Close();
        return funcionarios;
    }

    // deixa a equipe da tarefa igual à lista recebida
    // compara quem já está no banco com quem veio na lista nova:
    // quem é novo é inserido, quem saiu da lista é removido, e quem continua não é tocado
    public void AtualizarFuncionarios (int idTarefa, List<Funcionario> funcionarios)
    {
        // códigos de quem está na tarefa hoje, no banco
        List<int> ListaAtual = new List<int>();
        foreach (var funcionario in ListarFuncionario(idTarefa))
        {
            ListaAtual.Add(funcionario.Codigo);
        }

        // códigos de quem deve ficar na tarefa, vindos do formulário
        List<int> ListaNova = new List<int>();
        foreach (var funcionario in funcionarios)
        {
            ListaNova.Add(funcionario.Codigo);
        }

        // Except devolve o que está na primeira lista e não está na segunda (e já tira os repetidos)
        // adicionar = está na nova mas não na atual
        // remover = está na atual mas não na nova
        List<int> adicionar = ListaNova.Except(ListaAtual).ToList();
        List<int> remover = ListaAtual.Except(ListaNova).ToList();

        // um insert por funcionário novo, cada um vira uma linha em TarefaFuncionarios
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

            // ExecuteNonQuery é pra comandos que não devolvem linhas (INSERT, UPDATE, DELETE)
            cmd.ExecuteNonQuery();
        }

        // pra cada funcionário que saiu: tira o vínculo e os comentários dele nessa tarefa
        // atenção: a tabela Comentarios tem FK pra TarefaFuncionarios, então o banco só deixa
        // apagar o vínculo depois que os comentários já saíram. Do jeito que está (vínculo primeiro),
        // o primeiro delete dá erro de FK quando o funcionário tem comentário na tarefa
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
}