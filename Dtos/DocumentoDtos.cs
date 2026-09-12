namespace APICeleiroCriativo.Dtos;

public record DocumentoDto(int Codigo, string? Descricao, string? Extensao, string Diretorio, int TarefaId);
