using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record CargoDto(int Codigo, string Descricao, string? Gestor)
{
    public static CargoDto DeModel(CargoModel c) => new(c.Codigo, c.Descricao, c.Gestor);
}

public class CargoCreateDto
{
    public string Descricao { get; set; } = string.Empty;
    public string? Gestor { get; set; }
}
