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
    // texto do status: "A fazer", "Em andamento" ou "Finalizado"
    // tem que ser escrito exatamente assim (maiúsculas e minúsculas), porque o select da tela,
    // as colunas do kanban e o js comparam letra por letra
    // 'Cancelada' é usado como exclusão lógica: a view V_Tarefas esconde essas tarefas
    public string Status {get; set; }
    // no banco é só a coluna categoria_codigo, aqui é o objeto inteiro (código, descrição e cor)
    // no formulário só vai o código (campo Categoria.Codigo)
    public Categoria Categoria { get; set; }
    // as três listas não são colunas da tabela Tarefas, cada item é uma linha em outra tabela:
    // TarefaFuncionarios, Comentarios e Documentos
    // equipe da tarefa: no Create/Update, lista nula quer dizer que nenhum funcionário foi marcado no modal
    public List<Funcionario>? Funcionarios {get; set;}
    // só leitura aqui: preenchida pelo Read pra mostrar o chat
    // gravar comentário é com o ComentarioController, não pelo formulário da tarefa
    public List<Comentario>? Comentarios {get; set;}
    // só leitura por enquanto (o upload de arquivo está pendente)
    public List<Documento>? Documentos {get; set;}
    // código do projeto dono da tarefa (coluna projeto_codigo)
    // na tarefa nova vem da url (?idProjeto=2); na existente vem do Read
    // o ReadAll não preenche (fica 0), por isso o kanban usa a ViewBag.IdProjeto
    public int IdProjeto {get; set;}
}
