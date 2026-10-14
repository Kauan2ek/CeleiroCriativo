// o System.Data traz métodos extras pro reader (GetInt32, GetString, GetDateTime...)
// que recebem o NOME da coluna em vez do índice dela
// por baixo ele procura a posição da coluna pelo nome e lê o valor dessa posição
// aqui é usado no ListarFuncionario(), pra ler cada coluna sem depender da ordem delas na tabela
// o nome passado tem que ser o nome da coluna no banco (ou no SELECT), não o da propriedade da model
using System.Data;
using Microsoft.Data.SqlClient;

// repositório dos funcionários: por enquanto só tem a parte da equipe de uma tarefa
// (listar quem está alocado e atualizar a tabela TarefaFuncionarios)
// herda de DataConnection, que abre a conexão no construtor e deixa ela disponível em "conn"
// quem cria o repositório é quem tem que chamar o Dispose() pra fechar a conexão
public class FuncionarioSqlRepository: DataConnection
{
    // lista os funcionários que estão alocados numa tarefa (o id recebido é o código da TAREFA)
    // cada Funcionario volta completo: dados de pessoa, email, senha, ativo e o cargo
    // a consulta usa a function F_FuncionariosDaTarefa (Database/Scripts/Functions.sql),
    // que já junta TarefaFuncionarios, Funcionarios, Pessoas e Cargos e recebe o código da tarefa como parâmetro
    // atenção: a senha vem junto, e como o Read da tarefa devolve esses funcionários em JSON, ela aparece na resposta
    public List<Funcionario> ListarFuncionario(int id) 
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
            // cada coluna é lida pelo nome que a function devolve (ex: tipo_documento, cargo_descricao)
            // o telefone pode ser nulo no banco, por isso o IsDBNull antes
            // o cargo é um objeto dentro do funcionário, montado com as colunas cargo_*
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
        // o ListarFuncionario traz o funcionário completo, mas aqui só o código interessa
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