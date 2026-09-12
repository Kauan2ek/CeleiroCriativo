using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record TarefaDto(
    int Codigo,
    string Titulo,
    string Descricao,
    DateTime DataHoraInicio,
    DateTime? DataHoraFim,
    DateTime DataHoraPrevista,
    string Visibilidade,
    int StatusId,
    StatusDto? Status
)
{
    public static TarefaDto DeModel(TarefaModel t) => new(
        t.Codigo, t.Titulo, t.Descricao, t.DataHoraInicio, t.DataHoraFim, t.DataHoraPrevista,
        t.Visibilidade, t.StatusId, t.Status is null ? null : StatusDto.DeModel(t.Status)
    );
}

public class TarefaCreateDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime? DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public DateTime? DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } = "Publica";
    public int StatusId { get; set; }
}

public class TarefaUpdateDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime? DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public DateTime? DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } = "Publica";
    public int StatusId { get; set; }
}
