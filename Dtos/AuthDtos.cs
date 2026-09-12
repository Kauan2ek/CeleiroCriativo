namespace APICeleiroCriativo.Dtos;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public record LoginResponseDto(string Tipo, object Usuario, string Token);
