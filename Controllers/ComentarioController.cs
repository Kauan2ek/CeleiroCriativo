using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/comentarios")]
public class ComentarioController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public ComentarioController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComentarioDto>>> GetAll([FromQuery] int? tarefaId)
    {
        var query = _context.Comentarios.Include(c => c.Pessoa).AsQueryable();
        if (tarefaId is not null) query = query.Where(c => c.TarefaId == tarefaId);

        var comentarios = await query.OrderBy(c => c.DataHora).ToListAsync();
        return Ok(comentarios.Select(ComentarioDto.DeModel));
    }

    [HttpPost]
    public async Task<ActionResult<ComentarioDto>> Create(ComentarioCreateDto dto)
    {
        if (!await _context.Tarefas.AnyAsync(t => t.Codigo == dto.TarefaId))
            return BadRequest(new { mensagem = "Tarefa informada não existe." });
        if (!await _context.Set<PessoaModel>().AnyAsync(p => p.Codigo == dto.PessoaId))
            return BadRequest(new { mensagem = "Pessoa informada não existe." });

        var comentario = new ComentarioModel
        {
            Titulo = dto.Titulo ?? string.Empty,
            Descricao = dto.Descricao,
            DataHora = dto.DataHora ?? DateTime.UtcNow,
            TarefaId = dto.TarefaId,
            PessoaId = dto.PessoaId,
        };
        _context.Comentarios.Add(comentario);
        await _context.SaveChangesAsync();
        await _context.Entry(comentario).Reference(c => c.Pessoa).LoadAsync();

        return Ok(ComentarioDto.DeModel(comentario));
    }
}
