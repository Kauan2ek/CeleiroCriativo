// Model de Projeto: representa os dados da tabela Projetos e os objetos
// relacionados que são úteis nas telas do sistema.
public class Projeto
{
    // Código gerado pelo banco de dados.
    public int Codigo { get; set; }

    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";

    // As datas são opcionais no banco, então também precisam aceitar null no C#.
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime? DataPrevista { get; set; }

    // Projeto possui um Status cadastrado na tabela Status.
    // O objeto já é inicializado para evitar NullReferenceException em telas de criação.
    public Status Status { get; set; } = new Status();

    // Lista das tarefas pertencentes ao projeto.
    // O nome foi mantido como já estava sendo usado no projeto.
    public List<Tarefa> tarefas { get; set; } = new List<Tarefa>();

    // Cliente dono do projeto.
    public Cliente Cliente { get; set; } = new Cliente();
}
