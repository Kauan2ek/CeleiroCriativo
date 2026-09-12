namespace APICeleiroCriativo.Models;

public class DocumentoModel
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Extensao { get; set; } = string.Empty;
    public string Diretorio { get; set; } = string.Empty;

    public int TarefaId { get; set; }
    public TarefaModel? Tarefa { get; set; }
}
