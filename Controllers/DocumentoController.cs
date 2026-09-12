using APICeleiroCriativo.Data;
using APICeleiroCriativo.Dtos;
using APICeleiroCriativo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Controllers;

[ApiController]
[Route("api/documentos")]
public class DocumentoController : ControllerBase
{
    private readonly CeleiroCriativoContext _context;
    private readonly IWebHostEnvironment _ambiente;

    public DocumentoController(CeleiroCriativoContext context, IWebHostEnvironment ambiente)
    {
        _context = context;
        _ambiente = ambiente;
    }

    private DocumentoDto ParaDto(DocumentoModel d)
    {
        var url = $"{Request.Scheme}://{Request.Host}{d.Diretorio}";
        return new DocumentoDto(d.Codigo, d.Descricao, d.Extensao, url, d.TarefaId);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentoDto>>> GetAll([FromQuery] int? tarefaId)
    {
        var query = _context.Documentos.AsQueryable();
        if (tarefaId is not null) query = query.Where(d => d.TarefaId == tarefaId);

        var documentos = await query.ToListAsync();
        return Ok(documentos.Select(ParaDto));
    }

    /// <summary>Upload multipart: campos de formulário "tarefaId" e "arquivo".</summary>
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<DocumentoDto>> Create([FromForm] int tarefaId, [FromForm] IFormFile arquivo)
    {
        if (!await _context.Tarefas.AnyAsync(t => t.Codigo == tarefaId))
            return BadRequest(new { mensagem = "Tarefa informada não existe." });
        if (arquivo.Length == 0)
            return BadRequest(new { mensagem = "Arquivo vazio." });

        // WebRootPath pode vir null se wwwroot/ não existir no momento em que o
        // host inicializou; monta o caminho a partir de ContentRootPath para
        // não depender disso.
        var raizWeb = _ambiente.WebRootPath ?? Path.Combine(_ambiente.ContentRootPath, "wwwroot");
        var pastaTarefa = Path.Combine(raizWeb, "uploads", tarefaId.ToString());
        Directory.CreateDirectory(pastaTarefa);

        var extensao = Path.GetExtension(arquivo.FileName);
        var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
        var caminhoFisico = Path.Combine(pastaTarefa, nomeArquivo);

        await using (var stream = System.IO.File.Create(caminhoFisico))
        {
            await arquivo.CopyToAsync(stream);
        }

        var documento = new DocumentoModel
        {
            Descricao = arquivo.FileName,
            Extensao = extensao.TrimStart('.'),
            Diretorio = $"/uploads/{tarefaId}/{nomeArquivo}",
            TarefaId = tarefaId,
        };
        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync();

        return Ok(ParaDto(documento));
    }
}
