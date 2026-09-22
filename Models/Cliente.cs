public class Cliente : Pessoa
{
    public string CodigoVerificacao { get; } // Código não é alterado via código
    public string Email { get; set; }
    public string Senha { get; set; }
}
