public class Tarefa
{
    public int Codigo { get; } // Código não é alterado via código
    public string Titulo { get; set; }
    public string Descricao { get; set; } 
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public DateTime DataHoraPrevista { get; set; }
    public string Visibilidade { get; set; } 
    public List<Funcionario> Funcionarios {get; } // Alterado apenas via método
    public Status Status {get; set; } // O status deve ser alterado por meio de funções

    public bool AdicionarFuncionario(Funcionario func)
    {
        try
        {
            Funcionarios.Add(func);
            return true; 
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao adicionar funcionário: " + ex.Message);
            return false;
        }
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
            Console.WriteLine("Erro ao remover funcionário: " + ex.Message);
            return false;
        }
    }
}
