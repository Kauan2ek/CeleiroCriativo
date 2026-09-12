using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

/// <summary>Funcionário sem o campo Senha — nunca devolvido pela API.</summary>
public record FuncionarioDto(
    int Codigo,
    string Nome,
    string Telefone,
    string TipoDocumento,
    string Documento,
    string Email,
    bool Ativo,
    int CargoId,
    CargoDto? Cargo
)
{
    public static FuncionarioDto DeModel(FuncionarioModel f) => new(
        f.Codigo, f.Nome, f.Telefone, f.TipoDocumento, f.Documento, f.Email, f.Ativo,
        f.CargoId, f.Cargo is null ? null : CargoDto.DeModel(f.Cargo)
    );
}

public class FuncionarioCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public int CargoId { get; set; }
}

public class FuncionarioUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>Se vazio, a senha atual é mantida.</summary>
    public string? Senha { get; set; }
    public bool Ativo { get; set; } = true;
    public int CargoId { get; set; }
}
