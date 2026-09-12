using APICeleiroCriativo.Models;
using Microsoft.EntityFrameworkCore;

namespace APICeleiroCriativo.Data;

public class CeleiroCriativoContext : DbContext
{
    public CeleiroCriativoContext(DbContextOptions<CeleiroCriativoContext> options) : base(options)
    {
    }

    public DbSet<CargoModel> Cargos => Set<CargoModel>();
    public DbSet<ClienteModel> Clientes => Set<ClienteModel>();
    public DbSet<FuncionarioModel> Funcionarios => Set<FuncionarioModel>();
    public DbSet<StatusModel> Status => Set<StatusModel>();
    public DbSet<ProjetoModel> Projetos => Set<ProjetoModel>();
    public DbSet<TarefaModel> Tarefas => Set<TarefaModel>();
    public DbSet<CategoriaModel> Categorias => Set<CategoriaModel>();
    public DbSet<ComentarioModel> Comentarios => Set<ComentarioModel>();
    public DbSet<DocumentoModel> Documentos => Set<DocumentoModel>();
    public DbSet<ProjetoTarefaModel> ProjetosTarefas => Set<ProjetoTarefaModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Pessoa é a base (Table-per-Type): Cliente e Funcionario têm o mesmo
        // Codigo/PK de Pessoa, o que permite Comentario.PessoaId referenciar
        // tanto um cliente quanto um funcionário através de uma única tabela.
        modelBuilder.Entity<PessoaModel>().ToTable("Pessoa").HasKey(p => p.Codigo);
        modelBuilder.Entity<ClienteModel>().ToTable("Cliente");
        modelBuilder.Entity<FuncionarioModel>().ToTable("Funcionario");

        // "Codigo" não segue a convenção de chave do EF Core (Id / <Tipo>Id),
        // então cada entidade precisa declarar a chave primária explicitamente.
        modelBuilder.Entity<CargoModel>().HasKey(c => c.Codigo);
        modelBuilder.Entity<StatusModel>().HasKey(s => s.Codigo);
        modelBuilder.Entity<ProjetoModel>().HasKey(p => p.Codigo);
        modelBuilder.Entity<TarefaModel>().HasKey(t => t.Codigo);
        modelBuilder.Entity<CategoriaModel>().HasKey(c => c.Codigo);
        modelBuilder.Entity<ComentarioModel>().HasKey(c => c.Codigo);
        modelBuilder.Entity<DocumentoModel>().HasKey(d => d.Codigo);
        modelBuilder.Entity<ProjetoTarefaModel>().HasKey(pt => pt.Codigo);

        modelBuilder.Entity<ClienteModel>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<FuncionarioModel>()
            .HasIndex(f => f.Email)
            .IsUnique();

        modelBuilder.Entity<FuncionarioModel>()
            .HasOne(f => f.Cargo)
            .WithMany(c => c.Funcionarios)
            .HasForeignKey(f => f.CargoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjetoModel>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Projetos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TarefaModel>()
            .HasOne(t => t.Status)
            .WithMany(s => s.Tarefas)
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CategoriaModel>()
            .HasOne(c => c.Tarefa)
            .WithMany(t => t.Categorias)
            .HasForeignKey(c => c.TarefaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComentarioModel>()
            .HasOne(c => c.Tarefa)
            .WithMany(t => t.Comentarios)
            .HasForeignKey(c => c.TarefaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComentarioModel>()
            .HasOne(c => c.Pessoa)
            .WithMany()
            .HasForeignKey(c => c.PessoaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoModel>()
            .HasOne(d => d.Tarefa)
            .WithMany(t => t.Documentos)
            .HasForeignKey(d => d.TarefaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjetoTarefaModel>()
            .HasOne(pt => pt.Projeto)
            .WithMany(p => p.ProjetosTarefas)
            .HasForeignKey(pt => pt.ProjetoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjetoTarefaModel>()
            .HasOne(pt => pt.Tarefa)
            .WithMany(t => t.ProjetosTarefas)
            .HasForeignKey(pt => pt.TarefaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjetoTarefaModel>()
            .HasOne(pt => pt.Funcionario)
            .WithMany(f => f.ProjetosTarefas)
            .HasForeignKey(pt => pt.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjetoTarefaModel>()
            .HasIndex(pt => new { pt.ProjetoId, pt.TarefaId })
            .IsUnique();
    }
}
