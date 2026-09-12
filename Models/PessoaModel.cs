namespace APICeleiroCriativo.Models;

public abstract class PessoaModel
{
    public int Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
}
