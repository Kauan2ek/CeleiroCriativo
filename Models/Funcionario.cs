// Funcionário é um subtipo de Pessoa no banco de dados.
public class Funcionario : Pessoa
{
    // A exclusão de funcionário será lógica: Ativo = false.
    public bool Ativo { get; set; } = true;

    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";

    // Cargo é uma entidade auxiliar, carregada da tabela Cargos.
    public Cargo Cargo { get; set; } = new Cargo();
}
