namespace APICeleiroCriativo.Models;

public class StatusModel
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public List<TarefaModel> Tarefas { get; set; } = new();
}
