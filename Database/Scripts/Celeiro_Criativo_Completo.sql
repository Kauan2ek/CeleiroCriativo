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
-- Última sincronização: 28/10/2026
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
    documento       VARCHAR(20) NOT NULL UNIQUE,
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
    email           VARCHAR(100) NOT NULL UNIQUE,
    senha           VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Funcionarios_Pessoa FOREIGN KEY (pessoa_codigo) REFERENCES Pessoas(codigo),
    CONSTRAINT FK_Funcionarios_Cargo  FOREIGN KEY (cargo_codigo)  REFERENCES Cargos(codigo)
);

CREATE TABLE Clientes (
    pessoa_codigo       INT PRIMARY KEY,
    codigo_verificacao  VARCHAR(20) NULL,
    email               VARCHAR(100) NOT NULL UNIQUE,
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

-- 1. Adicionar cargo
CREATE OR ALTER PROCEDURE AdicionarCargo
    @descricao VARCHAR(50)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO Cargos (descricao)
        VALUES (@descricao);

        DECLARE @cargo_codigo INT = SCOPE_IDENTITY();

        COMMIT;

        SELECT codigo, descricao
        FROM Cargos
        WHERE codigo = @cargo_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 2. Atualizar cargo
CREATE OR ALTER PROCEDURE AtualizarCargo
    @cargo_codigo INT,
    @descricao VARCHAR(50)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        UPDATE Cargos
        SET descricao = @descricao
        WHERE codigo = @cargo_codigo;

        COMMIT;

        SELECT codigo, descricao
        FROM Cargos
        WHERE codigo = @cargo_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 3. Adicionar status
CREATE OR ALTER PROCEDURE AdicionarStatus
    @descricao VARCHAR(50)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO Status (descricao)
        VALUES (@descricao);
        DECLARE @status_codigo INT = SCOPE_IDENTITY();
        COMMIT;
        SELECT codigo, descricao
        FROM Status
        WHERE codigo = @status_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 4. Alterar status
CREATE OR ALTER PROCEDURE AlterarStatus
    @status_codigo INT,
    @descricao VARCHAR(50)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        UPDATE Status SET descricao = @descricao WHERE codigo = @status_codigo;
        COMMIT;
        SELECT codigo, descricao
        FROM Status
        WHERE codigo = @status_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 5. Adicionar categoria
CREATE OR ALTER PROCEDURE AdicionarCategoria
    @descricao VARCHAR(50),
    @cor VARCHAR(20) = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO Categorias (descricao, cor) VALUES (@descricao, @cor);
        DECLARE @categoria_codigo INT = SCOPE_IDENTITY();
        COMMIT;
        SELECT codigo, descricao, cor
        FROM Categorias
        WHERE codigo = @categoria_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 6. Atualizar categoria
CREATE OR ALTER PROCEDURE AtualizarCategoria
    @categoria_codigo INT,
    @descricao VARCHAR(50),
    @cor VARCHAR(20)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        UPDATE Categorias
        SET descricao = @descricao, cor = @cor  WHERE codigo = @categoria_codigo;
        COMMIT;
        SELECT codigo, descricao, cor
        FROM Categorias
        WHERE codigo = @categoria_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 7. Cadastrar funcionario
CREATE OR ALTER PROCEDURE CadastrarFuncionario
    @nome VARCHAR(50),
    @documento VARCHAR(20),
    @tipo_doc VARCHAR(20),
    @cargo_codigo INT,
    @email VARCHAR(100),
    @senha VARCHAR(255),
    @telefone VARCHAR(20) = NULL,
    @ativo BIT = 1
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @codigo INT
        SET @tipo_doc = LOWER(TRIM(@tipo_doc));
        SET @documento = REPLACE(REPLACE(REPLACE(REPLACE(@documento, '.', ''), '-', ''),'/', ''), ' ', '');
        SELECT  @codigo = (SELECT codigo FROM Pessoas WHERE documento = @documento)

        IF @codigo is null
        BEGIN
            INSERT INTO Pessoas (nome, telefone, documento, tipo_doc)
            VALUES (@nome, @telefone, @documento, @tipo_doc)
            SET @codigo = SCOPE_IDENTITY()
        END

        INSERT INTO Funcionarios (pessoa_codigo, cargo_codigo, ativo, email, senha)
        VALUES (@codigo, @cargo_codigo, @ativo, @email, @senha);

        COMMIT;

        SELECT p.codigo AS pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, f.cargo_codigo, f.ativo, f.email
        FROM Pessoas AS p
        INNER JOIN Funcionarios AS f ON f.pessoa_codigo = p.codigo
        WHERE p.codigo = @codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 8. Atualizar funcionario
CREATE OR ALTER PROCEDURE AtualizarFuncionario
    @funcionario_codigo INT,
    @nome VARCHAR(50),
    @telefone VARCHAR(20),
    @documento VARCHAR(20),
    @tipo_doc VARCHAR(20),
    @cargo_codigo INT,
    @ativo BIT,
    @email VARCHAR(100),
    @senha VARCHAR(255) = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        IF NOT EXISTS (SELECT 1 FROM Funcionarios WHERE pessoa_codigo = @funcionario_codigo)
            THROW 50002, 'Funcionário não encontrado.', 1;
        SET @documento = REPLACE(REPLACE(REPLACE(REPLACE(@documento, '.', ''), '-', ''),'/', ''), ' ', '');
        SET @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)));

        UPDATE Pessoas
        SET nome = @nome,
            telefone = @telefone,
            documento = @documento,
            tipo_doc = @tipo_doc
        WHERE codigo = @funcionario_codigo;

        UPDATE Funcionarios
        SET cargo_codigo = @cargo_codigo,
            ativo = @ativo,
            email = @email,
            senha = COALESCE(NULLIF(@senha, ''), senha)
        WHERE pessoa_codigo = @funcionario_codigo;

        COMMIT;

        SELECT p.codigo AS pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, f.cargo_codigo, f.ativo, f.email
        FROM Pessoas AS p
        INNER JOIN Funcionarios AS f ON f.pessoa_codigo = p.codigo
        WHERE p.codigo = @funcionario_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 9. Cadastrar cliente
CREATE OR ALTER PROCEDURE CadastrarCliente
    @nome VARCHAR(50),
    @documento VARCHAR(20),
    @tipo_doc VARCHAR(20),
    @email VARCHAR(100),
    @telefone VARCHAR(20) = NULL,
    @codigo_verificacao VARCHAR(20) = NULL,
    @senha VARCHAR(255) = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @codigo INT
        SET @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)))
        SET @documento = REPLACE(REPLACE(REPLACE(REPLACE(@documento, '.', ''), '-', ''), '/', ''), ' ', '')
        SET @codigo = (SELECT codigo FROM Pessoas WHERE documento = @documento)

        IF @codigo is null
        BEGIN
            INSERT INTO Pessoas (nome, telefone, documento, tipo_doc)
            VALUES (@nome, @telefone, @documento, @tipo_doc)
            SET @codigo = SCOPE_IDENTITY()
        END

        INSERT INTO Clientes (pessoa_codigo, codigo_verificacao, email, senha)
        VALUES (@codigo, @codigo_verificacao, @email, @senha)

        COMMIT;

        SELECT p.codigo AS pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, c.codigo_verificacao, c.email
        FROM Pessoas AS p
        INNER JOIN Clientes AS c ON c.pessoa_codigo = p.codigo
        WHERE p.codigo = @codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END
GO

-- 10. Atualizar cliente
CREATE OR ALTER PROCEDURE AtualizarCliente
    @cliente_codigo INT,
    @nome VARCHAR(50),
    @telefone VARCHAR(20),
    @documento VARCHAR(20),
    @tipo_doc VARCHAR(20),
    @email VARCHAR(100),
    @codigo_verificacao VARCHAR(20),
    @senha VARCHAR(255) = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        IF NOT EXISTS (SELECT 1 FROM Clientes WHERE pessoa_codigo = @cliente_codigo)
            THROW 50002, 'Cliente não encontrado.', 1;

        SET @documento = REPLACE(REPLACE(REPLACE(REPLACE(@documento, '.', ''), '-', ''),'/', ''), ' ', '');
        SET @tipo_doc = LOWER(LTRIM(RTRIM(@tipo_doc)));

        UPDATE Pessoas
        SET nome = @nome,
            telefone = @telefone,
            documento = @documento,
            tipo_doc = @tipo_doc
        WHERE codigo = @cliente_codigo;

        UPDATE Clientes
        SET codigo_verificacao = @codigo_verificacao,
            email = @email,
            senha = COALESCE(NULLIF(@senha, ''), senha)
        WHERE pessoa_codigo = @cliente_codigo;

        COMMIT;

        SELECT p.codigo AS pessoa_codigo, p.nome, p.telefone,
               p.documento, p.tipo_doc, c.codigo_verificacao, c.email
        FROM Pessoas AS p
        INNER JOIN Clientes AS c ON c.pessoa_codigo = p.codigo
        WHERE p.codigo = @cliente_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 11. Cadastrar projeto
CREATE OR ALTER PROCEDURE CadastrarProjeto
    @cliente_codigo INT,
    @titulo VARCHAR(50),
    @descricao VARCHAR(MAX) = NULL,
    @data_inicio DATE = NULL,
    @data_fim DATE = NULL,
    @data_prevista DATE = NULL,
    @status_codigo INT = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO Projetos (cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo)
        VALUES (@cliente_codigo, @titulo, @descricao, @data_inicio, @data_fim, @data_prevista, @status_codigo);

        DECLARE @projeto_codigo INT = SCOPE_IDENTITY();

        COMMIT;

        SELECT codigo, cliente_codigo, titulo, descricao,
               data_inicio, data_fim, data_prevista, status_codigo
        FROM Projetos
        WHERE codigo = @projeto_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 12. Atualizar projeto
CREATE OR ALTER PROCEDURE AtualizarProjeto
    @projeto_codigo INT,
    @cliente_codigo INT,
    @titulo VARCHAR(50),
    @descricao VARCHAR(MAX) = NULL,
    @data_inicio DATE = NULL,
    @data_fim DATE = NULL,
    @data_prevista DATE = NULL,
    @status_codigo INT = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        UPDATE Projetos
        SET cliente_codigo = @cliente_codigo,
            titulo = @titulo,
            descricao = @descricao,
            data_inicio = @data_inicio,
            data_fim = @data_fim,
            data_prevista = @data_prevista,
            status_codigo = @status_codigo
        WHERE codigo = @projeto_codigo;

        COMMIT;

        SELECT codigo, cliente_codigo, titulo, descricao,
               data_inicio, data_fim, data_prevista, status_codigo
        FROM Projetos
        WHERE codigo = @projeto_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 13. Adicionar tarefa
CREATE OR ALTER PROCEDURE AdicionarTarefa
    @projeto_codigo INT,
    @categoria_codigo INT,
    @titulo VARCHAR(50),
    @descricao VARCHAR(MAX),
    @status VARCHAR(30),
    @datahora_inicio DATETIME = NULL,
    @datahora_fim DATETIME = NULL,
    @datahora_prevista DATETIME = NULL,
    @visibilidade BIT = 1
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO Tarefas (projeto_codigo, categoria_codigo, titulo, descricao, status,
                             datahora_inicio, datahora_fim, datahora_prevista, visibilidade)
        VALUES (@projeto_codigo, @categoria_codigo, @titulo, @descricao, @status,
                @datahora_inicio, @datahora_fim, @datahora_prevista, @visibilidade);

        DECLARE @tarefa_codigo INT = SCOPE_IDENTITY();

        COMMIT;

        SELECT codigo, projeto_codigo, categoria_codigo, titulo, descricao,
               status, datahora_inicio, datahora_fim, datahora_prevista, visibilidade
        FROM Tarefas
        WHERE codigo = @tarefa_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 14. Atualizar tarefa
CREATE OR ALTER PROCEDURE AtualizarTarefa
    @tarefa_codigo INT,
    @projeto_codigo INT,
    @categoria_codigo INT,
    @titulo VARCHAR(50),
    @descricao VARCHAR(MAX),
    @status VARCHAR(30),
    @datahora_inicio DATETIME = NULL,
    @datahora_fim DATETIME = NULL,
    @datahora_prevista DATETIME = NULL,
    @visibilidade BIT = 1
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        UPDATE Tarefas
        SET projeto_codigo = @projeto_codigo,
            categoria_codigo = @categoria_codigo,
            titulo = @titulo,
            descricao = @descricao,
            status = @status,
            datahora_inicio = @datahora_inicio,
            datahora_fim = @datahora_fim,
            datahora_prevista = @datahora_prevista,
            visibilidade = @visibilidade
        WHERE codigo = @tarefa_codigo;

        COMMIT;

        SELECT codigo, projeto_codigo, categoria_codigo, titulo, descricao,
               status, datahora_inicio, datahora_fim, datahora_prevista, visibilidade
        FROM Tarefas
        WHERE codigo = @tarefa_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 15. Atribuir funcionario a tarefa
CREATE OR ALTER PROCEDURE AtribuirFuncionarioTarefa
    @funcionario_codigo INT,
    @tarefa_codigo INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO TarefaFuncionarios (funcionario_codigo, tarefa_codigo)
        VALUES (@funcionario_codigo, @tarefa_codigo);

        COMMIT;

        SELECT funcionario_codigo, tarefa_codigo
        FROM TarefaFuncionarios
        WHERE funcionario_codigo = @funcionario_codigo AND tarefa_codigo = @tarefa_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 16. Atualizar atribuicao de funcionario a tarefa
CREATE OR ALTER PROCEDURE AtualizarAtribuicaoFuncionarioTarefa
    @funcionario_codigo_atual INT,
    @tarefa_codigo_atual INT,
    @funcionario_codigo_novo INT,
    @tarefa_codigo_novo INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        UPDATE TarefaFuncionarios
        SET funcionario_codigo = @funcionario_codigo_novo,
            tarefa_codigo = @tarefa_codigo_novo
        WHERE funcionario_codigo = @funcionario_codigo_atual
          AND tarefa_codigo = @tarefa_codigo_atual;

        COMMIT;

        SELECT funcionario_codigo, tarefa_codigo
        FROM TarefaFuncionarios
        WHERE funcionario_codigo = @funcionario_codigo_novo AND tarefa_codigo = @tarefa_codigo_novo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 17. Adicionar documento
CREATE OR ALTER PROCEDURE AdicionarDocumento
    @tarefa_codigo INT,
    @descricao VARCHAR(MAX),
    @extensao VARCHAR(10),
    @diretorio VARCHAR(255)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO Documentos (tarefa_codigo, descricao, extensao, diretorio)
        VALUES (@tarefa_codigo, @descricao, @extensao, @diretorio);

        DECLARE @documento_codigo INT = SCOPE_IDENTITY();

        COMMIT;

        SELECT codigo, tarefa_codigo, descricao, extensao, diretorio
        FROM Documentos
        WHERE codigo = @documento_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 18. Atualizar documento
CREATE OR ALTER PROCEDURE AtualizarDocumento
    @documento_codigo INT,
    @tarefa_codigo INT,
    @descricao VARCHAR(MAX),
    @extensao VARCHAR(10),
    @diretorio VARCHAR(255)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        UPDATE Documentos
        SET tarefa_codigo = @tarefa_codigo,
            descricao = @descricao,
            extensao = @extensao,
            diretorio = @diretorio
        WHERE codigo = @documento_codigo;

        COMMIT;

        SELECT codigo, tarefa_codigo, descricao, extensao, diretorio
        FROM Documentos
        WHERE codigo = @documento_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 19. Adicionar comentario
CREATE OR ALTER PROCEDURE AdicionarComentario
    @tarefa_codigo INT,
    @funcionario_codigo INT,
    @descricao VARCHAR(MAX),
    @data_hora DATETIME = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO Comentarios (tarefa_codigo, funcionario_codigo, descricao, data_hora)
        VALUES (@tarefa_codigo, @funcionario_codigo, @descricao, COALESCE(@data_hora, GETDATE()));

        DECLARE @comentario_codigo INT = SCOPE_IDENTITY();
        COMMIT;
        SELECT codigo, tarefa_codigo, funcionario_codigo, descricao, data_hora
        FROM Comentarios
        WHERE codigo = @comentario_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO

-- 20. Atualizar comentario
CREATE OR ALTER PROCEDURE AtualizarComentario
    @comentario_codigo INT,
    @tarefa_codigo INT,
    @funcionario_codigo INT,
    @descricao VARCHAR(MAX),
    @data_hora DATETIME = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        UPDATE Comentarios
        SET tarefa_codigo = @tarefa_codigo,
            funcionario_codigo = @funcionario_codigo,
            descricao = @descricao,
            data_hora = COALESCE(@data_hora, data_hora)
        WHERE codigo = @comentario_codigo;
        COMMIT;

        SELECT codigo, tarefa_codigo, funcionario_codigo, descricao, data_hora
        FROM Comentarios
        WHERE codigo = @comentario_codigo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        THROW; -- Repassa o erro original, sem validacoes personalizadas.
    END CATCH
END;
GO


-- #####################################################################
-- 5) DADOS DE TESTE (Celeiro_Criativo_Inserts_v4.sql)
-- Dados fictícios; senhas em texto puro ('123456'), apenas para teste.
-- Tudo em uma transação: se um insert falhar, nada é gravado.
-- #####################################################################

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Códigos 1 a 5 = funcionários | 6 a 10 = clientes
INSERT INTO Pessoas (nome, telefone, documento, tipo_doc) VALUES
    ('Helena Duarte',                '(17) 99999-0001', '11111111111',    'cpf'),   -- 1
    ('Rafael Monteiro',              '(17) 99999-0002', '22222222222',    'cpf'),   -- 2
    ('Bruno Carvalho',               '(17) 99999-0003', '33333333333',    'cpf'),   -- 3
    ('Larissa Nogueira',             '(17) 99999-0004', '44444444444',    'cpf'),   -- 4
    ('Tiago Ferraz',                 NULL,              '55555555555',    'cpf'),   -- 5
    ('Café Aurora Ltda',             '(17) 3333-0006',  '11111111000111', 'cnpj'),  -- 6
    ('EcoVerde Sustentabilidade',    '(17) 3333-0007',  '22222222000122', 'cnpj'),  -- 7
    ('TecNova Soluções',             '(17) 3333-0008',  '33333333000133', 'cnpj'),  -- 8
    ('Flores do Campo Floricultura', '(17) 3333-0009',  '44444444000144', 'cnpj'),  -- 9
    ('Marina Albuquerque',           '(17) 99999-0010', '77777777777',    'cpf');   -- 10

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
    ('Excluído'),           -- 4 - usado para exclusão lógica de Projeto
    ('TESTE');              -- 5 - usado apenas para testes das procedures

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
