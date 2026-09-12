using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record ProjetoResumoDto(int Codigo, string Titulo)
{
    public static ProjetoResumoDto DeModel(ProjetoModel p) => new(p.Codigo, p.Titulo);
}

public record ProjetoDto(
    int Codigo,
    string Ideia,
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime? DataFim,
    DateTime DataPrevista,
    int ClienteId,
    PessoaResumoDto? Cliente,
    List<ProjetoTarefaDto> ProjetosTarefas
)
{
    public static ProjetoDto DeModel(ProjetoModel p) => new(
        p.Codigo, p.Ideia, p.Titulo, p.Descricao, p.DataInicio, p.DataFim, p.DataPrevista,
        p.ClienteId,
        p.Cliente is null ? null : PessoaResumoDto.DeModel(p.Cliente),
        p.ProjetosTarefas.Select(ProjetoTarefaDto.DeModel).ToList()
    );
}

public class ProjetoCreateDto
{
    public string? Ideia { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime DataPrevista { get; set; }
    public int ClienteId { get; set; }
}

public class ProjetoUpdateDto
{
    public string? Ideia { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime DataPrevista { get; set; }
}
