using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/projetotarefas")]
public class ProjetoTarefaController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public ProjetoTarefaController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    private IQueryable<ProjetoTarefaModel> ConsultaCompleta() =>
        _context.ProjetosTarefas
            .Include(pt => pt.Projeto)
            .Include(pt => pt.Tarefa).ThenInclude(t => t!.Status)
            .Include(pt => pt.Funcionario);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjetoTarefaDto>>> GetAll(
        [FromQuery] int? projetoId, [FromQuery] int? funcionarioId)
    {
        var query = ConsultaCompleta();
        if (projetoId is not null) query = query.Where(pt => pt.ProjetoId == projetoId);
        if (funcionarioId is not null) query = query.Where(pt => pt.FuncionarioId == funcionarioId);

        var itens = await query.ToListAsync();
        return Ok(itens.Select(ProjetoTarefaDto.DeModel));
    }

    [HttpPost]
    public async Task<ActionResult<ProjetoTarefaDto>> Create(ProjetoTarefaCreateDto dto)
    {
        if (!await _context.Projetos.AnyAsync(p => p.Codigo == dto.ProjetoId))
            return BadRequest(new { mensagem = "Projeto informado não existe." });
        if (!await _context.Tarefas.AnyAsync(t => t.Codigo == dto.TarefaId))
            return BadRequest(new { mensagem = "Tarefa informada não existe." });
        if (!await _context.Funcionarios.AnyAsync(f => f.Codigo == dto.FuncionarioId))
            return BadRequest(new { mensagem = "Funcionário informado não existe." });
        if (await _context.ProjetosTarefas.AnyAsync(pt => pt.ProjetoId == dto.ProjetoId && pt.TarefaId == dto.TarefaId))
            return Conflict(new { mensagem = "Esta tarefa já está vinculada a este projeto." });

        var projetoTarefa = new ProjetoTarefaModel
        {
            ProjetoId = dto.ProjetoId,
            TarefaId = dto.TarefaId,
            FuncionarioId = dto.FuncionarioId,
        };
        _context.ProjetosTarefas.Add(projetoTarefa);
        await _context.SaveChangesAsync();

        var criado = await ConsultaCompleta().FirstAsync(pt => pt.Codigo == projetoTarefa.Codigo);
        return Ok(ProjetoTarefaDto.DeModel(criado));
    }
}
