-- =========================================================
-- Celeiro Criativo (ES3)
-- Script T-SQL (SQL Server) gerado a partir do DER
-- =========================================================

CREATE DATABASE CeleiroCriativo;
GO

USE CeleiroCriativo;
GO

-- Entidade generica da hierarquia "Isa".
-- Funcionarios e Clientes sao subtipos e usam a mesma PK da Pessoa.
CREATE TABLE Pessoa (
    codigo          INT IDENTITY(1,1) PRIMARY KEY,
    nome            VARCHAR(100) NOT NULL,
    telefone        VARCHAR(20),
    tipo_documento  VARCHAR(4)   NOT NULL,   -- 'CPF' ou 'CNPJ'
    documento       VARCHAR(18)  NOT NULL UNIQUE
);

CREATE TABLE Cargos (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(100) NOT NULL
);

-- Subtipo de Pessoa (PK = FK para Pessoa)
-- Relacionamento "tem": 1 Cargo -> N Funcionarios
CREATE TABLE Funcionarios (
    codigo        INT PRIMARY KEY,
    email         VARCHAR(150) NOT NULL UNIQUE,
    senha         VARCHAR(255) NOT NULL,     -- tamanho pensado para guardar hash
    ativo         BIT NOT NULL DEFAULT 1,
    cargo_codigo  INT NOT NULL,
    CONSTRAINT FK_Funcionarios_Pessoa
        FOREIGN KEY (codigo) REFERENCES Pessoa(codigo),
    CONSTRAINT FK_Funcionarios_Cargos
        FOREIGN KEY (cargo_codigo) REFERENCES Cargos(codigo)
);

-- Subtipo de Pessoa (PK = FK para Pessoa)
CREATE TABLE Clientes (
    codigo               INT PRIMARY KEY,
    email                VARCHAR(150) NOT NULL UNIQUE,
    senha                VARCHAR(255) NOT NULL,
    codigo_verificacao   VARCHAR(10),
    CONSTRAINT FK_Clientes_Pessoa
        FOREIGN KEY (codigo) REFERENCES Pessoa(codigo)
);

CREATE TABLE Status (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(50) NOT NULL
);

-- Relacionamento "abre": 1 Cliente -> N Projetos
-- Relacionamento "tem":  1 Status  -> N Projetos
CREATE TABLE Projetos (
    codigo          INT IDENTITY(1,1) PRIMARY KEY,
    titulo          VARCHAR(150) NOT NULL,
    descricao       VARCHAR(1000),
    data_inicio     DATE NOT NULL DEFAULT GETDATE(),
    data_prevista   DATE,
    data_fim        DATE,
    cliente_codigo  INT NOT NULL,
    status_codigo   INT NOT NULL,
    CONSTRAINT FK_Projetos_Clientes
        FOREIGN KEY (cliente_codigo) REFERENCES Clientes(codigo),
    CONSTRAINT FK_Projetos_Status
        FOREIGN KEY (status_codigo) REFERENCES Status(codigo)
);

CREATE TABLE Categorias (
    codigo      INT IDENTITY(1,1) PRIMARY KEY,
    descricao   VARCHAR(100) NOT NULL,
    cor         VARCHAR(7)                   -- hexadecimal, ex: '#FF8800'
);

-- Relacionamento "tem": 1 Projeto   -> N Tarefas
-- Relacionamento "tem": 1 Categoria -> N Tarefas
CREATE TABLE Tarefas (
    codigo              INT IDENTITY(1,1) PRIMARY KEY,
    titulo              VARCHAR(150) NOT NULL,
    descricao           VARCHAR(1000),
    status              VARCHAR(20),
    visibilidade        BIT NOT NULL DEFAULT 1,   -- 1 = visivel para o cliente
    data_hora_inicio    DATETIME2,
    data_hora_prevista  DATETIME2,
    data_hora_fim       DATETIME2,
    projeto_codigo      INT NOT NULL,
    categoria_codigo    INT NOT NULL,
    CONSTRAINT FK_Tarefas_Projetos
        FOREIGN KEY (projeto_codigo) REFERENCES Projetos(codigo),
    CONSTRAINT FK_Tarefas_Categorias
        FOREIGN KEY (categoria_codigo) REFERENCES Categorias(codigo)
);

-- Entidade associativa: N:N entre Funcionarios e Tarefas
-- (com atributo responsavel -> tabela associativa)
CREATE TABLE Tarefas_Funcionarios (
    tarefa_codigo       INT NOT NULL,
    funcionario_codigo  INT NOT NULL,
    responsavel         BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (tarefa_codigo, funcionario_codigo),
    CONSTRAINT FK_TF_Tarefas
        FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo),
    CONSTRAINT FK_TF_Funcionarios
        FOREIGN KEY (funcionario_codigo) REFERENCES Funcionarios(codigo)
);

-- Relacionamento "possui": 1 Tarefa -> N Comentarios
-- Relacionamento "tem":    1 Tarefas_Funcionarios -> N Comentarios
-- A FK composta reaproveita tarefa_codigo, garantindo que so
-- comenta na tarefa quem esta alocado nela.
CREATE TABLE Comentarios (
    codigo              INT IDENTITY(1,1) PRIMARY KEY,
    titulo              VARCHAR(150),
    descricao           VARCHAR(1000) NOT NULL,
    data_hora           DATETIME2 NOT NULL DEFAULT GETDATE(),
    tarefa_codigo       INT NOT NULL,
    funcionario_codigo  INT NOT NULL,
    CONSTRAINT FK_Comentarios_Tarefas
        FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo),
    CONSTRAINT FK_Comentarios_TF
        FOREIGN KEY (tarefa_codigo, funcionario_codigo)
        REFERENCES Tarefas_Funcionarios(tarefa_codigo, funcionario_codigo)
);

-- Relacionamento "tem": 1 Tarefa -> N Documentos
CREATE TABLE Documentos (
    codigo         INT IDENTITY(1,1) PRIMARY KEY,
    descricao      VARCHAR(255),
    extensao       VARCHAR(10)  NOT NULL,
    diretorio      VARCHAR(500) NOT NULL,
    tarefa_codigo  INT NOT NULL,
    CONSTRAINT FK_Documentos_Tarefas
        FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo)
);
GO