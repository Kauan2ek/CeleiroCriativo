namespace APICeleiroCriativo.Models;

public class ProjetoModel
{
    public int Codigo { get; set; }
    public string Ideia { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime DataPrevista { get; set; }

    public int ClienteId { get; set; }
    public ClienteModel? Cliente { get; set; }

    public List<ProjetoTarefaModel> ProjetosTarefas { get; set; } = new();
}
