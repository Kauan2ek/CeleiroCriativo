// Categoria utilizada para classificar tarefas.
// Cada tarefa tem uma categoria (coluna Tarefas.categoria_codigo).
// Na tarefa, vem montada pela view V_Tarefas (colunas categoria_*); no select da tela, vem do CategoriaSqlRepository.ReadAll().
public class Categoria
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = "";
    // Cor opcional no banco, por isso o ?.
    public string? Cor { get; set; }
}
