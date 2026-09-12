namespace APICeleiroCriativo.Models;

public class CargoModel
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Gestor { get; set; } = string.Empty;

    public List<FuncionarioModel> Funcionarios { get; set; } = new();
}
