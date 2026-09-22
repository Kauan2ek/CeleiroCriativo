public class Projeto
{
    public int Codigo { get; } // Código não é alterado via código
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataPrevista { get; set; }
    public Status Status { get; set; }
}
