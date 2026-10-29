// Documento anexado a uma Tarefa.
// Por enquanto só é lido (DocumentoSqlRepository.DocumentosTarefa) e listado na tela da tarefa; o upload está pendente.
// Diretorio é o caminho usado no link "Abrir arquivo".
public class Documento
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = "";
    public string Extensao { get; set; } = "";
    public string Diretorio { get; set; } = "";
    public int IdTarefa { get; set; } = 0;
}
