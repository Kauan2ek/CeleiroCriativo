using APICeleiroCriativo.Models;

namespace APICeleiroCriativo.Dtos;

public record ProjetoTarefaDto(
    int Codigo,
    int ProjetoId,
    int TarefaId,
    int FuncionarioId,
    ProjetoResumoDto? Projeto,
    TarefaDto? Tarefa,
    PessoaResumoDto? Funcionario
)
{
    public static ProjetoTarefaDto DeModel(ProjetoTarefaModel pt) => new(
        pt.Codigo, pt.ProjetoId, pt.TarefaId, pt.FuncionarioId,
        pt.Projeto is null ? null : ProjetoResumoDto.DeModel(pt.Projeto),
        pt.Tarefa is null ? null : TarefaDto.DeModel(pt.Tarefa),
        pt.Funcionario is null ? null : PessoaResumoDto.DeModel(pt.Funcionario)
    );
}

public class ProjetoTarefaCreateDto
{
    public int ProjetoId { get; set; }
    public int TarefaId { get; set; }
    public int FuncionarioId { get; set; }
}
