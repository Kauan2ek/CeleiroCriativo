using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using APICeleiroCriativo.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/funcionarios")]
public class FuncionarioController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public FuncionarioController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FuncionarioDto>>> GetAll()
    {
        var funcionarios = await _context.Funcionarios.Include(f => f.Cargo).ToListAsync();
        return Ok(funcionarios.Select(FuncionarioDto.DeModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FuncionarioDto>> GetById(int id)
    {
        var funcionario = await _context.Funcionarios.Include(f => f.Cargo)
            .FirstOrDefaultAsync(f => f.Codigo == id);
        if (funcionario is null) return NotFound();
        return Ok(FuncionarioDto.DeModel(funcionario));
    }

    [HttpPost]
    public async Task<ActionResult<FuncionarioDto>> Create(FuncionarioCreateDto dto)
    {
        if (await _context.Funcionarios.AnyAsync(f => f.Email == dto.Email))
            return Conflict(new { mensagem = "Já existe um funcionário com este e-mail." });

        if (!await _context.Cargos.AnyAsync(c => c.Codigo == dto.CargoId))
            return BadRequest(new { mensagem = "Cargo informado não existe." });

        var funcionario = new FuncionarioModel
        {
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            TipoDocumento = dto.TipoDocumento,
            Documento = dto.Documento,
            Email = dto.Email,
            Senha = PasswordHasher.Hash(dto.Senha),
            Ativo = dto.Ativo,
            CargoId = dto.CargoId,
        };
        _context.Funcionarios.Add(funcionario);
        await _context.SaveChangesAsync();
        await _context.Entry(funcionario).Reference(f => f.Cargo).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = funcionario.Codigo }, FuncionarioDto.DeModel(funcionario));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FuncionarioDto>> Update(int id, FuncionarioUpdateDto dto)
    {
        var funcionario = await _context.Funcionarios.Include(f => f.Cargo)
            .FirstOrDefaultAsync(f => f.Codigo == id);
        if (funcionario is null) return NotFound();

        if (!await _context.Cargos.AnyAsync(c => c.Codigo == dto.CargoId))
            return BadRequest(new { mensagem = "Cargo informado não existe." });

        funcionario.Nome = dto.Nome;
        funcionario.Telefone = dto.Telefone;
        funcionario.TipoDocumento = dto.TipoDocumento;
        funcionario.Documento = dto.Documento;
        funcionario.Email = dto.Email;
        funcionario.Ativo = dto.Ativo;
        funcionario.CargoId = dto.CargoId;
        if (!string.IsNullOrWhiteSpace(dto.Senha))
            funcionario.Senha = PasswordHasher.Hash(dto.Senha);

        await _context.SaveChangesAsync();
        await _context.Entry(funcionario).Reference(f => f.Cargo).LoadAsync();
        return Ok(FuncionarioDto.DeModel(funcionario));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario is null) return NotFound();

        _context.Funcionarios.Remove(funcionario);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
