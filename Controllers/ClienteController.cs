using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using APICeleiroCriativo.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public ClienteController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return Ok(clientes.Select(ClienteDto.DeModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();
        return Ok(ClienteDto.DeModel(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto)
    {
        if (await _context.Clientes.AnyAsync(c => c.Email == dto.Email))
            return Conflict(new { mensagem = "Já existe um cliente com este e-mail." });

        var cliente = new ClienteModel
        {
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            TipoDocumento = dto.TipoDocumento,
            Documento = dto.Documento,
            Email = dto.Email,
            Senha = PasswordHasher.Hash(dto.Senha),
        };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cliente.Codigo }, ClienteDto.DeModel(cliente));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Update(int id, ClienteUpdateDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        cliente.Nome = dto.Nome;
        cliente.Telefone = dto.Telefone;
        cliente.TipoDocumento = dto.TipoDocumento;
        cliente.Documento = dto.Documento;
        cliente.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Senha))
            cliente.Senha = PasswordHasher.Hash(dto.Senha);

        await _context.SaveChangesAsync();
        return Ok(ClienteDto.DeModel(cliente));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
