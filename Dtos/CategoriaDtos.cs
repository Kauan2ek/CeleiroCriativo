using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record CategoriaDto(int Codigo, string Descricao, string? Cor, int TarefaId)
{
    public static CategoriaDto DeModel(CategoriaModel c) => new(c.Codigo, c.Descricao, c.Cor, c.TarefaId);
}

public class CategoriaCreateDto
{
    public string Descricao { get; set; } = string.Empty;
    public string? Cor { get; set; }
    public int TarefaId { get; set; }
}
