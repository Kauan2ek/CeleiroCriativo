using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

// controller dos documentos: igual ao ComentarioController, não tem tela própria
// o upload e a exclusão são feitos pela coluna "Arquivos" da tela da tarefa (DetalhesGestor)
// toda action volta pra tela da tarefa; se der erro, a mensagem vai pela url (?erro=...)
// e a action DetalhesGestor do TarefaController coloca ela no ViewBag.Erro
public class DocumentoController : Controller
{
    // repositório que faz o acesso ao banco, o controller nunca escreve sql
    private DocumentoSqlRepository repository;
    // wwwroot é pública pela url, então não dá pra deixar subir qualquer coisa (ex: .html com script)
    private readonly string[] extensoesPermitidas =
        { "pdf", "png", "jpg", "jpeg", "gif", "docx", "xlsx", "pptx", "txt", "mp4", "zip" };

    // igual aos outros controllers: criar o repositório já abre a conexão com o banco
    public DocumentoController()
    {
        repository = new DocumentoSqlRepository();
    }


    // envia um arquivo pra tarefa
    // POST /Documento/Create, enviado pelo formUploadDocumento (enctype="multipart/form-data")
    // idTarefa vem do hidden; arquivo vem do <input type="file" name="arquivo"> do botão "+"
    // os nomes dos parâmetros precisam ser iguais aos "name" do form
    [HttpPost]
    public ActionResult Create(int idTarefa, IFormFile? arquivo)
    {
        // nenhum arquivo escolhido, ou arquivo vazio
        if (arquivo == null || arquivo.Length == 0)
        {
            repository.Dispose();
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa, erro = "Selecione um arquivo ao enviar."});
        }

        // arquivo.PdF -> "pdf"
        string extensao = Path.GetExtension(arquivo.FileName).TrimStart('.').ToLower();

        if (!extensoesPermitidas.Contains(extensao))
        {
            repository.Dispose();
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa, erro = "Extensão não permitida."});
        }

        // 1) Salvar o arquivo no disco: wwwroot/uploads/tarefa-1/
        // caminho relativo: parte da pasta do projeto (onde roda o dotnet run)
        string pastaTarefa = @$"wwwroot/uploads/tarefa-{idTarefa}";
        Directory.CreateDirectory(pastaTarefa); // Se já existir, não faz nada

        // nome único (Guid): evita sobrescrever arquivos com o mesmo nome e nomes perigosos como "../"
        // o nome original não se perde: ele vai pro banco na Descricao
        string nome = $"{Guid.NewGuid()}.{extensao}";
        string path = Path.Combine(pastaTarefa, nome);

        // cria o arquivo vazio no disco, copia o conteúdo enviado pra dentro dele e fecha
        FileStream stream = new FileStream(path, FileMode.Create);
        arquivo.CopyTo(stream);
        stream.Close();

        // 2) Gravar no banco
        Documento documento = new Documento();
        documento.IdTarefa = idTarefa;
        documento.Descricao = Path.GetFileNameWithoutExtension(arquivo.FileName); // nome original, pra tela
        documento.Extensao = extensao;
        // caminho pela url: sem o "wwwroot", porque o asp.net serve a wwwroot como raiz do site
        documento.Diretorio = $"/uploads/tarefa-{idTarefa}/{nome}";

        try
        {
            repository.Create(documento);
            repository.Dispose();
            // volta pra tela da tarefa, que busca os documentos de novo e já mostra o novo na lista
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa});
        }
        // se o banco recusar (ex: tarefa não existe), a procedure lança o erro e ele "sobe" até aqui
        catch (SqlException e)
        {
            // o banco recusou: apaga o arquivo pra ele não ficar "órfão" na pasta
            // System.IO.File e não só File: dentro do Controller, File(...) é o método que devolve arquivo como resposta
            System.IO.File.Delete(path);

            repository.Dispose();
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa, erro = e.Message});
        }
    }

    // exclui um arquivo da tarefa (do banco e do disco)
    // POST /Documento/Delete, enviado pelo formExcluirDocumento-X (botão de lixeira do arquivo)
    // id é o código do documento; idTarefa é só pra voltar pra tela da tarefa
    [HttpPost]
    public ActionResult Delete(int id, int idTarefa)
    {
        try
        {
            // busca o documento no banco pra saber onde o arquivo está no disco
            // (o caminho não vem do form, pra ninguém conseguir mandar o caminho de outro arquivo do servidor)
            Documento? documento = repository.Read(id);

            if (documento == null)
            {
                repository.Dispose();
                return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa, erro = "Documento não encontrado."});
            }

            // 1) APAGAR DO BANCO
            // primeiro o banco: se ele recusar, cai no catch e o arquivo continua no disco, sem perder nada
            repository.Delete(id);
            repository.Dispose();

            // 2) APAGAR DO DISCO: "/uploads/tarefa-1/abc.pdf" -> "wwwroot/uploads/tarefa-1/abc.pdf"
            string path = "wwwroot" + documento.Diretorio;

            // o Exists evita erro nos documentos antigos do banco, que não têm arquivo de verdade na pasta
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa});
        }
        catch (SqlException e)
        {
            repository.Dispose();
            return RedirectToAction("DetalhesGestor", "Tarefa", new {id = idTarefa, erro = e.Message});
        }
    }
}
