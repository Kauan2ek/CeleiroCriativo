using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/cargos")]
public class CargoController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public CargoController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CargoDto>>> GetAll()
    {
        var cargos = await _context.Cargos.ToListAsync();
        return Ok(cargos.Select(CargoDto.DeModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CargoDto>> GetById(int id)
    {
        var cargo = await _context.Cargos.FindAsync(id);
        if (cargo is null) return NotFound();
        return Ok(CargoDto.DeModel(cargo));
    }

    [HttpPost]
    public async Task<ActionResult<CargoDto>> Create(CargoCreateDto dto)
    {
        var cargo = new Models.CargoModel { Descricao = dto.Descricao, Gestor = dto.Gestor ?? string.Empty };
        _context.Cargos.Add(cargo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cargo.Codigo }, CargoDto.DeModel(cargo));
    }
}
