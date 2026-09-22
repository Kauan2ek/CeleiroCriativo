public class TarefaModel
{
    public int Codigo { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; } 
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public DateTime DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } 
    public List<Funcionario> Funcionarios {get;}
    public Status Status {get;} // O status deve ser alterado por meio de funções

    public bool AdicionarFuncionario(Funcionario func)
    {
        Funcionarios.Add(func);
        return true; 
    }

    public bool RemoverFuncionario(Funcionario func)
    {
        try
        {
            Funcionarios.Remove(func);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao adicionar funcionário: " + ex.Message);
            return false;
        }
    }
}
