public class Status
{
    public int Codigo { get; set; } // Código não é alterado via código
    public string Descricao { get; set; } 
    
    public Status(int codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
