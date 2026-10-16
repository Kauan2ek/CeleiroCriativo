// Cliente é um subtipo de Pessoa no banco de dados.
public class Cliente : Pessoa
{
    public string? CodigoVerificacao { get; set; }
    public string Email { get; set; } = "";
    public string? Senha { get; set; }
}
