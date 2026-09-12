namespace APICeleiroCriativo.Models;

public class CategoriaModel
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;

    public int TarefaId { get; set; }
    public TarefaModel? Tarefa { get; set; }
}
