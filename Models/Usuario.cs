public class Usuario {
    public int Codigo {get; set;}
    public string Papel {get; set; }
    public string Nome {get; set;}
    public string Email {get; set; }
    public string Senha {get; set; }
    public Funcionario? Funcionario {get; set;}
    public Cliente? Cliente {get; set; }

}