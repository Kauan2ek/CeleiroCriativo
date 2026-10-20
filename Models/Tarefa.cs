// model da tarefa: representa uma linha da tabela Tarefas, mais os dados ligados a ela
// é o objeto que viaja entre a view, o controller e o repositório
public class Tarefa
{
    // gerado pelo banco (IDENTITY), não é informado na criação
    public int Codigo { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    // o ? deixa a data aceitar null, porque as três são opcionais no banco
    public DateTime? DataHoraInicio { get; set; }
    public DateTime? DataHoraPrevista { get; set; }
    public DateTime? DataHoraFim { get; set; }
    // se a tarefa aparece ou não pro cliente
    public bool Visibilidade { get; set; }
    // texto do status (ex: "A Fazer", "Em Andamento", "Finalizado")
    // 'Cancelada' é usado como exclusão lógica: a view V_Tarefas esconde essas tarefas
    public string Status {get; set; }
    // no banco é só a coluna categoria_codigo, aqui é o objeto inteiro (código, descrição e cor)
    public Categoria Categoria { get; set; }
    // as três listas não são colunas da tabela Tarefas, cada item é uma linha em outra tabela:
    // TarefaFuncionarios, Comentarios e Documentos
    public List<Funcionario>? Funcionarios {get; set;}
    public List<Comentario>? Comentarios {get; set;}
    public List<Documento>? Documentos {get; set;}
    // código do projeto dono da tarefa (coluna projeto_codigo)
    public int IdProjeto {get; set;}
}
