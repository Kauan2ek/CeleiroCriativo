using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriaController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public CategoriaController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll([FromQuery] int? tarefaId)
    {
        var query = _context.Categorias.AsQueryable();
        if (tarefaId is not null) query = query.Where(c => c.TarefaId == tarefaId);

        var categorias = await query.ToListAsync();
        return Ok(categorias.Select(CategoriaDto.DeModel));
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create(CategoriaCreateDto dto)
    {
        if (!await _context.Tarefas.AnyAsync(t => t.Codigo == dto.TarefaId))
            return BadRequest(new { mensagem = "Tarefa informada não existe." });

        var categoria = new CategoriaModel
        {
            Descricao = dto.Descricao,
            Cor = dto.Cor ?? string.Empty,
            TarefaId = dto.TarefaId,
        };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return Ok(CategoriaDto.DeModel(categoria));
    }
}
