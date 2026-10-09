public class Tarefa
{
    public int Codigo { get; set; } // Código não é alterado via código
    public string Titulo { get; set; }
    public string Descricao { get; set; } 
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public DateTime DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } 
    public List<Funcionario> Funcionarios {get; set;} // Alterado apenas via método
    public Status Status {get; set; } // O status deve ser alterado por meio de funções
    public List<Comentario> Comentarios {get; set;} // Alterado apenas via método
}
