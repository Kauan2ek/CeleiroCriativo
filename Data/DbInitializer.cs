using APICeleiroCriativo.Models;
using APICeleiroCriativo.Security;

namespace APICeleiroCriativo.Data;

/// <summary>
/// Seed de dados de desenvolvimento: cargos, status e usuários de teste
/// (mesmas credenciais documentadas em System/database/celeiro_criativo_create.sql
/// e em System/CeleiroCriativo/README.md).
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(CeleiroCriativoContext context)
    {
        if (!context.Cargos.Any())
        {
            context.Cargos.AddRange(
                new CargoModel { Descricao = "Gestor" },
                new CargoModel { Descricao = "Designer" },
                new CargoModel { Descricao = "Programador" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Status.Any())
        {
            context.Status.AddRange(
                new StatusModel { Descricao = "Pendente" },
                new StatusModel { Descricao = "Em Andamento" },
                new StatusModel { Descricao = "Concluída" },
                new StatusModel { Descricao = "Cancelada" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Clientes.Any() && !context.Funcionarios.Any())
        {
            var cargoGestor = context.Cargos.First(c => c.Descricao == "Gestor");
            var cargoDesigner = context.Cargos.First(c => c.Descricao == "Designer");

            var cliente = new ClienteModel
            {
                Nome = "Cliente Teste",
                Telefone = "(17) 99999-0001",
                TipoDocumento = "CPF",
                Documento = "00000000001",
                Email = "cliente@teste.com",
                Senha = PasswordHasher.Hash("123456"),
            };

            var gestor = new FuncionarioModel
            {
                Nome = "Gestor Teste",
                Telefone = "(17) 99999-0002",
                TipoDocumento = "CPF",
                Documento = "00000000002",
                Email = "gestor@teste.com",
                Senha = PasswordHasher.Hash("123456"),
                Ativo = true,
                CargoId = cargoGestor.Codigo,
            };

            var funcionario = new FuncionarioModel
            {
                Nome = "Funcionario Teste",
                Telefone = "(17) 99999-0003",
                TipoDocumento = "CPF",
                Documento = "00000000003",
                Email = "funcionario@teste.com",
                Senha = PasswordHasher.Hash("123456"),
                Ativo = true,
                CargoId = cargoDesigner.Codigo,
            };

            context.Clientes.Add(cliente);
            context.Funcionarios.AddRange(gestor, funcionario);
            await context.SaveChangesAsync();

            var statusEmAndamento = context.Status.First(s => s.Descricao == "Em Andamento");

            var projeto = new ProjetoModel
            {
                Titulo = "Campanha de Lançamento",
                Ideia = "Divulgar o novo produto nas redes sociais.",
                Descricao = "Projeto de exemplo para testes do sistema.",
                DataInicio = DateTime.UtcNow,
                DataPrevista = DateTime.UtcNow.AddDays(30),
                ClienteId = cliente.Codigo,
            };
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();

            var tarefa = new TarefaModel
            {
                Titulo = "Criar peças gráficas",
                Descricao = "Arte para redes sociais do lançamento.",
                DataHoraInicio = DateTime.UtcNow,
                DataHoraPrevista = DateTime.UtcNow.AddDays(10),
                Visibilidade = "Publica",
                StatusId = statusEmAndamento.Codigo,
            };
            context.Tarefas.Add(tarefa);
            await context.SaveChangesAsync();

            context.ProjetosTarefas.Add(new ProjetoTarefaModel
            {
                ProjetoId = projeto.Codigo,
                TarefaId = tarefa.Codigo,
                FuncionarioId = funcionario.Codigo,
            });
            await context.SaveChangesAsync();
        }
    }
}
