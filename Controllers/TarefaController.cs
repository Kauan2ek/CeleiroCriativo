using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/tarefas")]
public class TarefaController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public TarefaController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TarefaDto>>> GetAll()
    {
        var tarefas = await _context.Tarefas.Include(t => t.Status).ToListAsync();
        return Ok(tarefas.Select(TarefaDto.DeModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TarefaDto>> GetById(int id)
    {
        var tarefa = await _context.Tarefas.Include(t => t.Status).FirstOrDefaultAsync(t => t.Codigo == id);
        if (tarefa is null) return NotFound();
        return Ok(TarefaDto.DeModel(tarefa));
    }

    [HttpPost]
    public async Task<ActionResult<TarefaDto>> Create(TarefaCreateDto dto)
    {
        if (!await _context.Status.AnyAsync(s => s.Codigo == dto.StatusId))
            return BadRequest(new { mensagem = "Status informado não existe." });

        var tarefa = new TarefaModel
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao ?? string.Empty,
            DataHoraInicio = dto.DataHoraInicio ?? DateTime.UtcNow,
            DataHoraFim = dto.DataHoraFim,
            DataHoraPrevista = dto.DataHoraPrevista ?? DateTime.UtcNow,
            Visibilidade = dto.Visibilidade,
            StatusId = dto.StatusId,
        };
        _context.Tarefas.Add(tarefa);
        await _context.SaveChangesAsync();
        await _context.Entry(tarefa).Reference(t => t.Status).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = tarefa.Codigo }, TarefaDto.DeModel(tarefa));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TarefaDto>> Update(int id, TarefaUpdateDto dto)
    {
        var tarefa = await _context.Tarefas.Include(t => t.Status).FirstOrDefaultAsync(t => t.Codigo == id);
        if (tarefa is null) return NotFound();

        if (!await _context.Status.AnyAsync(s => s.Codigo == dto.StatusId))
            return BadRequest(new { mensagem = "Status informado não existe." });

        tarefa.Titulo = dto.Titulo;
        tarefa.Descricao = dto.Descricao ?? string.Empty;
        if (dto.DataHoraInicio is not null) tarefa.DataHoraInicio = dto.DataHoraInicio.Value;
        tarefa.DataHoraFim = dto.DataHoraFim;
        if (dto.DataHoraPrevista is not null) tarefa.DataHoraPrevista = dto.DataHoraPrevista.Value;
        tarefa.Visibilidade = dto.Visibilidade;
        tarefa.StatusId = dto.StatusId;

        await _context.SaveChangesAsync();
        await _context.Entry(tarefa).Reference(t => t.Status).LoadAsync();
        return Ok(TarefaDto.DeModel(tarefa));
    }
}
