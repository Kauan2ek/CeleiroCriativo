namespace APICeleiroCriativo.Models;

public class ClienteModel : PessoaModel
{
    public string CodigoVerificacao { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;

    public List<ProjetoModel> Projetos { get; set; } = new();
}
