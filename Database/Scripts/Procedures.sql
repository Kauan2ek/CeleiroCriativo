use CeleiroCriativo
go

create or alter procedure CadastrarPessoa
(
	@nome varchar(50), @telefone varchar(20), @documento varchar(20), @tipo_doc varchar(4), @tipo_pessoa varchar(15),

    -- Dados do funcionário
    @cargo_codigo int = NULL, @ativo bit = 1,

    -- Dados de acesso
    @email varchar(100), @senha varchar(255),

    -- Dados do cliente
    @codigo_verificacao varchar(20) = NULL
)
as
begin
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

    begin try

        begin tran

        -- Cadastra a pessoa --
        insert into Pessoas (nome, telefone, documento, tipo_doc)
        values (@nome, @telefone, @documento, @tipo_doc)

		-- Recupera o código gerado pela Pessoa --
        declare @pessoa_codigo int;

        set @pessoa_codigo = SCOPE_IDENTITY();

		-- Verifica qual tipo de pessoa será cadastrado --
        if @tipo_pessoa = 'FUNCIONARIO'
        begin

            insert into Funcionarios (pessoa_codigo, cargo_codigo, ativo, email, senha)
            values (@pessoa_codigo, @cargo_codigo, @ativo, @email, @senha)

        end

        else if @tipo_pessoa = 'CLIENTE'
        begin

            insert into Clientes(pessoa_codigo, codigo_verificacao, email, senha)
            values(@pessoa_codigo, @codigo_verificacao, @email, @senha)

        end

        -- Confirma as operações --
        commit

        -- Retorna o código da pessoa cadastrada --
        select @pessoa_codigo as pessoa_codigo

    end try

	begin catch
		-- Desfazer a transacao caso ocorra um erro --
        if @@TRANCOUNT > 0
            rollback;

		throw

	end catch
end
go

create or alter procedure AlterarStatusProjeto
    @projeto_codigo int, @status_codigo int = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o projeto existe --
        if not exists (
            select 1 from Projetos where codigo = @projeto_codigo
        )
            throw 50011, 'Projeto nao encontrado.', 1;

        -- Validar o status, se informado --
        if @status_codigo is not null and not exists (
               select 1 from Status where codigo = @status_codigo
           )
            throw 50012, 'Status informado nao encontrado.', 1;

		-- Iniciar a transacao --
        begin tran

        -- Atualizar o status --
        update Projetos
        set status_codigo = @status_codigo
        where codigo = @projeto_codigo;

		-- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select p.codigo as projeto_codigo, p.titulo, p.status_codigo, s.descricao as status_descricao
        from Projetos as p
        LEFT JOIN Status as s on s.codigo = p.status_codigo
        where p.codigo = @projeto_codigo;

    end try
    begin catch
		-- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

create or alter procedure CadastrarProjeto -- Não sei se é necessário neste contexto --
    @cliente_codigo int, @titulo varchar(50), @descricao varchar(MAX) = NULL,
    @data_inicio date = NULL, @data_fim date = NULL, @data_prevista date = NULL,
    @status_codigo int = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o titulo foi informado --
        if NULLIF(LTRIM(RTRIM(@titulo)), '') IS NULL
            throw 50001, 'Titulo do projeto e obrigatorio.', 1;

        -- Verificar se o cliente existe --
        if not exists (
            select 1 from Clientes where pessoa_codigo = @cliente_codigo
        )
            throw 50002, 'Cliente nao encontrado.', 1;

        -- Validar o status, se informado --
        if @status_codigo is not null and not exists (
               select 1 from Status where codigo = @status_codigo
           )
            throw 50003, 'Status informado nao encontrado.', 1;

        -- Validar as datas --
        if @data_inicio is not null and @data_fim is not null
           and @data_fim < @data_inicio
            throw 50004, 'A data final nao pode ser anterior a data inicial.', 1;

        if @data_inicio is not null and @data_prevista is not null
           and @data_prevista < @data_inicio
            throw 50005, 'A data prevista nao pode ser anterior a data inicial.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar o projeto --
        insert into Projetos
        (
            cliente_codigo, titulo, descricao, data_inicio,
            data_fim, data_prevista, status_codigo
        )
        values
        (
            @cliente_codigo, @titulo, @descricao, @data_inicio,
            @data_fim, @data_prevista, @status_codigo
        );

        -- Recuperar o codigo gerado --
        declare @projeto_codigo int;
        set @projeto_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do projeto cadastrado --
        select codigo, cliente_codigo, titulo, descricao, data_inicio,
               data_fim, data_prevista, status_codigo
        from Projetos
        where codigo = @projeto_codigo;

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

create or alter procedure CadastrarTarefa
    @projeto_codigo int, @categoria_codigo int = NULL,
    @titulo varchar(50), @descricao varchar(MAX) = NULL,
    @status varchar(30), @datahora_inicio datetime = NULL,
    @datahora_fim datetime = NULL, @datahora_prevista datetime = NULL,
    @visibilidade bit = 1
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o titulo foi informado --
        if NULLIF(LTRIM(RTRIM(@titulo)), '') IS NULL
            throw 50021, 'Titulo da tarefa e obrigatorio.', 1;

        -- Verificar se o status foi informado --
        if NULLIF(LTRIM(RTRIM(@status)), '') IS NULL
            throw 50022, 'Status da tarefa e obrigatorio.', 1;

        -- Verificar se o projeto existe --
        if not exists (
            select 1 from Projetos where codigo = @projeto_codigo
        )
            throw 50023, 'Projeto nao encontrado.', 1;

        -- Verificar se a categoria existe, se informada --
        if @categoria_codigo is not null and not exists (
            select 1 from Categorias where codigo = @categoria_codigo
        )
            throw 50024, 'Categoria nao encontrada.', 1;

        -- Validar as datas --
        if @datahora_inicio is not null and @datahora_fim is not null
           and @datahora_fim < @datahora_inicio
            throw 50025, 'A data final nao pode ser anterior a data inicial.', 1;

        if @datahora_inicio is not null and @datahora_prevista is not null
           and @datahora_prevista < @datahora_inicio
            throw 50026, 'A data prevista nao pode ser anterior a data inicial.', 1;

        -- Iniciar a transacao --
        begin tran;

        -- Cadastrar a tarefa --
        insert into Tarefas
        (
            projeto_codigo, categoria_codigo, titulo, descricao,
            status, datahora_inicio, datahora_fim,
            datahora_prevista, visibilidade
        )
        values
        (
            @projeto_codigo, @categoria_codigo, @titulo, @descricao,
            @status, @datahora_inicio, @datahora_fim,
            @datahora_prevista, @visibilidade
        );

        -- Recuperar o codigo gerado --
        declare @tarefa_codigo int;
        set @tarefa_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit;

        -- Retornar os dados da tarefa cadastrada --
        select codigo, projeto_codigo, categoria_codigo, titulo,
               descricao, status, datahora_inicio, datahora_fim,
               datahora_prevista, visibilidade
        from Tarefas
        where codigo = @tarefa_codigo;

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

create or alter procedure AtribuirFuncionarioTarefa
    @funcionario_codigo int, @tarefa_codigo int
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o funcionario existe --
        if not exists (
            select 1 from Funcionarios where pessoa_codigo = @funcionario_codigo
        )
            throw 50031, 'Funcionario nao encontrado.', 1;

        -- Verificar se a tarefa existe --
        if not exists (
            select 1 from Tarefas where codigo = @tarefa_codigo
        )
            throw 50032, 'Tarefa nao encontrada.', 1;

        -- Verificar se o funcionario ja esta atribuido a tarefa --
        if exists (
            select 1 from TarefaFuncionarios
            where funcionario_codigo = @funcionario_codigo
              and tarefa_codigo = @tarefa_codigo
        )
            throw 50033, 'Funcionario ja atribuido a esta tarefa.', 1;

        -- Iniciar a transacao --
        begin tran;

        -- Atribuir o funcionario a tarefa --
        insert into TarefaFuncionarios
        (
            funcionario_codigo, tarefa_codigo
        )
        values
        (
            @funcionario_codigo, @tarefa_codigo
        );

        -- Confirmar a transacao --
        commit;

        -- Retornar os dados da atribuicao --
        select tf.funcionario_codigo, tf.tarefa_codigo,
               p.nome as funcionario_nome, t.titulo as tarefa_titulo
        from TarefaFuncionarios as tf
        inner join Pessoas as p on p.codigo = tf.funcionario_codigo
        inner join Tarefas as t on t.codigo = tf.tarefa_codigo
        where tf.funcionario_codigo = @funcionario_codigo
          and tf.tarefa_codigo = @tarefa_codigo;

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw

    end catch
end
go

-- Testes --

-- Procedure 1 --

-- Funcionarios --
exec CadastrarPessoa
    @nome = 'João da Silva',
    @telefone = '17999999999',
    @documento = '12345678900',
    @tipo_doc = 'cpf',
    @tipo_pessoa = 'FUNCIONARIO',
    @cargo_codigo = 1,
    @ativo = 1,
    @email = 'joao@celeirocriativo.com',
    @senha = '123456';

select * from Pessoas
select * from Clientes
select * from Funcionarios

-- Clientes --
exec CadastrarPessoa
    @nome = 'Maria Oliveira',
    @telefone = '17988888888',
    @documento = '98765432100',
    @tipo_doc = 'cpf',
    @tipo_pessoa = 'CLIENTE',
    @email = 'maria@email.com',
    @senha = '123456',
    @codigo_verificacao = 'ABC123';
go

-- Procedure 2 --

exec AlterarStatusProjeto
    @projeto_codigo = 1,
    @status_codigo = 2;
go

select
    p.codigo,
    p.titulo,
    s.descricao as status_projeto
from Projetos as p
LEFT JOIN Status as s
    on s.codigo = p.status_codigo;
go

-- Procedure 3 --

select
    pessoa_codigo,
    email
from Clientes;

select
    codigo,
    descricao
from Status;

select *
from Projetos;

exec CadastrarProjeto
    @cliente_codigo = 6,
    @titulo = 'Site institucional',
    @descricao = 'Desenvolvimento do site da empresa.',
    @data_inicio = '2026-10-15',
    @data_fim = NULL,
    @data_prevista = '2026-11-15',
    @status_codigo = 1;
go

-- Procedure 4 --

select codigo, titulo
from Projetos;

select codigo, descricao
from Categorias;

select *
from Tarefas;


exec CadastrarTarefa
    @projeto_codigo = 3,
    @categoria_codigo = 2,
    @titulo = 'Criar pagina inicial',
    @descricao = 'Desenvolver a pagina inicial do site.',
    @status = 'pendente',
    @datahora_inicio = '20261015 13:00:00',
    @datahora_fim = NULL,
    @datahora_prevista = '20261020 18:00:00',
    @visibilidade = 1;
go

-- Procedure 5 --

exec AtribuirFuncionarioTarefa
    @funcionario_codigo = 1,
    @tarefa_codigo = 2;
go

select tf.funcionario_codigo, p.nome as funcionario_nome,
       tf.tarefa_codigo, t.titulo as tarefa_titulo
from TarefaFuncionarios as tf
inner join Pessoas as p on p.codigo = tf.funcionario_codigo
inner join Tarefas as t on t.codigo = tf.tarefa_codigo;
go