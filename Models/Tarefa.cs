public class Tarefa
{
    public int Codigo { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; } 
    public DateTime? DataHoraInicio { get; set; }
    public DateTime? DataHoraPrevista { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public bool Visibilidade { get; set; } 
    public string Status {get; set; }
    public Categoria Categoria { get; set; }
    public List<Funcionario>? Funcionarios {get; set;}
    public List<Comentario>? Comentarios {get; set;}
    public List<Documento>? Documentos {get; set;}
    public int IdProjeto {get; set;}
}
