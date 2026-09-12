using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record ComentarioDto(
    int Codigo,
    string? Titulo,
    string Descricao,
    DateTime DataHora,
    int TarefaId,
    int PessoaId,
    PessoaResumoDto? Pessoa
)
{
    public static ComentarioDto DeModel(ComentarioModel c) => new(
        c.Codigo, c.Titulo, c.Descricao, c.DataHora, c.TarefaId, c.PessoaId,
        c.Pessoa is null ? null : PessoaResumoDto.DeModel(c.Pessoa)
    );
}

public class ComentarioCreateDto
{
    public string? Titulo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime? DataHora { get; set; }
    public int TarefaId { get; set; }
    public int PessoaId { get; set; }
}
