using Microsoft.AspNetCore.Mvc;

public class DocumentoController : Controller
{
    [HttpPost]
    public bool Create(int idTarefa, IFormFile? arquivo)
    {
        if (arquivo == null)
        {
            return false;
        }

        string pasta = @$"/uploads/tarefas/tarefa-{idTarefa}";
        Directory.CreateDirectory(pasta);
        string? conteudo = arquivo.ToString();
        return true;
    }
}