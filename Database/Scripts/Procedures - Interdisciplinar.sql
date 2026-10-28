use CeleiroCriativo
go

-- 1. Procedure para adicionar um cargo na tabela Cargos

create or alter procedure AdicionarCargo
    @descricao varchar(50)
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a descricao foi informada --
        if NULLIF(LTRIM(RTRIM(@descricao)), '') IS NULL
            throw 51001, 'Descricao do cargo e obrigatoria.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar o cargo --
        insert into Cargos (descricao)
        values (@descricao)

        -- Recuperar o codigo gerado --
        declare @cargo_codigo int
        set @cargo_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do cargo cadastrado --
        select codigo, descricao
        from Cargos
        where codigo = @cargo_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AdicionarCargo
    @descricao = 'Designer web'
go


-- 2. Procedure para atualizar um cargo na tabela Cargos

create or alter procedure AtualizarCargo
    @cargo_codigo int, @descricao varchar(50)
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o cargo existe --
        if not exists (select 1 from Cargos where codigo = @cargo_codigo)
            throw 51002, 'Cargo nao encontrado.', 1;

        -- Verificar se a descricao foi informada --
        if NULLIF(LTRIM(RTRIM(@descricao)), '') IS NULL
            throw 51003, 'Descricao do cargo e obrigatoria.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar o cargo --
        update Cargos
        set descricao = @descricao
        where codigo = @cargo_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select codigo, descricao
        from Cargos
        where codigo = @cargo_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarCargo
    @cargo_codigo = 6,
    @descricao = 'Designer gráfico'
go

-- 3. Procedure para adicionar um status na tabela Status

create or alter procedure AdicionarStatus
    @descricao varchar(50)
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a descricao foi informada --
        if NULLIF(LTRIM(RTRIM(@descricao)), '') IS NULL
            throw 51011, 'Descricao do status e obrigatoria.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar o status --
        insert into Status (descricao)
        values (@descricao)

        -- Recuperar o codigo gerado --
        declare @status_codigo int
        set @status_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do status cadastrado --
        select codigo, descricao
        from Status
        where codigo = @status_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AdicionarStatus
    @descricao = 'Em andamento';
go

-- 4. Procedure para alterar um status na tabela Status

create or alter procedure AlterarStatus
    @status_codigo int, @descricao varchar(50)
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o status existe --
        if not exists (select 1 from Status where codigo = @status_codigo)
            throw 51012, 'Status nao encontrado.', 1;

        -- Verificar se a descricao foi informada --
        if NULLIF(LTRIM(RTRIM(@descricao)), '') IS NULL
            throw 51013, 'Descricao do status e obrigatoria.', 1

        -- Iniciar a transacao --
        begin tran

        -- Atualizar o status --
        update Status
        set descricao = @descricao
        where codigo = @status_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select codigo, descricao
        from Status
        where codigo = @status_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AlterarStatus
    @status_codigo = 4,
    @descricao = 'Status atualizado'
go

-- 5. Procedure para adicionar uma categoria na tabela Categorias

create or alter procedure AdicionarCategoria
    @descricao varchar(50), @cor varchar(20) = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a descricao foi informada --
        if NULLIF(LTRIM(RTRIM(@descricao)), '') IS NULL
            throw 51021, 'Descricao da categoria e obrigatoria.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar a categoria --
        insert into Categorias (descricao, cor)
        values (@descricao, @cor)

        -- Recuperar o codigo gerado --
        declare @categoria_codigo int
        set @categoria_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit;

        -- Retornar os dados da categoria cadastrada --
        select codigo, descricao, cor
        from Categorias
        where codigo = @categoria_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AdicionarCategoria
    @descricao = 'Desenvolvimento',
    @cor = '#0000FF'
go


-- 6. Procedure para atualizar uma categoria na tabela Categorias

create or alter procedure AtualizarCategoria
    @categoria_codigo int, @descricao varchar(50), @cor varchar(20)
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a categoria existe --
        if not exists (select 1 from Categorias where codigo = @categoria_codigo)
            throw 51022, 'Categoria nao encontrada.', 1;

        -- Verificar se a descricao foi informada --
        if NULLIF(LTRIM(RTRIM(@descricao)), '') IS NULL
            throw 51023, 'Descricao da categoria e obrigatoria.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar a categoria --
        update Categorias
        set descricao = @descricao,
            cor = @cor
        where codigo = @categoria_codigo

        -- Confirmar a transacao --
        commit;

        -- Retornar os dados atualizados --
        select codigo, descricao, cor
        from Categorias
        where codigo = @categoria_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarCategoria
    @categoria_codigo = 4,
    @descricao = 'Desenvolvimento atualizado',
    @cor = '#FF0000'
go

-- 7. Procedure para cadastrar um funcionario na tabela Funcionarios

create or alter procedure CadastrarFuncionario
    @nome varchar(50), @documento varchar(20), @tipo_doc varchar(4),
    @cargo_codigo int, @email varchar(100), @senha varchar(255),
    @telefone varchar(20) = NULL, @ativo bit = 1
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o nome foi informado --
        if NULLIF(LTRIM(RTRIM(@nome)), '') IS NULL
            throw 51031, 'Nome do funcionario e obrigatorio.', 1;

        -- Verificar se o documento foi informado --
        if NULLIF(LTRIM(RTRIM(@documento)), '') IS NULL
            throw 51032, 'Documento do funcionario e obrigatorio.', 1;

        -- Validar o tipo de documento --
        if @tipo_doc is null or LOWER(LTRIM(RTRIM(@tipo_doc))) not in ('cpf', 'cnpj')
            throw 51033, 'Tipo de documento invalido.', 1;

        -- Verificar se o email foi informado --
        if NULLIF(LTRIM(RTRIM(@email)), '') IS NULL
            throw 51034, 'Email do funcionario e obrigatorio.', 1;

        -- Verificar se a senha foi informada --
        if NULLIF(LTRIM(RTRIM(@senha)), '') IS NULL
            throw 51035, 'Senha do funcionario e obrigatoria.', 1;

        -- Verificar se o cargo existe --
        if not exists (select 1 from Cargos where codigo = @cargo_codigo)
            throw 51036, 'Cargo nao encontrado.', 1;

        -- Normalizar o tipo de documento --
        set @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)));

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar os dados pessoais --
        insert into Pessoas (nome, telefone, documento, tipo_doc)
        values (@nome, @telefone, @documento, @tipo_doc)

        -- Recuperar o codigo da pessoa --
        declare @funcionario_codigo int
        set @funcionario_codigo = SCOPE_IDENTITY();

        -- Cadastrar os dados do funcionario --
        insert into Funcionarios (pessoa_codigo, cargo_codigo, ativo, email, senha)
        values (@funcionario_codigo, @cargo_codigo, @ativo, @email, @senha)

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do funcionario cadastrado --
        select p.codigo as pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, f.cargo_codigo,
               f.ativo, f.email
        from Pessoas as p
        inner join Funcionarios as f on f.pessoa_codigo = p.codigo
        where p.codigo = @funcionario_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec CadastrarFuncionario
    @nome = 'Joao Silva',
    @documento = '12345678901',
    @tipo_doc = 'cpf',
    @cargo_codigo = 6,
    @email = 'joao@teste.com',
    @senha = '123456',
    @telefone = '17999999999',
    @ativo = 1
go

-- 8. Procedure para atualizar um funcionario na tabela Funcionarios

create or alter procedure AtualizarFuncionario
    @funcionario_codigo int, @nome varchar(50), @telefone varchar(20),
    @documento varchar(20), @tipo_doc varchar(4), @cargo_codigo int,
    @ativo bit, @email varchar(100), @senha varchar(255) = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o funcionario existe --
        if not exists (select 1 from Funcionarios where pessoa_codigo = @funcionario_codigo)
            throw 51037, 'Funcionario nao encontrado.', 1;

        -- Verificar se o nome foi informado --
        if NULLIF(LTRIM(RTRIM(@nome)), '') IS NULL
            throw 51038, 'Nome do funcionario e obrigatorio.', 1;

        -- Verificar se o documento foi informado --
        if NULLIF(LTRIM(RTRIM(@documento)), '') IS NULL
            throw 51039, 'Documento do funcionario e obrigatorio.', 1;

        -- Validar o tipo de documento --
        if @tipo_doc is null or LOWER(LTRIM(RTRIM(@tipo_doc))) not in ('cpf', 'cnpj')
            throw 51040, 'Tipo de documento invalido.', 1;

        -- Verificar se o cargo existe --
        if not exists (select 1 from Cargos where codigo = @cargo_codigo)
            throw 51041, 'Cargo nao encontrado.', 1;

        -- Verificar se o email foi informado --
        if NULLIF(LTRIM(RTRIM(@email)), '') IS NULL
            throw 51042, 'Email do funcionario e obrigatorio.', 1;

        -- Validar o campo ativo --
        if @ativo is null
            throw 51043, 'O campo ativo deve ser informado.', 1;

        -- Normalizar o tipo de documento --
        set @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)));

        -- Iniciar a transacao --
        begin tran

        -- Atualizar os dados pessoais --
        update Pessoas
        set nome = @nome,
            telefone = @telefone,
            documento = @documento,
            tipo_doc = @tipo_doc
        where codigo = @funcionario_codigo

        -- Atualizar os dados do funcionario --
        update Funcionarios
        set cargo_codigo = @cargo_codigo,
            ativo = @ativo,
            email = @email,
            senha = COALESCE(NULLIF(@senha, ''), senha)
        where pessoa_codigo = @funcionario_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select p.codigo as pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, f.cargo_codigo,
               f.ativo, f.email
        from Pessoas as p
        inner join Funcionarios as f on f.pessoa_codigo = p.codigo
        where p.codigo = @funcionario_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarFuncionario
    @funcionario_codigo = 13,
    @nome = 'Maria Dantas',
    @telefone = '17988888888',
    @documento = '12345678901',
    @tipo_doc = 'cpf',
    @cargo_codigo = 6,
    @ativo = 1,
    @email = 'mariadantas@teste.com',
    @senha = NULL
go

-- 9. Procedure para cadastrar um cliente na tabela Clientes

create or alter procedure CadastrarCliente
    @nome varchar(50), @documento varchar(20), @tipo_doc varchar(4),
    @email varchar(100), @telefone varchar(20) = NULL,
    @codigo_verificacao varchar(20) = NULL, @senha varchar(255) = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o nome foi informado --
        if NULLIF(LTRIM(RTRIM(@nome)), '') IS NULL
            throw 51051, 'Nome do cliente e obrigatorio.', 1;

        -- Verificar se o documento foi informado --
        if NULLIF(LTRIM(RTRIM(@documento)), '') IS NULL
            throw 51052, 'Documento do cliente e obrigatorio.', 1;

        -- Validar o tipo de documento --
        if @tipo_doc is null or LOWER(LTRIM(RTRIM(@tipo_doc))) not in ('cpf', 'cnpj')
            throw 51053, 'Tipo de documento invalido.', 1;

        -- Verificar se o email foi informado --
        if NULLIF(LTRIM(RTRIM(@email)), '') IS NULL
            throw 51054, 'Email do cliente e obrigatorio.', 1;

        -- Normalizar o tipo de documento --
        set @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)))

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar os dados pessoais --
        insert into Pessoas (nome, telefone, documento, tipo_doc)
        values (@nome, @telefone, @documento, @tipo_doc)

        -- Recuperar o codigo da pessoa --
        declare @cliente_codigo int;
        set @cliente_codigo = SCOPE_IDENTITY();

        -- Cadastrar os dados do cliente --
        insert into Clientes (pessoa_codigo, codigo_verificacao, email, senha)
        values (@cliente_codigo, @codigo_verificacao, @email, @senha)

        -- Confirmar a transacao --
        commit;

        -- Retornar os dados do cliente cadastrado --
        select p.codigo as pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, c.codigo_verificacao,
               c.email
        from Pessoas as p
        inner join Clientes as c on c.pessoa_codigo = p.codigo
        where p.codigo = @cliente_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw
    end catch
end
go

-- Teste de execucao

exec CadastrarCliente
    @nome = 'Maria Oliveira',
    @documento = '98765432100',
    @tipo_doc = 'cpf',
    @email = 'maria@teste.com',
    @telefone = '17999999999',
    @codigo_verificacao = 'ABC123',
    @senha = '123456'
go


-- 10. Procedure para atualizar um cliente na tabela Clientes

create or alter procedure AtualizarCliente
    @cliente_codigo int, @nome varchar(50), @telefone varchar(20),
    @documento varchar(20), @tipo_doc varchar(4),
    @email varchar(100), @codigo_verificacao varchar(20),
    @senha varchar(255) = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o cliente existe --
        if not exists (select 1 from Clientes where pessoa_codigo = @cliente_codigo)
            throw 51055, 'Cliente nao encontrado.', 1;

        -- Verificar se o nome foi informado --
        if NULLIF(LTRIM(RTRIM(@nome)), '') IS NULL
            throw 51056, 'Nome do cliente e obrigatorio.', 1;

        -- Verificar se o documento foi informado --
        if NULLIF(LTRIM(RTRIM(@documento)), '') IS NULL
            throw 51057, 'Documento do cliente e obrigatorio.', 1;

        -- Validar o tipo de documento --
        if @tipo_doc is null or LOWER(LTRIM(RTRIM(@tipo_doc))) not in ('cpf', 'cnpj')
            throw 51058, 'Tipo de documento invalido.', 1;

        -- Verificar se o email foi informado --
        if NULLIF(LTRIM(RTRIM(@email)), '') IS NULL
            throw 51059, 'Email do cliente e obrigatorio.', 1;

        -- Normalizar o tipo de documento --
        set @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)));

        -- Iniciar a transacao --
        begin tran

        -- Atualizar os dados pessoais --
        update Pessoas
        set nome = @nome,
            telefone = @telefone,
            documento = @documento,
            tipo_doc = @tipo_doc
        where codigo = @cliente_codigo

        -- Atualizar os dados do cliente --
        update Clientes
        set codigo_verificacao = @codigo_verificacao,
            email = @email,
            senha = COALESCE(@senha, senha)
        where pessoa_codigo = @cliente_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select p.codigo as pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, c.codigo_verificacao,
               c.email
        from Pessoas as p
        inner join Clientes as c on c.pessoa_codigo = p.codigo
        where p.codigo = @cliente_codigo

    end try
    begin catch

        -- Desfazer a transacao caso ocorra um erro --
        if XACT_STATE() <> 0
            rollback;

        throw 
    end catch
end
go

-- Teste de execucao

exec AtualizarCliente
    @cliente_codigo = 14,
    @nome = 'Joao Mario',
    @telefone = '17988888888',
    @documento = '98765432100',
    @tipo_doc = 'cpf',
    @email = 'joaomario@teste.com',
    @codigo_verificacao = 'XYZ789',
    @senha = NULL
go

-- 11. Procedure para cadastrar um projeto na tabela Projetos

create or alter procedure CadastrarProjeto
    @cliente_codigo int, @titulo varchar(50),
    @descricao varchar(MAX) = NULL,
    @data_inicio date = NULL, @data_fim date = NULL,
    @data_prevista date = NULL, @status_codigo int = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o titulo foi informado --
        if NULLIF(LTRIM(RTRIM(@titulo)), '') IS NULL
            throw 51101, 'Titulo do projeto e obrigatorio.', 1;

        -- Verificar se o cliente existe --
        if not exists (select 1 from Clientes where pessoa_codigo = @cliente_codigo)
            throw 51102, 'Cliente nao encontrado.', 1;

        -- Validar o status, se informado --
        if @status_codigo is not null and not exists (select 1 from Status where codigo = @status_codigo)
            throw 51103, 'Status informado nao encontrado.', 1;

        -- Validar as datas --
        if @data_inicio is not null and @data_fim is not null
           and @data_fim < @data_inicio
            throw 51104, 'A data final nao pode ser anterior a data inicial.', 1;

        if @data_inicio is not null and @data_prevista is not null
           and @data_prevista < @data_inicio
            throw 51105, 'A data prevista nao pode ser anterior a data inicial.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar o projeto --
        insert into Projetos (cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo)
		values (@cliente_codigo, @titulo, @descricao, @data_inicio,@data_fim, @data_prevista, @status_codigo)

        -- Recuperar o codigo gerado --
        declare @projeto_codigo int
        set @projeto_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do projeto cadastrado --
        select codigo, cliente_codigo, titulo, descricao,
               data_inicio, data_fim, data_prevista, status_codigo
        from Projetos
        where codigo = @projeto_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go


-- Teste de execucao

exec CadastrarProjeto
    @cliente_codigo = 14,
    @titulo = 'Projeto de teste',
    @descricao = 'Desenvolvimento de um site',
    @data_inicio = '20261027',
    @data_fim = NULL,
    @data_prevista = '20261110',
    @status_codigo = 1
go


-- 12. Procedure para atualizar um projeto na tabela Projetos

create or alter procedure AtualizarProjeto
    @projeto_codigo int, @cliente_codigo int, @titulo varchar(50),
    @descricao varchar(MAX) = NULL, @data_inicio date = NULL,
    @data_fim datetime = NULL, @data_prevista datetime = NULL,
    @status_codigo int = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o projeto existe --
        if not exists (select 1 from Projetos where codigo = @projeto_codigo)
            throw 51106, 'Projeto nao encontrado.', 1;

        -- Verificar se o cliente existe --
        if not exists (select 1 from Clientes where pessoa_codigo = @cliente_codigo)
            throw 51107, 'Cliente nao encontrado.', 1;

        -- Verificar se o titulo foi informado --
        if NULLIF(LTRIM(RTRIM(@titulo)), '') IS NULL
            throw 51108, 'Titulo do projeto e obrigatorio.', 1;

        -- Validar o status, se informado --
        if @status_codigo is not null and not exists (select 1 from Status where codigo = @status_codigo)
            throw 51109, 'Status informado nao encontrado.', 1;

        -- Validar as datas --
        if @data_inicio is not null and @data_fim is not null
           and @data_fim < @data_inicio
            throw 51110, 'A data final nao pode ser anterior a data inicial.', 1;

        if @data_inicio is not null and @data_prevista is not null
           and @data_prevista < @data_inicio
            throw 51111, 'A data prevista nao pode ser anterior a data inicial.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar o projeto --
        update Projetos
        set cliente_codigo = @cliente_codigo,
            titulo = @titulo,
            descricao = @descricao,
            data_inicio = @data_inicio,
            data_fim = @data_fim,
            data_prevista = @data_prevista,
            status_codigo = @status_codigo
        where codigo = @projeto_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select codigo, cliente_codigo, titulo, descricao,
               data_inicio, data_fim, data_prevista, status_codigo
        from Projetos
        where codigo = @projeto_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarProjeto
    @projeto_codigo = 7,
    @cliente_codigo = 14,
    @titulo = 'Projeto atualizado',
    @descricao = 'Desenvolvimento atualizado',
    @data_inicio = '2026-10-27 11:00:00',
    @data_fim = NULL,
    @data_prevista = '2026-11-15 13:00:00',
    @status_codigo = 1
go

-- 13. Procedure para adicionar uma tarefa na tabela Tarefas

create or alter procedure AdicionarTarefa
    @projeto_codigo int, @categoria_codigo int = NULL,
    @titulo varchar(50), @descricao varchar(MAX) = NULL,
    @status varchar(30), @datahora_inicio datetime = NULL,
    @datahora_fim datetime = NULL,
    @datahora_prevista datetime = NULL, @visibilidade bit = 1
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o titulo foi informado --
        if NULLIF(LTRIM(RTRIM(@titulo)), '') IS NULL
            throw 51121, 'Titulo da tarefa e obrigatorio.', 1;

        -- Verificar se o status foi informado --
        if NULLIF(LTRIM(RTRIM(@status)), '') IS NULL
            throw 51122, 'Status da tarefa e obrigatorio.', 1;

        -- Verificar se o projeto existe --
        if not exists (select 1 from Projetos where codigo = @projeto_codigo)
            throw 51123, 'Projeto nao encontrado.', 1;

        -- Validar a categoria, se informada --
        if @categoria_codigo is not null and not exists (select 1 from Categorias where codigo = @categoria_codigo)
            throw 51124, 'Categoria nao encontrada.', 1;

        -- Validar as datas --
        if @datahora_inicio is not null and @datahora_fim is not null
           and @datahora_fim < @datahora_inicio
            throw 51125, 'A data final nao pode ser anterior a data inicial.', 1;

        if @datahora_inicio is not null and @datahora_prevista is not null
           and @datahora_prevista < @datahora_inicio
            throw 51126, 'A data prevista nao pode ser anterior a data inicial.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar a tarefa --
        insert into Tarefas (projeto_codigo, categoria_codigo, titulo, descricao, status, 
		datahora_inicio, datahora_fim,datahora_prevista, visibilidade)
        values(@projeto_codigo, @categoria_codigo, @titulo, @descricao, @status, 
		@datahora_inicio, @datahora_fim,@datahora_prevista, @visibilidade)

        -- Recuperar o codigo gerado --
        declare @tarefa_codigo int
        set @tarefa_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados da tarefa cadastrada --
        select codigo, projeto_codigo, categoria_codigo, titulo,
               descricao, status, datahora_inicio, datahora_fim,
               datahora_prevista, visibilidade
        from Tarefas
        where codigo = @tarefa_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AdicionarTarefa
    @projeto_codigo = 7,
    @categoria_codigo = 6,
    @titulo = 'Criar pagina inicial',
    @descricao = 'Desenvolver a pagina inicial do site',
    @status = 'Pendente',
    @datahora_inicio = '2026-10-27 13:00:00',
    @datahora_fim = NULL,
    @datahora_prevista = '2026-11-05 18:00:00',
    @visibilidade = 1
go

-- 14. Procedure para atualizar uma tarefa na tabela Tarefas

create or alter procedure AtualizarTarefa
    @tarefa_codigo int, @projeto_codigo int,
    @categoria_codigo int = NULL, @titulo varchar(50),
    @descricao varchar(MAX) = NULL, @status varchar(30),
    @datahora_inicio datetime = NULL, @datahora_fim datetime = NULL,
    @datahora_prevista datetime = NULL, @visibilidade bit = 1
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a tarefa existe --
        if not exists (select 1 from Tarefas where codigo = @tarefa_codigo)
            throw 51127, 'Tarefa nao encontrada.', 1;

        -- Verificar se o projeto existe --
        if not exists (select 1 from Projetos where codigo = @projeto_codigo)
            throw 51128, 'Projeto nao encontrado.', 1;

        -- Verificar se o titulo foi informado --
        if NULLIF(LTRIM(RTRIM(@titulo)), '') IS NULL
            throw 51129, 'Titulo da tarefa e obrigatorio.', 1;

        -- Verificar se o status foi informado --
        if NULLIF(LTRIM(RTRIM(@status)), '') IS NULL
            throw 51130, 'Status da tarefa e obrigatorio.', 1;

        -- Validar a categoria, se informada --
        if @categoria_codigo is not null and not exists (select 1 from Categorias where codigo = @categoria_codigo)
            throw 51131, 'Categoria nao encontrada.', 1;

        -- Validar as datas --
        if @datahora_inicio is not null and @datahora_fim is not null
           and @datahora_fim < @datahora_inicio
            throw 51132, 'A data final nao pode ser anterior a data inicial.', 1;

        if @datahora_inicio is not null and @datahora_prevista is not null
           and @datahora_prevista < @datahora_inicio
            throw 51133, 'A data prevista nao pode ser anterior a data inicial.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar a tarefa --
        update Tarefas
        set projeto_codigo = @projeto_codigo,
            categoria_codigo = @categoria_codigo,
            titulo = @titulo,
            descricao = @descricao,
            status = @status,
            datahora_inicio = @datahora_inicio,
            datahora_fim = @datahora_fim,
            datahora_prevista = @datahora_prevista,
            visibilidade = @visibilidade
        where codigo = @tarefa_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select codigo, projeto_codigo, categoria_codigo, titulo,
               descricao, status, datahora_inicio, datahora_fim,
               datahora_prevista, visibilidade
        from Tarefas
        where codigo = @tarefa_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarTarefa
    @tarefa_codigo = 11,
    @projeto_codigo = 7,
    @categoria_codigo = 6,
    @titulo = 'Criar pagina inicial atualizada',
    @descricao = 'Desenvolvimento atualizado',
    @status = 'Em andamento',
    @datahora_inicio = '2026-10-27 13:00:00',
    @datahora_fim = NULL,
    @datahora_prevista = '2026-11-08 18:00:00',
    @visibilidade = 1
go

-- 15. Procedure para atribuir uma tarefa a um funcionario na tabela TarefaFuncionario

create or alter procedure AtribuirFuncionarioTarefa
    @funcionario_codigo int, @tarefa_codigo int
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o funcionario existe --
        if not exists (select 1 from Funcionarios
            where pessoa_codigo = @funcionario_codigo)
            throw 51141, 'Funcionario nao encontrado.', 1;

        -- Verificar se a tarefa existe --
        if not exists (select 1 from Tarefas where codigo = @tarefa_codigo)
            throw 51142, 'Tarefa nao encontrada.', 1;

        -- Verificar se a atribuicao ja existe --
        if exists (select 1 from TarefaFuncionarios
            where funcionario_codigo = @funcionario_codigo
              and tarefa_codigo = @tarefa_codigo)
            throw 51143, 'Essa tarefa ja esta atribuida ao funcionario.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atribuir a tarefa ao funcionario --
        insert into TarefaFuncionarios (funcionario_codigo, tarefa_codigo)
        values (@funcionario_codigo, @tarefa_codigo)

        -- Confirmar a transacao --
        commit

        -- Retornar a atribuicao cadastrada --
        select funcionario_codigo, tarefa_codigo
        from TarefaFuncionarios
        where funcionario_codigo = @funcionario_codigo
          and tarefa_codigo = @tarefa_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AtribuirFuncionarioTarefa
    @funcionario_codigo = 13,
    @tarefa_codigo = 6
go

-- 16. Procedure para atualizar a atribuição de uma tarefa a um funcionario na tabela TarefaFuncionario

create or alter procedure AtualizarAtribuicaoFuncionarioTarefa
    @funcionario_codigo_atual int, @tarefa_codigo_atual int,
    @funcionario_codigo_novo int, @tarefa_codigo_novo int
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a atribuicao atual existe --
        if not exists (select 1 from TarefaFuncionarios where funcionario_codigo = @funcionario_codigo_atual 
			           and tarefa_codigo = @tarefa_codigo_atual)
            throw 51144, 'Atribuicao atual nao encontrada.', 1;

        -- Verificar se o novo funcionario existe --
        if not exists (select 1 from Funcionarios where pessoa_codigo = @funcionario_codigo_novo)
            throw 51145, 'Novo funcionario nao encontrado.', 1;

        -- Verificar se a nova tarefa existe --
        if not exists (select 1 from Tarefas where codigo = @tarefa_codigo_novo)
            throw 51146, 'Nova tarefa nao encontrada.', 1;

        -- Verificar se a nova atribuicao ja existe --
        if (@funcionario_codigo_atual <> @funcionario_codigo_novo or @tarefa_codigo_atual <> @tarefa_codigo_novo) 
			and exists (select 1 from TarefaFuncionarios where funcionario_codigo = @funcionario_codigo_novo 
			and tarefa_codigo = @tarefa_codigo_novo)
            throw 51147, 'A nova atribuicao ja existe.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar a atribuicao --
        update TarefaFuncionarios
        set funcionario_codigo = @funcionario_codigo_novo,
            tarefa_codigo = @tarefa_codigo_novo
        where funcionario_codigo = @funcionario_codigo_atual
          and tarefa_codigo = @tarefa_codigo_atual

        -- Confirmar a transacao --
        commit

        -- Retornar a atribuicao atualizada --
        select funcionario_codigo, tarefa_codigo
        from TarefaFuncionarios
        where funcionario_codigo = @funcionario_codigo_novo
          and tarefa_codigo = @tarefa_codigo_novo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarAtribuicaoFuncionarioTarefa
    @funcionario_codigo_atual = 13,
    @tarefa_codigo_atual = 6,
    @funcionario_codigo_novo = 11,
    @tarefa_codigo_novo = 6
go


-- 17. Procedure para adicionar um documento na tabela Documentos

create or alter procedure AdicionarDocumento
    @tarefa_codigo int, @descricao varchar(MAX) = NULL,
    @extensao varchar(10) = NULL, @diretorio varchar(255) = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a tarefa existe --
        if not exists (select 1 from Tarefas where codigo = @tarefa_codigo)
            throw 51151, 'Tarefa nao encontrada.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar o documento --
        insert into Documentos (tarefa_codigo, descricao, extensao, diretorio)
		values (@tarefa_codigo, @descricao, @extensao, @diretorio)

        -- Recuperar o codigo gerado --
        declare @documento_codigo int
        set @documento_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do documento cadastrado --
        select codigo, tarefa_codigo, descricao, extensao, diretorio
        from Documentos
        where codigo = @documento_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AdicionarDocumento
    @tarefa_codigo = 1,
    @descricao = 'Documento de teste',
    @extensao = 'pdf',
    @diretorio = '/documentos/teste.pdf';
go

-- 18. Procedure para atualizar um documento na tabela Documentos

create or alter procedure AtualizarDocumento
    @documento_codigo int, @tarefa_codigo int,
    @descricao varchar(MAX) = NULL,
    @extensao varchar(10) = NULL, @diretorio varchar(255) = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o documento existe --
        if not exists (select 1 from Documentos where codigo = @documento_codigo)
            throw 51152, 'Documento nao encontrado.', 1;

        -- Verificar se a tarefa existe --
        if not exists (select 1 from Tarefas where codigo = @tarefa_codigo)
            throw 51153, 'Tarefa nao encontrada.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar o documento --
        update Documentos
        set tarefa_codigo = @tarefa_codigo,
            descricao = @descricao,
            extensao = @extensao,
            diretorio = @diretorio
        where codigo = @documento_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select codigo, tarefa_codigo, descricao, extensao, diretorio
        from Documentos
        where codigo = @documento_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarDocumento
    @documento_codigo = 1,
    @tarefa_codigo = 1,
    @descricao = 'Documento atualizado',
    @extensao = 'pdf',
    @diretorio = '/documentos/atualizado.pdf'
go

-- 19. Procedure para adicionar um comentario na tabela Comentarios

create or alter procedure AdicionarComentario
    @tarefa_codigo int, @funcionario_codigo int,
    @titulo varchar(50) = NULL, @descricao varchar(MAX) = NULL,
    @data_hora datetime = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se a tarefa existe --
        if not exists (select 1 from Tarefas where codigo = @tarefa_codigo)
            throw 51161, 'Tarefa nao encontrada.', 1;

        -- Verificar se o funcionario esta atribuido a tarefa --
        if not exists (select 1 from TarefaFuncionarios where funcionario_codigo = @funcionario_codigo 
		               and tarefa_codigo = @tarefa_codigo)
            throw 51162, 'O funcionario nao esta atribuido a essa tarefa.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Cadastrar o comentario --
        insert into Comentarios (tarefa_codigo, funcionario_codigo, descricao, data_hora)
		values (@tarefa_codigo, @funcionario_codigo, @descricao, COALESCE(@data_hora, GETDATE()))

        -- Recuperar o codigo gerado --
        declare @comentario_codigo int
        set @comentario_codigo = SCOPE_IDENTITY();

        -- Confirmar a transacao --
        commit

        -- Retornar os dados do comentario cadastrado --
        select codigo, tarefa_codigo, funcionario_codigo,
               descricao, data_hora
        from Comentarios
        where codigo = @comentario_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AdicionarComentario
    @tarefa_codigo = 6,
    @funcionario_codigo = 11,
    @titulo = 'Comentario de teste',
    @descricao = 'Comentario sobre a tarefa'
go

-- 20. Procedure para atualizar um comentario na tabela Comentarios

create or alter procedure AtualizarComentario
    @comentario_codigo int, @tarefa_codigo int,
    @funcionario_codigo int, @titulo varchar(50) = NULL,
    @descricao varchar(MAX) = NULL, @data_hora datetime = NULL
as
begin
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    begin try

        -- Verificar se o comentario existe --
        if not exists (select 1 from Comentarios where codigo = @comentario_codigo)
            throw 51163, 'Comentario nao encontrado.', 1;

        -- Verificar se o funcionario esta atribuido a tarefa --
        if not exists (select 1 from TarefaFuncionarios 
		where funcionario_codigo = @funcionario_codigo 
		and tarefa_codigo = @tarefa_codigo)
            throw 51164, 'O funcionario nao esta atribuido a essa tarefa.', 1;

        -- Iniciar a transacao --
        begin tran

        -- Atualizar o comentario --
        update Comentarios
        set tarefa_codigo = @tarefa_codigo,
            funcionario_codigo = @funcionario_codigo,
            descricao = @descricao,
            data_hora = COALESCE(@data_hora, data_hora)
        where codigo = @comentario_codigo

        -- Confirmar a transacao --
        commit

        -- Retornar os dados atualizados --
        select codigo, tarefa_codigo, funcionario_codigo,
               descricao, data_hora
        from Comentarios
        where codigo = @comentario_codigo

    end try
    begin catch
        if XACT_STATE() <> 0
            rollback;
        throw
    end catch
end
go

-- Teste de execucao

exec AtualizarComentario
    @comentario_codigo = 1,
    @tarefa_codigo = 6,
    @funcionario_codigo = 11,
    @titulo = 'Comentario atualizado',
    @descricao = 'Descricao atualizada'
go