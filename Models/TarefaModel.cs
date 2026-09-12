namespace APICeleiroCriativo.Models;

public class TarefaModel
{
    public int Codigo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public DateTime DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } = string.Empty;

    public int StatusId { get; set; }
    public StatusModel? Status { get; set; }

    public List<CategoriaModel> Categorias { get; set; } = new();
    public List<ComentarioModel> Comentarios { get; set; } = new();
    public List<DocumentoModel> Documentos { get; set; } = new();
    public List<ProjetoTarefaModel> ProjetosTarefas { get; set; } = new();
}
