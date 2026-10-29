-- !!! AO ALTERAR ESTE ARQUIVO, ATUALIZE TAMBÉM O Celeiro_Criativo_Completo.sql !!!
USE CeleiroCriativo
GO

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

-- Teste de execucao (altera os dados do banco)
EXEC AdicionarCargo
    @descricao = 'Designer web';
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarCargo
    @cargo_codigo = 6,
    @descricao = 'Designer gráfico';
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

-- Teste de execucao (altera os dados do banco)
EXEC AdicionarStatus
    @descricao = 'TESTE2';
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

-- Teste de execucao (altera os dados do banco)
EXEC AlterarStatus
    @status_codigo = 5,
    @descricao = 'Deletado (apenas testes)'; -- Apenas para testes, quebra a aplicação
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

-- Teste de execucao (altera os dados do banco)
EXEC AdicionarCategoria
    @descricao = 'Desenvolvimento',
    @cor = '#0000FF';
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarCategoria
    @categoria_codigo = 4,
    @descricao = 'Redação',
    @cor = '#FF0000';
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

-- Teste de execucao (altera os dados do banco)
EXEC CadastrarFuncionario  @nome = 'Joao Silva',  @documento = '12345678901',
    @tipo_doc = 'cpf',  @cargo_codigo = 3,    @email = 'joao@teste.com',
    @senha = '123456',  @telefone = '17999999999',  @ativo = 1;
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarFuncionario
    @funcionario_codigo = 13,
    @nome = 'Maria Dantas',
    @telefone = '17988888888',
    @documento = '12345678901',
    @tipo_doc = 'cpf',
    @cargo_codigo = 6,
    @ativo = 1,
    @email = 'mariadantas@teste.com',
    @senha = NULL;
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

-- Teste de execucao (altera os dados do banco)
EXEC CadastrarCliente
    @nome = 'Maria Oliveira',
    @documento = '98765432100',
    @tipo_doc = 'cpf',
    @email = 'maria@teste.com',
    @telefone = '17999999999',
    @codigo_verificacao = 'ABC123',
    @senha = '123456';
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarCliente
    @cliente_codigo = 14,
    @nome = 'Joao Mario',
    @telefone = '17988888888',
    @documento = '98765432100',
    @tipo_doc = 'cpf',
    @email = 'joaomario@teste.com',
    @codigo_verificacao = 'XYZ789',
    @senha = NULL;
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

-- Teste de execucao (altera os dados do banco)
EXEC CadastrarProjeto
    @cliente_codigo = 14,
    @titulo = 'Projeto de teste',
    @descricao = 'Desenvolvimento de um site',
    @data_inicio = '20261027',
    @data_fim = NULL,
    @data_prevista = '20261110',
    @status_codigo = 1;
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarProjeto
    @projeto_codigo = 7,
    @cliente_codigo = 14,
    @titulo = 'Projeto atualizado',
    @descricao = 'Desenvolvimento atualizado',
    @data_inicio = '2026-10-27 11:00:00',
    @data_fim = NULL,
    @data_prevista = '2026-11-15 13:00:00',
    @status_codigo = 1;
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

-- Teste de execucao (altera os dados do banco)
EXEC AdicionarTarefa
    @projeto_codigo = 7,
    @categoria_codigo = 6,
    @titulo = 'Criar pagina inicial',
    @descricao = 'Desenvolver a pagina inicial do site',
    @status = 'Pendente',
    @datahora_inicio = '2026-10-27 13:00:00',
    @datahora_fim = NULL,
    @datahora_prevista = '2026-11-05 18:00:00',
    @visibilidade = 1;
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarTarefa
    @tarefa_codigo = 11,
    @projeto_codigo = 7,
    @categoria_codigo = 6,
    @titulo = 'Criar pagina inicial atualizada',
    @descricao = 'Desenvolvimento atualizado',
    @status = 'Em andamento',
    @datahora_inicio = '2026-10-27 13:00:00',
    @datahora_fim = NULL,
    @datahora_prevista = '2026-11-08 18:00:00',
    @visibilidade = 1;
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

-- Teste de execucao (altera os dados do banco)
EXEC AtribuirFuncionarioTarefa
    @funcionario_codigo = 13,
    @tarefa_codigo = 6;
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarAtribuicaoFuncionarioTarefa
    @funcionario_codigo_atual = 13,
    @tarefa_codigo_atual = 6,
    @funcionario_codigo_novo = 11,
    @tarefa_codigo_novo = 6;
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

-- Teste de execucao (altera os dados do banco)
EXEC AdicionarDocumento
    @tarefa_codigo = 1,
    @descricao = 'Documento de teste',
    @extensao = 'pdf',
    @diretorio = '/documentos/teste.pdf';
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarDocumento
    @documento_codigo = 1,
    @tarefa_codigo = 1,
    @descricao = 'Documento atualizado',
    @extensao = 'pdf',
    @diretorio = '/documentos/atualizado.pdf';
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

-- Teste de execucao (altera os dados do banco)
EXEC AdicionarComentario
    @tarefa_codigo = 6,
    @funcionario_codigo = 11,
    @descricao = 'Comentario sobre a tarefa';
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

-- Teste de execucao (altera os dados do banco)
EXEC AtualizarComentario
    @comentario_codigo = 1,
    @tarefa_codigo = 6,
    @funcionario_codigo = 11,
    @descricao = 'Descricao atualizada';
GO
