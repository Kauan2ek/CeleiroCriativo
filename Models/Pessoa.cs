// Classe base para Cliente e Funcionário.
public abstract class Pessoa
{
    // Código é gerado na tabela Pessoas e compartilhado com o subtipo.
    public int Codigo { get; set; }

    public string Nome { get; set; } = "";
    public string? Telefone { get; set; }
    public string TipoDocumento { get; set; } = "cpf";
    public string Documento { get; set; } = "";
}
