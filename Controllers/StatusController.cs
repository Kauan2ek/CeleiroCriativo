using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public StatusController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StatusDto>>> GetAll()
    {
        var status = await _context.Status.OrderBy(s => s.Codigo).ToListAsync();
        return Ok(status.Select(StatusDto.DeModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StatusDto>> GetById(int id)
    {
        var status = await _context.Status.FindAsync(id);
        if (status is null) return NotFound();
        return Ok(StatusDto.DeModel(status));
    }

    [HttpPost]
    public async Task<ActionResult<StatusDto>> Create(StatusCreateDto dto)
    {
        var status = new Models.StatusModel { Descricao = dto.Descricao };
        _context.Status.Add(status);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = status.Codigo }, StatusDto.DeModel(status));
    }
}
