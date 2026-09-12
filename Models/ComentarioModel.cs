namespace APICeleiroCriativo.Models;

public class ComentarioModel
{
    public int Codigo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }

    public int TarefaId { get; set; }
    public TarefaModel? Tarefa { get; set; }

    public int PessoaId { get; set; }
    public PessoaModel? Pessoa { get; set; }
}
