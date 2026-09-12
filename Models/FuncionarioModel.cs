namespace APICeleiroCriativo.Models;

public class FuncionarioModel : PessoaModel
{
    public bool Ativo { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;

    public int CargoId { get; set; }
    public CargoModel? Cargo { get; set; }

    public List<ProjetoTarefaModel> ProjetosTarefas { get; set; } = new();
}
