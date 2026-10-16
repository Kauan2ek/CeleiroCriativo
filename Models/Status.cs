// Representa um status de projeto cadastrado na tabela Status.
public class Status
{
    // Código gerado pelo banco de dados.
    public int Codigo { get; set; }

    // Texto exibido para o usuário, por exemplo: "Em análise".
    public string Descricao { get; set; } = "";

    // Construtor vazio necessário para o ASP.NET conseguir montar o objeto
    // durante o model binding dos formulários.
    public Status()
    {
    }

    // Mantém o construtor que já existia no projeto para os pontos em que
    // seja mais prático criar um Status informando código e descrição.
    public Status(int codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
