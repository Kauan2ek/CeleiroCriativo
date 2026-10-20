using System.Data;
using Microsoft.Data.SqlClient;

// Repository de Funcionário.
// Além do CRUD do funcionário, esta classe mantém as funções de associação
// entre Funcionário e Tarefa que já existiam no projeto.
public class FuncionarioSqlRepository : DataConnection
{
    // Lista funcionários. Por padrão mostra apenas os ativos, pois a exclusão
    // é lógica e usa a coluna Funcionarios.ativo.
    // Na tela da tarefa, é essa lista que aparece no modal de equipe (ViewBag.Funcionarios).
    public List<Funcionario> ReadAll(bool incluirInativos = false)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT P.codigo, P.nome, P.telefone, P.documento, P.tipo_doc,
                   F.email, F.ativo,
                   C.codigo AS cargo_codigo, C.descricao AS cargo_descricao
            FROM Funcionarios F
            INNER JOIN Pessoas P ON P.codigo = F.pessoa_codigo
            INNER JOIN Cargos C ON C.codigo = F.cargo_codigo
            WHERE (@incluirInativos = 1 OR F.ativo = 1)
            ORDER BY P.nome
        ";
        cmd.Parameters.AddWithValue("@incluirInativos", incluirInativos);

        List<Funcionario> funcionarios = new List<Funcionario>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            funcionarios.Add(MontarFuncionario(reader));
        }

        reader.Close();
        return funcionarios;
    }

    // Busca um funcionário pelo código.
    public Funcionario? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT P.codigo, P.nome, P.telefone, P.documento, P.tipo_doc,
                   F.email, F.ativo,
                   C.codigo AS cargo_codigo, C.descricao AS cargo_descricao
            FROM Funcionarios F
            INNER JOIN Pessoas P ON P.codigo = F.pessoa_codigo
            INNER JOIN Cargos C ON C.codigo = F.cargo_codigo
            WHERE P.codigo = @codigo
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        Funcionario? funcionario = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            funcionario = MontarFuncionario(reader);
        }

        reader.Close();
        return funcionario;
    }

    // Cria Pessoa e Funcionário na mesma transação.
    // Se o segundo INSERT falhar, o primeiro também é desfeito.
    public int Create(Funcionario funcionario)
    {
        SqlTransaction transaction = conn.BeginTransaction();

        try
        {
            SqlCommand cmdPessoa = new SqlCommand();
            cmdPessoa.Connection = conn;
            cmdPessoa.Transaction = transaction;
            cmdPessoa.CommandText = @"
                INSERT INTO Pessoas (nome, telefone, documento, tipo_doc)
                VALUES (@nome, @telefone, @documento, @tipoDocumento);

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";
            cmdPessoa.Parameters.AddWithValue("@nome", funcionario.Nome);
            cmdPessoa.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(funcionario.Telefone) ? DBNull.Value : funcionario.Telefone);
            cmdPessoa.Parameters.AddWithValue("@documento", funcionario.Documento);
            cmdPessoa.Parameters.AddWithValue("@tipoDocumento", funcionario.TipoDocumento);

            int codigoPessoa = (int)cmdPessoa.ExecuteScalar();

            SqlCommand cmdFuncionario = new SqlCommand();
            cmdFuncionario.Connection = conn;
            cmdFuncionario.Transaction = transaction;
            cmdFuncionario.CommandText = @"
                INSERT INTO Funcionarios (pessoa_codigo, cargo_codigo, ativo, email, senha)
                VALUES (@pessoa, @cargo, 1, @email, @senha)
            ";
            cmdFuncionario.Parameters.AddWithValue("@pessoa", codigoPessoa);
            cmdFuncionario.Parameters.AddWithValue("@cargo", funcionario.Cargo.Codigo);
            cmdFuncionario.Parameters.AddWithValue("@email", funcionario.Email);
            cmdFuncionario.Parameters.AddWithValue("@senha", funcionario.Senha);
            cmdFuncionario.ExecuteNonQuery();

            transaction.Commit();
            return codigoPessoa;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Atualiza os dados de Pessoa e Funcionário.
    // Quando a senha vem vazia na edição, a senha atual é mantida.
    public bool Update(Funcionario funcionario)
    {
        SqlTransaction transaction = conn.BeginTransaction();

        try
        {
            SqlCommand cmdPessoa = new SqlCommand();
            cmdPessoa.Connection = conn;
            cmdPessoa.Transaction = transaction;
            cmdPessoa.CommandText = @"
                UPDATE Pessoas
                SET nome = @nome,
                    telefone = @telefone,
                    documento = @documento,
                    tipo_doc = @tipoDocumento
                WHERE codigo = @codigo
            ";
            cmdPessoa.Parameters.AddWithValue("@codigo", funcionario.Codigo);
            cmdPessoa.Parameters.AddWithValue("@nome", funcionario.Nome);
            cmdPessoa.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(funcionario.Telefone) ? DBNull.Value : funcionario.Telefone);
            cmdPessoa.Parameters.AddWithValue("@documento", funcionario.Documento);
            cmdPessoa.Parameters.AddWithValue("@tipoDocumento", funcionario.TipoDocumento);

            bool pessoaAtualizada = cmdPessoa.ExecuteNonQuery() > 0;

            SqlCommand cmdFuncionario = new SqlCommand();
            cmdFuncionario.Connection = conn;
            cmdFuncionario.Transaction = transaction;

            // A senha só entra no SET quando uma nova senha foi informada.
            if (string.IsNullOrWhiteSpace(funcionario.Senha))
            {
                cmdFuncionario.CommandText = @"
                    UPDATE Funcionarios
                    SET cargo_codigo = @cargo,
                        email = @email
                    WHERE pessoa_codigo = @codigo
                ";
            }
            else
            {
                cmdFuncionario.CommandText = @"
                    UPDATE Funcionarios
                    SET cargo_codigo = @cargo,
                        email = @email,
                        senha = @senha
                    WHERE pessoa_codigo = @codigo
                ";
                cmdFuncionario.Parameters.AddWithValue("@senha", funcionario.Senha);
            }

            cmdFuncionario.Parameters.AddWithValue("@codigo", funcionario.Codigo);
            cmdFuncionario.Parameters.AddWithValue("@cargo", funcionario.Cargo.Codigo);
            cmdFuncionario.Parameters.AddWithValue("@email", funcionario.Email);

            bool funcionarioAtualizado = cmdFuncionario.ExecuteNonQuery() > 0;

            // Se uma das duas tabelas não encontrou o registro, não deixamos
            // uma atualização pela metade.
            if (!pessoaAtualizada || !funcionarioAtualizado)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Exclusão lógica: mantém o histórico e os vínculos com tarefas,
    // mas o funcionário deixa de aparecer nas listagens e novas atribuições.
    public bool Delete(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "UPDATE Funcionarios SET ativo = 0 WHERE pessoa_codigo = @codigo";
        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Lista os funcionários alocados em uma tarefa (a equipe), em ordem alfabética.
    // Usado pelo TarefaSqlRepository.Read pra mostrar a equipe na tela da tarefa,
    // e pelo AtualizarFuncionarios pra saber quem já está na equipe.
    // Atenção: enquanto não existe login, a tela usa o primeiro dessa lista como autor dos comentários,
    // então o "primeiro" é o primeiro em ordem alfabética.
    public List<Funcionario> FuncionariosTarefa(int idTarefa)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT P.codigo, P.nome, P.telefone, P.documento, P.tipo_doc,
                   F.email, F.ativo,
                   C.codigo AS cargo_codigo, C.descricao AS cargo_descricao
            FROM TarefaFuncionarios TF
            INNER JOIN Funcionarios F ON F.pessoa_codigo = TF.funcionario_codigo
            INNER JOIN Pessoas P ON P.codigo = F.pessoa_codigo
            INNER JOIN Cargos C ON C.codigo = F.cargo_codigo
            WHERE TF.tarefa_codigo = @tarefa
            ORDER BY P.nome
        ";
        cmd.Parameters.AddWithValue("@tarefa", idTarefa);

        List<Funcionario> funcionarios = new List<Funcionario>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            funcionarios.Add(MontarFuncionario(reader));
        }

        reader.Close();
        return funcionarios;
    }

    // Deixa a equipe da tarefa exatamente igual à lista recebida (os marcados no modal da tela).
    // Compara a lista atual do banco com a nova: quem está só na nova é inserido,
    // quem está só na atual é removido, e quem está nas duas continua como está.
    // Chamado pelo TarefaSqlRepository no Create e no Update.
    public void AtualizarFuncionarios(int idTarefa, List<Funcionario> funcionarios)
    {
        List<int> listaAtual = new List<int>();
        foreach (Funcionario funcionario in FuncionariosTarefa(idTarefa))
        {
            listaAtual.Add(funcionario.Codigo);
        }

        List<int> listaNova = new List<int>();
        foreach (Funcionario funcionario in funcionarios)
        {
            listaNova.Add(funcionario.Codigo);
        }

        List<int> adicionar = listaNova.Except(listaAtual).ToList();
        List<int> remover = listaAtual.Except(listaNova).ToList();

        // Adiciona novos funcionários à tarefa.
        foreach (int codigo in adicionar)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"
                INSERT INTO TarefaFuncionarios (funcionario_codigo, tarefa_codigo)
                VALUES (@funcionario, @tarefa)
            ";
            cmd.Parameters.AddWithValue("@funcionario", codigo);
            cmd.Parameters.AddWithValue("@tarefa", idTarefa);
            cmd.ExecuteNonQuery();
        }

        // Para remover um funcionário da equipe, primeiro apagamos os comentários
        // escritos por ele naquela tarefa: a FK_Comentarios_TarefaFuncionario liga cada comentário
        // ao par (funcionário, tarefa) da TarefaFuncionarios, então o banco não deixa apagar
        // a linha da equipe enquanto existir comentário dela.
        // Atenção: tirar alguém da equipe apaga os comentários dessa pessoa naquela tarefa.
        foreach (int codigo in remover)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"
                DELETE FROM Comentarios
                WHERE funcionario_codigo = @funcionario AND tarefa_codigo = @tarefa;

                DELETE FROM TarefaFuncionarios
                WHERE funcionario_codigo = @funcionario AND tarefa_codigo = @tarefa;
            ";
            cmd.Parameters.AddWithValue("@funcionario", codigo);
            cmd.Parameters.AddWithValue("@tarefa", idTarefa);
            cmd.ExecuteNonQuery();
        }
    }

    // Monta a Model sem trazer a senha para telas/listagens.
    private Funcionario MontarFuncionario(SqlDataReader reader)
    {
        return new Funcionario
        {
            Codigo = reader.GetInt32("codigo"),
            Nome = reader.GetString("nome"),
            Telefone = reader.IsDBNull("telefone") ? null : reader.GetString("telefone"),
            Documento = reader.GetString("documento"),
            TipoDocumento = reader.GetString("tipo_doc"),
            Email = reader.GetString("email"),
            Senha = "",
            Ativo = reader.GetBoolean("ativo"),
            Cargo = new Cargo
            {
                Codigo = reader.GetInt32("cargo_codigo"),
                Descricao = reader.GetString("cargo_descricao")
            }
        };
    }
}
