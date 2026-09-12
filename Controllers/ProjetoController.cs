using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/projetos")]
public class ProjetoController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public ProjetoController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    private IQueryable<ProjetoModel> ConsultaCompleta() =>
        _context.Projetos
            .Include(p => p.Cliente)
            .Include(p => p.ProjetosTarefas).ThenInclude(pt => pt.Tarefa).ThenInclude(t => t!.Status)
            .Include(p => p.ProjetosTarefas).ThenInclude(pt => pt.Funcionario);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjetoDto>>> GetAll([FromQuery] int? clienteId)
    {
        var query = ConsultaCompleta();
        if (clienteId is not null) query = query.Where(p => p.ClienteId == clienteId);

        var projetos = await query.OrderByDescending(p => p.DataInicio).ToListAsync();
        return Ok(projetos.Select(ProjetoDto.DeModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjetoDto>> GetById(int id)
    {
        var projeto = await ConsultaCompleta().FirstOrDefaultAsync(p => p.Codigo == id);
        if (projeto is null) return NotFound();
        return Ok(ProjetoDto.DeModel(projeto));
    }

    [HttpPost]
    public async Task<ActionResult<ProjetoDto>> Create(ProjetoCreateDto dto)
    {
        if (!await _context.Clientes.AnyAsync(c => c.Codigo == dto.ClienteId))
            return BadRequest(new { mensagem = "Cliente informado não existe." });

        var projeto = new ProjetoModel
        {
            Ideia = dto.Ideia ?? string.Empty,
            Titulo = dto.Titulo,
            Descricao = dto.Descricao ?? string.Empty,
            DataInicio = dto.DataInicio ?? DateTime.UtcNow,
            DataFim = dto.DataFim,
            DataPrevista = dto.DataPrevista,
            ClienteId = dto.ClienteId,
        };
        _context.Projetos.Add(projeto);
        await _context.SaveChangesAsync();

        var criado = await ConsultaCompleta().FirstAsync(p => p.Codigo == projeto.Codigo);
        return CreatedAtAction(nameof(GetById), new { id = projeto.Codigo }, ProjetoDto.DeModel(criado));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjetoDto>> Update(int id, ProjetoUpdateDto dto)
    {
        var projeto = await _context.Projetos.FindAsync(id);
        if (projeto is null) return NotFound();

        projeto.Ideia = dto.Ideia ?? string.Empty;
        projeto.Titulo = dto.Titulo;
        projeto.Descricao = dto.Descricao ?? string.Empty;
        if (dto.DataInicio is not null) projeto.DataInicio = dto.DataInicio.Value;
        projeto.DataFim = dto.DataFim;
        projeto.DataPrevista = dto.DataPrevista;

        await _context.SaveChangesAsync();

        var atualizado = await ConsultaCompleta().FirstAsync(p => p.Codigo == id);
        return Ok(ProjetoDto.DeModel(atualizado));
    }
}
