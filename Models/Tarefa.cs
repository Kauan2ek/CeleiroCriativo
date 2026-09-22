public class TarefaModel
{
    public int Codigo { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; } 
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public DateTime DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } 
}
