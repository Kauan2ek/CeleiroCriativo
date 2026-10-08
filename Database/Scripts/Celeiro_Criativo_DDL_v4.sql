-- =====================================================================
-- CELEIRO CRIATIVO - SCRIPT DE CRIAÇÃO DO BANCO (SQL Server)
-- v4 - Baseado no DER de 2026-10-06 e no Celeiro_Criativo_DDL_v3.sql
-- Ordem de criação respeita as dependências de chave estrangeira.
-- Autor: Kauan Dias - 17/09/2026
--
-- Mudanças da v3 para a v4 (DER 2026-10-06):
--   1) Projetos 1:N Tarefas -> nova coluna Tarefas.projeto_codigo
--      (FK_Tarefas_Projeto). Agora a tarefa pertence ao projeto
--      diretamente.
--   2) TarefaFuncionarios deixou de ser ternária: o DER não liga mais
--      Tarefas_funcionarios a Projetos. Saiu projeto_codigo; a PK passou
--      a ser (funcionario_codigo, tarefa_codigo).
--   3) Status da Tarefa virou ATRIBUTO no DER (a entidade Status agora
--      só se relaciona com Projetos). Saiu a FK Tarefas -> Status;
--      entrou a coluna Tarefas.status (VARCHAR).
--   4) Comentarios agora se relaciona com Tarefas_funcionarios (N:1):
--      o autor do comentário é o funcionário ALOCADO naquela tarefa.
--      FK composta (funcionario_codigo, tarefa_codigo) ->
--      TarefaFuncionarios.
--   5) Tarefas N:1 Categorias: o DER agora confirma a direção da FK
--      (Tarefas -> Categorias), que já era a usada na v3.
-- =====================================================================

USE CeleiroCriativo;
GO

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

-- Status agora é usado apenas por Projetos (ver mudança 3).
CREATE TABLE Status (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(50) NOT NULL
);

CREATE TABLE Categorias (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(50) NOT NULL,
    cor         VARCHAR(20) NULL
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
    senha               VARCHAR(255) NULL, -- nullable: só é definida após aprovação da ideia
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

-- ATENÇÃO (v4): "status" agora é atributo simples da tarefa. Se os
-- valores forem fixos, vale travar com CHECK, ex.:
--   CONSTRAINT CK_Tarefas_Status CHECK (status IN ('pendente','em andamento','concluida'))
-- (valores de exemplo — defina os reais antes de ativar).
CREATE TABLE Tarefas (
    codigo              INT IDENTITY(1,1) PRIMARY KEY,
    projeto_codigo      INT NOT NULL,
    categoria_codigo    INT NULL,
    titulo              VARCHAR(50) NOT NULL,
    descricao           VARCHAR(MAX) NULL,
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
-- v4: deixou de ser ternária — o projeto já é conhecido pela tarefa.
-- ---------------------------------------------------------------------

CREATE TABLE TarefaFuncionarios (
    funcionario_codigo  INT NOT NULL,
    tarefa_codigo       INT NOT NULL,
    responsavel         BIT NOT NULL DEFAULT 0,
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
    descricao       VARCHAR(MAX) NULL,
    extensao        VARCHAR(10) NULL, -- ex.: 'pdf', 'docx', 'png'
    diretorio       VARCHAR(255) NULL,
    CONSTRAINT FK_Documentos_Tarefa FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo)
);

-- ---------------------------------------------------------------------
-- COMENTARIOS (depende de Tarefas e TarefaFuncionarios)
-- v4: o DER liga Comentarios a Tarefas ("possui") E a
-- Tarefas_funcionarios ("tem"). As duas relações compartilham a mesma
-- coluna tarefa_codigo, então a FK composta garante que só quem está
-- alocado na tarefa pode comentar nela.
-- ---------------------------------------------------------------------

CREATE TABLE Comentarios (
    codigo               INT IDENTITY(1,1) PRIMARY KEY,
    tarefa_codigo        INT NOT NULL,
    funcionario_codigo   INT NOT NULL,
    titulo               VARCHAR(50) NULL,
    descricao            VARCHAR(MAX) NULL,
    data_hora            DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Comentarios_Tarefa            FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo),
    CONSTRAINT FK_Comentarios_TarefaFuncionario FOREIGN KEY (funcionario_codigo, tarefa_codigo)
        REFERENCES TarefaFuncionarios(funcionario_codigo, tarefa_codigo)
);
