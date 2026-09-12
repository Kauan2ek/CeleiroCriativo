using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record StatusDto(int Codigo, string Descricao)
{
    public static StatusDto DeModel(StatusModel s) => new(s.Codigo, s.Descricao);
}

public class StatusCreateDto
{
    public string Descricao { get; set; } = string.Empty;
}
