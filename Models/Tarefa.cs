// Model da tarefa: representa uma linha da tabela Tarefas, mais os dados ligados a ela.
// É o objeto que viaja entre a View, o Controller e o Repository.
public class Tarefa
{
    // Gerado pelo banco (IDENTITY), não é informado na criação.
    public int Codigo { get; set; }

    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";

    // O ? deixa a data aceitar null, porque as três são opcionais no banco.
    public DateTime? DataHoraInicio { get; set; }
    public DateTime? DataHoraPrevista { get; set; }
    public DateTime? DataHoraFim { get; set; }

    // Se a tarefa aparece ou não para o cliente.
    public bool Visibilidade { get; set; } = true;

    // Status da tarefa é texto no banco, diferente do Status de Projeto.
    // Valores usados atualmente: "A fazer", "Em andamento" e "Finalizado".
    // "Cancelada" é reservado para a exclusão lógica.
    public string Status { get; set; } = "A fazer";

    // No banco a tarefa guarda apenas categoria_codigo; na Model usamos o objeto completo.
    public Categoria Categoria { get; set; } = new Categoria();

    // Estas listas vêm de tabelas relacionadas e não de colunas da tabela Tarefas.
    public List<Funcionario>? Funcionarios { get; set; } = new List<Funcionario>();
    public List<Comentario>? Comentarios { get; set; } = new List<Comentario>();
    public List<Documento>? Documentos { get; set; } = new List<Documento>();

    // Código do projeto ao qual a tarefa pertence.
    public int IdProjeto { get; set; }
}
