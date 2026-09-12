namespace APICeleiroCriativo.Models;

public class ProjetoTarefaModel
{
    public int Codigo { get; set; }

    public int ProjetoId { get; set; }
    public ProjetoModel? Projeto { get; set; }

    public int TarefaId { get; set; }
    public TarefaModel? Tarefa { get; set; }

    public int FuncionarioId { get; set; }
    public FuncionarioModel? Funcionario { get; set; }
}
