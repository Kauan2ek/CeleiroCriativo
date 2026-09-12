using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

/// <summary>Referência curta de uma Pessoa (cliente ou funcionário), sem dados sensíveis.</summary>
public record PessoaResumoDto(int Codigo, string Nome)
{
    public static PessoaResumoDto DeModel(PessoaModel p) => new(p.Codigo, p.Nome);
}
