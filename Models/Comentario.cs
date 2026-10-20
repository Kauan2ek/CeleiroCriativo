// Comentário de uma Tarefa (mensagem do chat da tela da tarefa).
// Só funcionário comenta: cliente não tem comentário nesse modelo.
public class Comentario
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = "";
    // Preenchida pelo banco (DEFAULT GETDATE()), não vem do formulário.
    public DateTime DataHora { get; set; }

    // Autor do comentário. Pelo banco (FK_Comentarios_TarefaFuncionario), o funcionário precisa estar na equipe da tarefa.
    // No formulário só vai o código (campo Funcionario.Codigo); enquanto não existe login,
    // a tela manda o primeiro funcionário da equipe.
    // Na leitura (ComentariosTarefa) vem com nome e cargo, pra mostrar quem escreveu.
    public Funcionario Funcionario { get; set; } = new Funcionario();

    // código da tarefa dona do comentário (coluna tarefa_codigo), igual ao IdProjeto da Tarefa
    // usado no Create e no redirecionamento de volta pra tela da tarefa; o ComentariosTarefa não preenche (fica 0)
    public int IdTarefa { get; set; }
}
