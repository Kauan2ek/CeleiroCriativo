-- =====================================================================
-- CELEIRO CRIATIVO - SCRIPT COMPLETO DO BANCO (SQL Server)
-- Abra no SSMS / Azure Data Studio e aperte F5 UMA vez: recria o banco
-- CeleiroCriativo do zero, com tabelas, functions, views, procedures e
-- dados de teste.
--
-- !!! ATENÇÃO: APAGA o banco CeleiroCriativo se ele já existir !!!
--     (as FKs dos inserts dependem do IDENTITY começando em 1)
--
-- ---------------------------------------------------------------------
-- MANTENHA ESTE ARQUIVO ATUALIZADO
-- ---------------------------------------------------------------------
-- Este arquivo é a junção dos scripts abaixo, nesta ordem. Sempre que
-- um deles mudar (ou surgir um script novo), copie a mudança para cá
-- também e atualize a data de "Última sincronização".
--
--   1) Celeiro_Criativo_DDL_v4.sql      -> tabelas
--   2) Functions.sql                    -> functions
--   3) Views.sql                        -> views
--   4) Procedures.sql                   -> procedures (sem os testes do fim)
--   5) Celeiro_Criativo_Inserts_v4.sql  -> dados de teste
--
--   Fora daqui: Celeiro_Criativo_DDL_v3.sql (versão antiga) e
--   Atualizacao_CRUD_Gestor_20261016.sql (já incluído nos itens 3 e 5;
--   serve só para atualizar um banco existente sem recriá-lo).
--
-- Última sincronização: 22/10/2026
-- =====================================================================

USE master;
GO

IF DB_ID('CeleiroCriativo') IS NOT NULL
BEGIN
    -- Derruba conexões abertas (ex.: a aplicação rodando) antes de apagar
    ALTER DATABASE CeleiroCriativo SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CeleiroCriativo;
END;
GO

CREATE DATABASE CeleiroCriativo;
GO

USE CeleiroCriativo;
GO


-- #####################################################################
-- 1) TABELAS (Celeiro_Criativo_DDL_v4.sql)
-- #####################################################################

-- ---------------------------------------------------------------------
-- TABELAS SEM DEPENDÊNCIA
-- ---------------------------------------------------------------------

CREATE TABLE Pessoas (
    codigo          INT IDENTITY(1,1) PRIMARY KEY,
    nome            VARCHAR(50) NOT NULL,
    telefone        VARCHAR(20) NULL,
    documento       VARCHAR(20) NOT NULL,
    tipo_doc        VARCHAR(4) NOT NULL,
    CONSTRAINT CK_Pessoa_TipoDoc CHECK (tipo_doc IN ('cpf', 'cnpj'))
);

CREATE TABLE Cargos (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(50) NOT NULL
);

-- Status é usado apenas por Projetos (o status da tarefa é a coluna Tarefas.status).
CREATE TABLE Status (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(50) NOT NULL
);

CREATE TABLE Categorias (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(50) NOT NULL,
    cor         VARCHAR(20)     NULL
);

-- ---------------------------------------------------------------------
-- SUBTIPOS DE PESSOA
-- ---------------------------------------------------------------------

CREATE TABLE Funcionarios (
    pessoa_codigo   INT PRIMARY KEY,
    cargo_codigo    INT NOT NULL,
    ativo           BIT NOT NULL DEFAULT 1,
    email           VARCHAR(100) NOT NULL,
    senha           VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Funcionarios_Pessoa FOREIGN KEY (pessoa_codigo) REFERENCES Pessoas(codigo),
    CONSTRAINT FK_Funcionarios_Cargo  FOREIGN KEY (cargo_codigo)  REFERENCES Cargos(codigo)
);

CREATE TABLE Clientes (
    pessoa_codigo       INT PRIMARY KEY,
    codigo_verificacao  VARCHAR(20) NULL,
    email               VARCHAR(100) NOT NULL,
    senha               VARCHAR(255) NULL, -- nullable no banco, mas o cadastro atual já permite salvar a senha
    CONSTRAINT FK_Clientes_Pessoa FOREIGN KEY (pessoa_codigo) REFERENCES Pessoas(codigo)
);

-- ---------------------------------------------------------------------
-- PROJETOS (depende de Clientes e Status)
-- ---------------------------------------------------------------------

CREATE TABLE Projetos (
    codigo          INT IDENTITY(1,1) PRIMARY KEY,
    cliente_codigo  INT NOT NULL,
    titulo          VARCHAR(50) NOT NULL,
    descricao       VARCHAR(MAX) NULL,
    data_inicio     DATE NULL,
    data_fim        DATE NULL,
    data_prevista   DATE NULL,
    status_codigo   INT NULL,
    CONSTRAINT FK_Projetos_Cliente       FOREIGN KEY (cliente_codigo) REFERENCES Clientes(pessoa_codigo),
    CONSTRAINT FK_Projetos_Status        FOREIGN KEY (status_codigo)  REFERENCES Status(codigo),
    CONSTRAINT CK_Projetos_data_fim      CHECK (data_fim >= data_inicio),
    CONSTRAINT CK_Projetos_data_prevista CHECK (data_prevista >= data_inicio)
);

-- ---------------------------------------------------------------------
-- TAREFAS (depende de Projetos e Categorias)
-- ---------------------------------------------------------------------

CREATE TABLE Tarefas (
    codigo              INT IDENTITY(1,1) PRIMARY KEY,
    projeto_codigo      INT NOT NULL,
    categoria_codigo    INT NOT NULL,
    titulo              VARCHAR(50) NOT NULL,
    descricao           VARCHAR(MAX) NOT NULL,
    status              VARCHAR(30) NOT NULL,
    datahora_inicio     DATETIME NULL,
    datahora_fim        DATETIME NULL,
    datahora_prevista   DATETIME NULL,
    visibilidade        BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Tarefas_Projeto   FOREIGN KEY (projeto_codigo)   REFERENCES Projetos(codigo),
    CONSTRAINT FK_Tarefas_Categoria FOREIGN KEY (categoria_codigo) REFERENCES Categorias(codigo)
);

-- ---------------------------------------------------------------------
-- TAREFAFUNCIONARIOS (entidade associativa: Funcionário x Tarefa)
-- ---------------------------------------------------------------------

CREATE TABLE TarefaFuncionarios (
    funcionario_codigo  INT NOT NULL,
    tarefa_codigo       INT NOT NULL,
    --responsavel         BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_TarefaFuncionarios PRIMARY KEY (funcionario_codigo, tarefa_codigo),
    CONSTRAINT FK_TarefaFuncionarios_Funcionario FOREIGN KEY (funcionario_codigo) REFERENCES Funcionarios(pessoa_codigo),
    CONSTRAINT FK_TarefaFuncionarios_Tarefa      FOREIGN KEY (tarefa_codigo)      REFERENCES Tarefas(codigo)
);

-- ---------------------------------------------------------------------
-- DOCUMENTOS (depende de Tarefas)
-- ---------------------------------------------------------------------

CREATE TABLE Documentos (
    codigo          INT IDENTITY(1,1) PRIMARY KEY,
    tarefa_codigo   INT NOT NULL,
    descricao       VARCHAR(MAX) NOT NULL,
    extensao        VARCHAR(10) NOT NULL, -- ex.: 'pdf', 'docx', 'png'
    diretorio       VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Documentos_Tarefa FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo)
);

-- ---------------------------------------------------------------------
-- COMENTARIOS (depende de Tarefas e TarefaFuncionarios)
-- A FK composta garante que só quem está alocado na tarefa comenta nela.
-- ---------------------------------------------------------------------

CREATE TABLE Comentarios (
    codigo               INT IDENTITY(1,1) PRIMARY KEY,
    tarefa_codigo        INT NOT NULL,
    funcionario_codigo   INT NOT NULL,
    --titulo               VARCHAR(50) NULL,
    descricao            VARCHAR(MAX) NOT NULL,
    data_hora            DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Comentarios_Tarefa            FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo),
    CONSTRAINT FK_Comentarios_TarefaFuncionario FOREIGN KEY (funcionario_codigo, tarefa_codigo)
        REFERENCES TarefaFuncionarios(funcionario_codigo, tarefa_codigo)
);
GO


-- #####################################################################
-- 2) FUNCTIONS (Functions.sql)
-- #####################################################################

CREATE OR ALTER FUNCTION dbo.F_FuncionariosDaTarefa (@tarefa INT)
RETURNS TABLE
AS
RETURN
(
	SELECT A.funcionario_codigo codigo, C.nome, B.email, B.senha,
		   C.telefone, C.tipo_doc tipo_documento, C.documento, B.ativo,
		   B.cargo_codigo, D.descricao cargo_descricao
	FROM TarefaFuncionarios A
	INNER JOIN Funcionarios B ON A.funcionario_codigo = B.pessoa_codigo
	INNER JOIN Pessoas      C ON C.codigo             = B.pessoa_codigo
	INNER JOIN Cargos       D ON D.codigo             = B.cargo_codigo
	WHERE A.tarefa_codigo = @tarefa
);
GO


-- #####################################################################
-- 3) VIEWS (Views.sql)
-- #####################################################################

-- Esconde as tarefas com exclusão lógica (status 'Cancelada')
CREATE OR ALTER VIEW V_Tarefas
AS
SELECT A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
       A.datahora_fim, A.visibilidade, A.status,
       A.projeto_codigo, A.categoria_codigo,
       B.descricao categoria_descricao, B.cor categoria_cor
FROM Tarefas A
INNER JOIN Categorias B ON B.codigo = A.categoria_codigo
WHERE A.status <> 'Cancelada'
GO

CREATE OR ALTER VIEW V_Funcionarios
AS
SELECT B.codigo, B.nome, b.telefone, b.documento, B.tipo_doc,
	   A.email, A.senha,
	   C.codigo cargo_codigo, C.descricao cargo_descricao
FROM Funcionarios A
INNER JOIN Pessoas B ON A.pessoa_codigo = B.codigo
INNER JOIN Cargos  C ON C.codigo        = A.cargo_codigo
GO

-- Depende de V_Funcionarios, por isso vem depois dela
CREATE OR ALTER VIEW V_Comentarios
AS
SELECT A.*,
	   B.nome funcionario_nome, B.Telefone funcionario_telefone,
	   B.email funcionario_email,
	   B.cargo_codigo, B.cargo_descricao
FROM Comentarios A
INNER JOIN V_Funcionarios B ON B.codigo = A.funcionario_codigo
GO


-- #####################################################################
-- 4) PROCEDURES (Procedures.sql, sem a parte de testes)
-- #####################################################################

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


-- #####################################################################
-- 5) DADOS DE TESTE (Celeiro_Criativo_Inserts_v4.sql)
-- Dados fictícios; senhas em texto puro ('123456'), apenas para teste.
-- Tudo em uma transação: se um insert falhar, nada é gravado.
-- #####################################################################

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Códigos 1 a 5 = funcionários | 6 a 10 = clientes
INSERT INTO Pessoas (nome, telefone, documento, tipo_doc) VALUES
    ('Helena Duarte',                '(17) 99999-0001', '111.111.111-11',     'cpf'),   -- 1
    ('Rafael Monteiro',              '(17) 99999-0002', '222.222.222-22',     'cpf'),   -- 2
    ('Bruno Carvalho',               '(17) 99999-0003', '333.333.333-33',     'cpf'),   -- 3
    ('Larissa Nogueira',             '(17) 99999-0004', '444.444.444-44',     'cpf'),   -- 4
    ('Tiago Ferraz',                 NULL,              '555.555.555-55',     'cpf'),   -- 5
    ('Café Aurora Ltda',             '(17) 3333-0006',  '11.111.111/0001-11', 'cnpj'),  -- 6
    ('EcoVerde Sustentabilidade',    '(17) 3333-0007',  '22.222.222/0001-22', 'cnpj'),  -- 7
    ('TecNova Soluções',             '(17) 3333-0008',  '33.333.333/0001-33', 'cnpj'),  -- 8
    ('Flores do Campo Floricultura', '(17) 3333-0009',  '44.444.444/0001-44', 'cnpj'),  -- 9
    ('Marina Albuquerque',           '(17) 99999-0010', '666.666.666-66',     'cpf');   -- 10

INSERT INTO Cargos (descricao) VALUES
    ('Gestor'),             -- 1
    ('Designer'),           -- 2
    ('Programador'),        -- 3
    ('Editor de Vídeo'),    -- 4
    ('Redator');            -- 5

-- Status de PROJETO (o status da tarefa é a coluna Tarefas.status)
INSERT INTO Status (descricao) VALUES
    ('Em análise'),         -- 1
    ('Em andamento'),       -- 2
    ('Finalizado'),         -- 3
    ('Excluído');           -- 4 - usado para exclusão lógica de Projeto

INSERT INTO Categorias (descricao, cor) VALUES
    ('Design',          '#4A90D9'),   -- 1
    ('Desenvolvimento', '#7ED321'),   -- 2
    ('Vídeo',           '#D0021B'),   -- 3
    ('Redação',         '#F5A623'),   -- 4
    ('Revisão',         '#9013FE');   -- 5

INSERT INTO Funcionarios (pessoa_codigo, cargo_codigo, ativo, email, senha) VALUES
    (1, 1, 1, 'helena@celeirocriativo.com',  '123456'),   -- Gestor
    (2, 2, 1, 'rafael@celeirocriativo.com',  '123456'),   -- Designer
    (3, 3, 1, 'bruno@celeirocriativo.com',   '123456'),   -- Programador
    (4, 4, 1, 'larissa@celeirocriativo.com', '123456'),   -- Editor de Vídeo
    (5, 5, 1, 'tiago@celeirocriativo.com',   '123456');   -- Redator

INSERT INTO Clientes (pessoa_codigo, codigo_verificacao, email, senha) VALUES
    (6,  'A1B2C3', 'contato@cafeaurora.exemplo.com',    '123456'),
    (7,  'D4E5F6', 'contato@ecoverde.exemplo.com',      '123456'),
    (8,  'G7H8I9', 'contato@tecnova.exemplo.com',       '123456'),
    (9,  'J1K2L3', 'contato@floresdocampo.exemplo.com', '123456'),
    (10, 'M4N5O6', 'marina@exemplo.com',                '123456');

INSERT INTO Projetos (cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo) VALUES
    (6,  'Identidade visual Café Aurora',   'Criação do logotipo e do manual da marca da cafeteria.',       '20260803', '20260828', '20260831', 3),  -- 1
    (7,  'Site institucional EcoVerde',     'Site de apresentação da empresa, com página de contato.',      '20260901', NULL,       '20261030', 2),  -- 2
    (8,  'Campanha de lançamento TecNova',  'Vídeo promocional e artes para as redes sociais.',             '20260921', NULL,       '20261120', 2),  -- 3
    (9,  'Catálogo digital Flores do Campo','Catálogo em PDF com os arranjos e preços da floricultura.',    '20260706', '20260724', '20260731', 3),  -- 4
    (10, 'Loja virtual Ateliê Marina',      'Ideia enviada pelo site: loja virtual para peças artesanais.', NULL,       NULL,       NULL,       1);  -- 5

-- status da tarefa: 'A fazer', 'Em andamento' ou 'Finalizado' ('Cancelada' = excluída)
-- O projeto 5 está "Em análise" e por isso ainda não tem tarefas.
INSERT INTO Tarefas (projeto_codigo, categoria_codigo, titulo, descricao, status, datahora_inicio, datahora_fim, datahora_prevista, visibilidade) VALUES
    (1, 1, 'Criação do logotipo',           'Três propostas de logotipo para aprovação.',           'Finalizado',   '2026-08-03T09:00:00', '2026-08-14T17:00:00', '2026-08-14T18:00:00', 1),  -- 1
    (1, 1, 'Manual da marca',               'Cores, tipografia e regras de uso do logotipo.',       'Finalizado',   '2026-08-17T09:00:00', '2026-08-28T16:30:00', '2026-08-31T18:00:00', 1),  -- 2
    (2, 1, 'Layout das páginas',            'Layout da página inicial, sobre e contato.',           'Finalizado',   '2026-09-01T09:00:00', '2026-09-18T17:00:00', '2026-09-18T18:00:00', 1),  -- 3
    (2, 2, 'Desenvolvimento do front-end',  'Implementação das páginas a partir do layout.',        'Em andamento', '2026-09-21T09:00:00', NULL,                  '2026-10-23T18:00:00', 1),  -- 4
    (2, 4, 'Textos das páginas',            'Redação dos textos institucionais do site.',           'A fazer',      NULL,                  NULL,                  '2026-10-16T18:00:00', 0),  -- 5
    (3, 4, 'Roteiro do vídeo',              'Roteiro do vídeo promocional de 60 segundos.',         'Finalizado',   '2026-09-21T09:00:00', '2026-09-30T15:00:00', '2026-10-02T18:00:00', 1),  -- 6
    (3, 3, 'Edição do vídeo promocional',   'Edição, trilha sonora e legendas do vídeo.',           'Em andamento', '2026-10-05T09:00:00', NULL,                  '2026-10-30T18:00:00', 1),  -- 7
    (3, 1, 'Artes para redes sociais',      'Dez artes para divulgação da campanha.',               'A fazer',      NULL,                  NULL,                  '2026-11-13T18:00:00', 0),  -- 8
    (4, 1, 'Diagramação do catálogo',       'Diagramação das páginas com fotos e preços.',          'Finalizado',   '2026-07-06T09:00:00', '2026-07-17T17:00:00', '2026-07-17T18:00:00', 1),  -- 9
    (4, 5, 'Revisão final do catálogo',     'Revisão de textos e preços antes da entrega.',         'Finalizado',   '2026-07-20T09:00:00', '2026-07-24T11:00:00', '2026-07-24T18:00:00', 1);  -- 10

INSERT INTO TarefaFuncionarios (funcionario_codigo, tarefa_codigo) VALUES
    (2, 1),
    (2, 2),
    (5, 2),
    (2, 3),
    (3, 4),
    (5, 5),
    (5, 6),
    (4, 7),
    (2, 7),
    (2, 8),
    (2, 9),
    (5, 10),
    (1, 10);

INSERT INTO Documentos (tarefa_codigo, descricao, extensao, diretorio) VALUES
    (1, 'Logotipo aprovado',             'png',  '/uploads/tarefa-1/logotipo-cafe-aurora.png'),
    (2, 'Manual da marca',               'pdf',  '/uploads/tarefa-2/manual-da-marca.pdf'),
    (3, 'Layout das páginas',            'pdf',  '/uploads/tarefa-3/layout-site-ecoverde.pdf'),
    (6, 'Roteiro do vídeo promocional',  'docx', '/uploads/tarefa-6/roteiro-video-tecnova.docx'),
    (7, 'Prévia do vídeo (primeiro corte)', 'mp4', '/uploads/tarefa-7/previa-video-tecnova.mp4'),
    (9, 'Catálogo diagramado',           'pdf',  '/uploads/tarefa-9/catalogo-flores-do-campo.pdf');

-- O par (funcionario_codigo, tarefa_codigo) precisa existir em TarefaFuncionarios
INSERT INTO Comentarios (tarefa_codigo, funcionario_codigo, descricao, data_hora) VALUES
    -- Tarefa 1 - Criação do logotipo
    (1,  2, 'Iniciei a pesquisa de referências de cafeterias e marcas artesanais.',        '2026-08-04T10:00:00'),
    (1,  2, 'Subi as três propostas de logotipo para aprovação.',                          '2026-08-10T14:20:00'),
    (1,  2, 'Ajustei a proposta 2 conforme o feedback: tons mais quentes no símbolo.',     '2026-08-12T09:30:00'),
    (1,  2, 'Versão final do logotipo enviada, com os arquivos vetoriais nos documentos.', '2026-08-14T16:45:00'),

    -- Tarefa 2 - Manual da marca
    (2,  2, 'Paleta de cores definida: marrom café, creme e verde-oliva.',                 '2026-08-18T11:00:00'),
    (2,  2, 'Seção de tipografia pronta, enviei para o Tiago revisar os textos.',          '2026-08-21T15:10:00'),
    (2,  5, 'Revisei os textos do manual, sem pendências.',                                '2026-08-27T10:05:00'),
    (2,  2, 'Manual finalizado e exportado em PDF.',                                       '2026-08-28T16:00:00'),

    -- Tarefa 3 - Layout das páginas
    (3,  2, 'Wireframes das três páginas concluídos.',                                     '2026-09-03T10:20:00'),
    (3,  2, 'Layout da página inicial aprovado, seguindo para sobre e contato.',           '2026-09-10T14:00:00'),
    (3,  2, 'Layout completo enviado, liberado para o desenvolvimento.',                   '2026-09-18T16:30:00'),

    -- Tarefa 4 - Desenvolvimento do front-end
    (4,  3, 'Estrutura do projeto criada e componentes base configurados.',                '2026-09-22T09:45:00'),
    (4,  3, 'Cabeçalho e rodapé responsivos finalizados.',                                 '2026-09-29T17:10:00'),
    (4,  3, 'Página inicial implementada, faltam sobre e contato.',                        '2026-10-06T16:40:00'),
    (4,  3, 'Formulário de contato em desenvolvimento, falta a validação dos campos.',     '2026-10-13T15:20:00'),

    -- Tarefa 5 - Textos das páginas (ainda "A fazer")
    (5,  5, 'Recebi o briefing do cliente, aguardando liberação para começar.',            '2026-10-01T10:00:00'),
    (5,  5, 'Separei referências de tom de voz de sites do segmento sustentável.',         '2026-10-07T14:30:00'),
    (5,  5, 'Pedi ao cliente a história da empresa para usar na página sobre.',            '2026-10-13T09:15:00'),

    -- Tarefa 6 - Roteiro do vídeo
    (6,  5, 'Primeira versão do roteiro com foco nos diferenciais do produto.',            '2026-09-22T11:00:00'),
    (6,  5, 'Ajustei o roteiro para caber nos 60 segundos.',                               '2026-09-25T16:20:00'),
    (6,  5, 'Roteiro final aprovado e enviado nos documentos.',                            '2026-09-30T14:40:00'),

    -- Tarefa 7 - Edição do vídeo promocional
    (7,  4, 'Material bruto organizado, começando a decupagem.',                           '2026-10-05T10:30:00'),
    (7,  4, 'Primeiro corte do vídeo disponível nos documentos.',                          '2026-10-09T11:15:00'),
    (7,  2, 'Enviei a vinheta com o logotipo para usar na abertura.',                      '2026-10-09T15:30:00'),
    (7,  4, 'Trilha sonora escolhida e sincronizada com as cenas.',                        '2026-10-12T17:00:00'),
    (7,  2, 'Ajustei as cores da vinheta para combinar com a correção de cor do vídeo.',   '2026-10-13T11:40:00'),

    -- Tarefa 8 - Artes para redes sociais (ainda "A fazer")
    (8,  2, 'Aguardando o vídeo ficar pronto para manter a mesma identidade nas artes.',   '2026-10-02T09:30:00'),
    (8,  2, 'Formatos definidos: feed, stories e capa.',                                   '2026-10-08T15:00:00'),
    (8,  2, 'Esbocei as primeiras ideias de composição para as artes.',                    '2026-10-13T16:10:00'),

    -- Tarefa 9 - Diagramação do catálogo
    (9,  2, 'Recebi as fotos dos arranjos, algumas precisam de tratamento.',               '2026-07-07T10:15:00'),
    (9,  2, 'Metade das páginas diagramada, layout em grade de duas colunas.',             '2026-07-13T14:50:00'),
    (9,  2, 'Catálogo diagramado e enviado para revisão.',                                 '2026-07-17T16:40:00'),

    -- Tarefa 10 - Revisão final do catálogo
    (10, 5, 'Iniciei a revisão, conferindo os preços com a tabela do cliente.',            '2026-07-20T10:00:00'),
    (10, 5, 'Encontrei dois preços divergentes, o cliente já confirmou os valores certos.', '2026-07-22T15:30:00'),
    (10, 5, 'Revisão concluída, catálogo pronto para a aprovação final.',                  '2026-07-24T10:50:00'),
    (10, 1, 'Revisão conferida, catálogo aprovado para entrega ao cliente.',               '2026-07-24T11:30:00');

COMMIT TRANSACTION;
GO


-- #####################################################################
-- CONFERÊNCIA: quantidade de registros por tabela
-- #####################################################################

SELECT 'Pessoas' tabela, COUNT(*) total FROM Pessoas
UNION ALL SELECT 'Cargos',             COUNT(*) FROM Cargos
UNION ALL SELECT 'Status',             COUNT(*) FROM Status
UNION ALL SELECT 'Categorias',         COUNT(*) FROM Categorias
UNION ALL SELECT 'Funcionarios',       COUNT(*) FROM Funcionarios
UNION ALL SELECT 'Clientes',           COUNT(*) FROM Clientes
UNION ALL SELECT 'Projetos',           COUNT(*) FROM Projetos
UNION ALL SELECT 'Tarefas',            COUNT(*) FROM Tarefas
UNION ALL SELECT 'TarefaFuncionarios', COUNT(*) FROM TarefaFuncionarios
UNION ALL SELECT 'Documentos',         COUNT(*) FROM Documentos
UNION ALL SELECT 'Comentarios',        COUNT(*) FROM Comentarios;
GO
