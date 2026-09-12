using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;

    public AuthController(CeleiroCriativoContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Login único para Cliente e Funcionário (o papel de Gestor é resolvido
    /// pelo front-end a partir do Cargo do Funcionário). Simplificação
    /// assumida para este projeto acadêmico: o token retornado não é
    /// validado pela API em requisições seguintes (sem [Authorize]) — não
    /// há auditoria/controle de sessão real, conforme já fora do escopo
    /// definido para o sistema.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == dto.Email);
        if (cliente is not null && PasswordHasher.Verify(dto.Senha, cliente.Senha))
        {
            var token = Guid.NewGuid().ToString("N");
            return Ok(new LoginResponseDto("cliente", ClienteDto.DeModel(cliente), token));
        }

        var funcionario = await _context.Funcionarios
            .Include(f => f.Cargo)
            .FirstOrDefaultAsync(f => f.Email == dto.Email);
        if (funcionario is not null && funcionario.Ativo && PasswordHasher.Verify(dto.Senha, funcionario.Senha))
        {
            var token = Guid.NewGuid().ToString("N");
            return Ok(new LoginResponseDto("funcionario", FuncionarioDto.DeModel(funcionario), token));
        }

        return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
    }
}
