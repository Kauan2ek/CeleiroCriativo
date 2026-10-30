// Documento anexado a uma Tarefa.
// O arquivo em si fica numa pasta do servidor (wwwroot/uploads/tarefa-X/); no banco só fica o "endereço" dele.
// Diretorio é o caminho usado no link "Abrir arquivo" (ex: /uploads/tarefa-1/abc123.pdf).
public class Documento
{
    public int Codigo { get; set; }
    // nome original do arquivo, sem a extensão (ex: "logotipo-final"), é o que aparece na tela
    public string Descricao { get; set; } = "";
    // extensão sem o ponto (ex: "pdf")
    public string Extensao { get; set; } = "";
    public string Diretorio { get; set; } = "";
    // código da tarefa dona do documento (coluna tarefa_codigo), igual ao IdTarefa do Comentario
    // preenchido no Create e no Read; o DocumentosTarefa não preenche (fica 0), porque quem chama já sabe de qual tarefa é
    public int IdTarefa { get; set; } = 0;
}
