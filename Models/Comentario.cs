// Comentário de uma Tarefa.
public class Comentario
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = "";
    public DateTime DataHora { get; set; }

    // Autor do comentário. Pelo banco, o funcionário precisa estar alocado na tarefa.
    public Funcionario Funcionario { get; set; } = new Funcionario();
}
