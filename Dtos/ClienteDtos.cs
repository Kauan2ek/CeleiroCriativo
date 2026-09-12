using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

/// <summary>Cliente sem o campo Senha — nunca devolvido pela API.</summary>
public record ClienteDto(
    int Codigo,
    string Nome,
    string Telefone,
    string TipoDocumento,
    string Documento,
    string Email,
    string CodigoVerificacao
)
{
    public static ClienteDto DeModel(ClienteModel c) => new(
        c.Codigo, c.Nome, c.Telefone, c.TipoDocumento, c.Documento, c.Email, c.CodigoVerificacao
    );
}

public class ClienteCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class ClienteUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>Se vazio, a senha atual é mantida.</summary>
    public string? Senha { get; set; }
}
