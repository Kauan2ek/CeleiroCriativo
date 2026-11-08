using System.Data;
using Microsoft.Data.SqlClient;

public class UsuarioSqlRepository : DataConnection
{
    public Usuario? Entrar(Usuario usuario)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT IIF(B.pessoa_codigo IS NOT NULL, 'cliente', 'funcionario') tipo_usuario,
                   A.*, 
                   COALESCE(B.email, C.email) email, COALESCE(B.senha, C.senha) senha, -- contém em ambos
                   B.codigo_verificacao, 
                   C.ativo,
                   D.codigo cargo_codigo, D.descricao cargo_descricao
            FROM Pessoas A
            LEFT JOIN Clientes B     ON A.codigo = B.pessoa_codigo
            LEFT JOIN Funcionarios C ON A.codigo = C.pessoa_codigo
            LEFT JOIN Cargos D       ON C.cargo_codigo = D.codigo
            WHERE (B.email = @email OR C.email = @email) AND (B.senha = @senha OR C.senha = @senha)
              AND (C.ativo IS NULL OR C.ativo = 1)
        "; // criar uma VIEW Usuarios
        cmd.Parameters.AddWithValue("email", usuario.Email);
        cmd.Parameters.AddWithValue("senha", usuario.Senha);

        SqlDataReader reader = cmd.ExecuteReader();
        Usuario? usuarioRetorno = null;

        if (reader.Read())
        {
            usuarioRetorno = new Usuario();
            // Dados em comum a ambos
            usuarioRetorno.Codigo = reader.GetInt32("Codigo");
            usuarioRetorno.Nome = reader.GetString("nome");
            usuarioRetorno.Email = reader.GetString("Email");
            usuarioRetorno.Senha = reader.GetString("Senha");

            // Essa ideia está obsoleta por hora (pendente)
            if (reader.GetString("tipo_usuario") == "cliente")
            {
                // Pessoa
                usuarioRetorno.Cliente?.Codigo = reader.GetInt32("codigo");
                usuarioRetorno.Cliente?.Nome = reader.GetString("nome");
                usuarioRetorno.Cliente?.Telefone = reader.GetString("telefone");
                usuarioRetorno.Cliente?.TipoDocumento = reader.GetString("tipo_doc");
                usuarioRetorno.Cliente?.Documento = reader.GetString("documento");
                // Cliente
                usuarioRetorno.Papel = reader.GetString("tipo_usuario");
                usuarioRetorno.Cliente?.CodigoVerificacao = reader.GetString("codigo_verificacao");
                usuarioRetorno.Cliente?.Email = reader.GetString("email");
                usuarioRetorno.Cliente?.Senha = reader.GetString("senha");
            } else {
                // Pessoa
                usuarioRetorno.Funcionario?.Codigo = reader.GetInt32("codigo");
                usuarioRetorno.Funcionario?.Nome = reader.GetString("nome");
                usuarioRetorno.Funcionario?.Telefone = reader.GetString("telefone");
                usuarioRetorno.Funcionario?.TipoDocumento = reader.GetString("nome");
                usuarioRetorno.Funcionario?.Documento = reader.GetString("nome");
                // Funcionário
                usuarioRetorno.Papel = reader.GetString("cargo_descricao").ToLower() == "gestor" ? "gestor" : "funcionario";
                usuarioRetorno.Funcionario?.Ativo = reader.GetBoolean("codigo_verificacao");
                usuarioRetorno.Funcionario?.Email = reader.GetString("email");
                usuarioRetorno.Funcionario?.Senha = reader.GetString("senha");
                usuarioRetorno.Funcionario?.Cargo.Codigo = reader.GetInt32("cargo_codigo");
                usuarioRetorno.Funcionario?.Cargo.Descricao = reader.GetString("cargo_descricao");
            }
        } 
        reader.Close();
        return usuarioRetorno; 
    }

    public void GerarCodigo(string email)
    {
        Random rnd = new Random();
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"UPDATE Clientes SET codigo_verificacao = @codigoVerificacao WHERE email = @email";
        cmd.Parameters.AddWithValue("codigoVerificacao", rnd.Next(100000, 1000000));
        cmd.Parameters.AddWithValue("email", email);

        cmd.ExecuteNonQuery();
    }

    public bool ValidarCodigo (string codigo, string email)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"SELECT COUNT(*)
            FROM (SELECT codigo_verificacao, email FROM Clientes) C
            WHERE C.codigo_verificacao = @codigoVerificacao AND C.email = @email";
        cmd.Parameters.AddWithValue("codigoVerificacao", codigo);
        cmd.Parameters.AddWithValue("email", email);

        if ((int) cmd.ExecuteScalar() > 0)
        {
            return true;
        }
        return false;
    }

    public bool RedefinirSenha (string email, string senha)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"UPDATE Clientes SET senha = @senha, codigo_verificacao = NULL WHERE email = @email";
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("senha", senha);

        if (cmd.ExecuteNonQuery() > 0)
        {
            return true;
        }
        return false;
    }

    public bool EmailExiste(string email)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            SELECT COUNT(*)
            FROM Clientes
            WHERE email = @email
        ";
        cmd.Parameters.AddWithValue("@email", email);

        return (int) cmd.ExecuteScalar() > 0;
    }
}