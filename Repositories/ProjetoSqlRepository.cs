using System.Data;
using Microsoft.Data.SqlClient;

// Repository de Projeto.
// Concentra o SQL relacionado à tabela Projetos e monta também Cliente e Status.
public class ProjetoSqlRepository : DataConnection
{
    // Lista todos os projetos visíveis para o Gestor.
    // Projetos com status "Excluído" continuam no banco, mas não aparecem na aplicação.
    public List<Projeto> ReadAll()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT PR.codigo, PR.titulo, PR.descricao,
                   PR.data_inicio, PR.data_fim, PR.data_prevista,
                   S.codigo AS status_codigo, S.descricao AS status_descricao,
                   P.codigo AS cliente_codigo, P.nome AS cliente_nome,
                   P.telefone AS cliente_telefone, P.documento AS cliente_documento,
                   P.tipo_doc AS cliente_tipo_doc,
                   C.email AS cliente_email, C.codigo_verificacao
            FROM Projetos PR
            INNER JOIN Clientes C ON C.pessoa_codigo = PR.cliente_codigo
            INNER JOIN Pessoas P ON P.codigo = C.pessoa_codigo
            LEFT JOIN Status S ON S.codigo = PR.status_codigo
            WHERE ISNULL(S.descricao, '') <> 'Excluído'
            ORDER BY PR.codigo DESC
        ";

        List<Projeto> projetos = new List<Projeto>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            projetos.Add(MontarProjeto(reader));
        }

        reader.Close();
        return projetos;
    }

    // Busca um projeto pelo código.
    // Além dos dados do Projeto, também carrega suas tarefas para a tela de detalhes.
    public Projeto? Read(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT PR.codigo, PR.titulo, PR.descricao,
                   PR.data_inicio, PR.data_fim, PR.data_prevista,
                   S.codigo AS status_codigo, S.descricao AS status_descricao,
                   P.codigo AS cliente_codigo, P.nome AS cliente_nome,
                   P.telefone AS cliente_telefone, P.documento AS cliente_documento,
                   P.tipo_doc AS cliente_tipo_doc,
                   C.email AS cliente_email, C.codigo_verificacao
            FROM Projetos PR
            INNER JOIN Clientes C ON C.pessoa_codigo = PR.cliente_codigo
            INNER JOIN Pessoas P ON P.codigo = C.pessoa_codigo
            LEFT JOIN Status S ON S.codigo = PR.status_codigo
            WHERE PR.codigo = @codigo
              AND ISNULL(S.descricao, '') <> 'Excluído'
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        Projeto? projeto = null;
        SqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            projeto = MontarProjeto(reader);
        }

        reader.Close();

        // As tarefas são carregadas somente depois que o reader desta conexão foi fechado.
        if (projeto != null)
        {
            TarefaSqlRepository tarefaRepository = new TarefaSqlRepository();
            projeto.tarefas = tarefaRepository.ReadAll(projeto.Codigo);
            tarefaRepository.Dispose();
        }

        return projeto;
    }

    // Cria um projeto e devolve o código gerado.
    public int Create(Projeto projeto)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            INSERT INTO Projetos
                (cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo)
            VALUES
                (@cliente, @titulo, @descricao, @inicio, @fim, @prevista, @status);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
        ";

        PreencherParametrosProjeto(cmd, projeto);

        return (int)cmd.ExecuteScalar();
    }

    // Atualiza um Projeto existente.
    // O código do Projeto não muda; Cliente e Status podem ser alterados pelo Gestor.
    public bool Update(Projeto projeto)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            UPDATE Projetos
            SET cliente_codigo = @cliente,
                titulo = @titulo,
                descricao = @descricao,
                data_inicio = @inicio,
                data_fim = @fim,
                data_prevista = @prevista,
                status_codigo = @status
            WHERE codigo = @codigo
        ";

        cmd.Parameters.AddWithValue("@codigo", projeto.Codigo);
        PreencherParametrosProjeto(cmd, projeto);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Aprova um projeto em análise.
    // O código do status é localizado pela descrição para não depender de um número fixo.
    public bool Aprovar(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            UPDATE Projetos
            SET status_codigo = (SELECT TOP 1 codigo FROM Status WHERE descricao = 'Em andamento')
            WHERE codigo = @codigo
              AND status_codigo = (SELECT TOP 1 codigo FROM Status WHERE descricao = 'Em análise')
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Exclusão lógica de Projeto.
    // A linha continua no banco e apenas recebe o status "Excluído".
    public bool Delete(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            UPDATE Projetos
            SET status_codigo = (SELECT TOP 1 codigo FROM Status WHERE descricao = 'Excluído')
            WHERE codigo = @codigo
              AND EXISTS (SELECT 1 FROM Status WHERE descricao = 'Excluído')
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Ao NEGAR uma solicitação que ainda está "Em análise", o usuário pediu
    // para remover o Projeto por enquanto. Como projetos em análise não deveriam
    // ter tarefas, o DELETE físico é permitido apenas quando não há tarefas ligadas.
    public bool Negar(int id)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            DELETE FROM Projetos
            WHERE codigo = @codigo
              AND status_codigo = (SELECT TOP 1 codigo FROM Status WHERE descricao = 'Em análise')
              AND NOT EXISTS (
                    SELECT 1
                    FROM Tarefas
                    WHERE projeto_codigo = @codigo
              )
        ";
        cmd.Parameters.AddWithValue("@codigo", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Preenche os parâmetros repetidos por Create e Update.
    private void PreencherParametrosProjeto(SqlCommand cmd, Projeto projeto)
    {
        cmd.Parameters.AddWithValue("@cliente", projeto.Cliente.Codigo);
        cmd.Parameters.AddWithValue("@titulo", projeto.Titulo);
        cmd.Parameters.AddWithValue("@descricao", string.IsNullOrWhiteSpace(projeto.Descricao) ? DBNull.Value : projeto.Descricao);
        cmd.Parameters.AddWithValue("@inicio", projeto.DataInicio.HasValue ? (object)projeto.DataInicio.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@fim", projeto.DataFim.HasValue ? (object)projeto.DataFim.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@prevista", projeto.DataPrevista.HasValue ? (object)projeto.DataPrevista.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@status", projeto.Status.Codigo > 0 ? (object)projeto.Status.Codigo : DBNull.Value);
    }

    // Converte uma linha do SELECT em um objeto Projeto completo o suficiente
    // para as telas do Gestor.
    private Projeto MontarProjeto(SqlDataReader reader)
    {
        Projeto projeto = new Projeto();
        projeto.Codigo = reader.GetInt32("codigo");
        projeto.Titulo = reader.GetString("titulo");
        projeto.Descricao = reader.IsDBNull("descricao") ? "" : reader.GetString("descricao");
        projeto.DataInicio = reader.IsDBNull("data_inicio") ? null : reader.GetDateTime("data_inicio");
        projeto.DataFim = reader.IsDBNull("data_fim") ? null : reader.GetDateTime("data_fim");
        projeto.DataPrevista = reader.IsDBNull("data_prevista") ? null : reader.GetDateTime("data_prevista");

        projeto.Status = new Status
        {
            Codigo = reader.IsDBNull("status_codigo") ? 0 : reader.GetInt32("status_codigo"),
            Descricao = reader.IsDBNull("status_descricao") ? "Sem status" : reader.GetString("status_descricao")
        };

        projeto.Cliente = new Cliente
        {
            Codigo = reader.GetInt32("cliente_codigo"),
            Nome = reader.GetString("cliente_nome"),
            Telefone = reader.IsDBNull("cliente_telefone") ? null : reader.GetString("cliente_telefone"),
            Documento = reader.GetString("cliente_documento"),
            TipoDocumento = reader.GetString("cliente_tipo_doc"),
            Email = reader.GetString("cliente_email"),
            CodigoVerificacao = reader.IsDBNull("codigo_verificacao") ? null : reader.GetString("codigo_verificacao")
        };

        return projeto;
    }
}
