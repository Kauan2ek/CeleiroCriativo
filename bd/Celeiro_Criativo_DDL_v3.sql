-- =====================================================================
-- CELEIRO CRIATIVO - SCRIPT DE CRIAÇÃO DO BANCO (SQL Server)
-- v3 - Baseado no DER atualizado (2026-09-30) e no Celeiro_Criativo_DDL_v2.sql
-- Ordem de criação respeita as dependências de chave estrangeira.
-- Autor: Kauan Dias - 17/09/2026
--
-- Atualizado por Claude em 2026-09-30 com base no DER mais recente:
--   1) Projetos ganhou relação "tem" com Status (N:1) — coluna
--      status_codigo já existia, mas faltava a FK. Adicionada
--      FK_Projetos_Status.
--   2) TarefaFuncionarios: a coluna projeto_codigo tinha sido removida
--      numa edição manual anterior, o que quebrava a relação ternária
--      Funcionário x Projeto x Tarefa que o DER ainda mostra ("tem"
--      entre Tarefas_funcionarios e Projetos). Restaurada.
--   3) Comentarios.funcionario_codigo -> Funcionarios mantido como na
--      edição manual (confirmado com o usuário) — só a constraint foi
--      renomeada de FK_Comentarios_Pessoa para FK_Comentarios_Funcionario
--      pra não ficar com nome de "Pessoa" apontando pra Funcionarios.
-- =====================================================================

-- ---------------------------------------------------------------------
-- TABELAS SEM DEPENDÊNCIA
-- ---------------------------------------------------------------------
use CeleiroCriativo
-- ATENÇÃO: "documento" e "tipo de documento" migraram de Cliente para
-- Pessoa nesta versão do DER -> agora TODA pessoa (funcionário ou
-- cliente) tem CPF/CNPJ cadastrado, não só o cliente.
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

-- ATENÇÃO: "salário" saiu do modelo; entrou "ativo" (funcionário
-- ativo/inativo na empresa).
CREATE TABLE Funcionarios (
    pessoa_codigo   INT PRIMARY KEY,
    cargo_codigo    INT NOT NULL,
    ativo           BIT NOT NULL DEFAULT 1,
    email           VARCHAR(100) NOT NULL,
    senha           VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Funcionarios_Pessoa FOREIGN KEY (pessoa_codigo) REFERENCES Pessoas(codigo),
    CONSTRAINT FK_Funcionarios_Cargo  FOREIGN KEY (cargo_codigo)  REFERENCES Cargos(codigo)
);

-- ATENÇÃO: "codigo de verificação" é novo (provavelmente usado para
-- confirmar e-mail/telefone no cadastro). Assumi VARCHAR(20); ajuste se
-- o formato real for diferente (ex.: sempre numérico de 6 dígitos).
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

-- ATENÇÃO (v2): datas em nível de Projeto (data_inicio, data_fim,
-- data_prevista). Usei DATE (sem hora), diferente de Tarefas que usa
-- DATETIME - o rótulo no DER é "data" e não "data hora" para Projeto.
-- Ajuste pra DATETIME se precisar registrar hora também.
-- ATENÇÃO (2026-09-30): status_codigo é novo no DER — Projeto agora tem
-- status próprio, independente do status de cada Tarefa.
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
-- TAREFAS (depende de Status e Categorias)
-- ---------------------------------------------------------------------

-- ATENÇÃO: "visibilidade" é novo e ambíguo no DER (só o nome do atributo,
-- sem enum visível). Assumi BIT (1 = visível ao cliente, 0 = interno).
-- Se for algo com mais de dois estados (ex.: público/equipe/privado),
-- troque para VARCHAR ou uma FK pra tabela de domínio própria.
-- ATENÇÃO (v3): categoria_codigo mantido aqui (FK Tarefas -> Categorias)
-- por decisão explícita do usuário, mesmo o DER visual sugerindo a
-- direção oposta (FK em Categorias -> Tarefas). Ver cabeçalho do arquivo.
CREATE TABLE Tarefas (
    codigo              INT IDENTITY(1,1) PRIMARY KEY,
    status_codigo       INT NOT NULL,
    categoria_codigo    INT NULL,
    titulo              VARCHAR(50) NOT NULL,
    descricao           VARCHAR(MAX) NULL,
    datahora_inicio     DATETIME NULL,
    datahora_fim        DATETIME NULL,
    datahora_prevista   DATETIME NULL,
    visibilidade        BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Tarefas_Status    FOREIGN KEY (status_codigo)    REFERENCES Status(codigo),
    CONSTRAINT FK_Tarefas_Categoria FOREIGN KEY (categoria_codigo) REFERENCES Categorias(codigo)
);

-- ---------------------------------------------------------------------
-- TAREFAFUNCIONARIOS (relação ternária: Funcionário x Projeto x Tarefa)
-- ATENÇÃO (v3): renomeada de "Projeto_Tarefas" (v2) para
-- "TarefaFuncionarios", a pedido do usuário — mesmo nome/formato usado
-- no DER atual ("Tarefas_funcionarios"), só que sem underscore para
-- seguir o padrão das demais tabelas.
-- ATENÇÃO (2026-09-30): projeto_codigo restaurado — o DER ainda mostra
-- "tem" entre Tarefas_funcionarios e Projetos, então a relação continua
-- ternária (Funcionário x Projeto x Tarefa), não só Funcionário x Tarefa.
-- ---------------------------------------------------------------------

CREATE TABLE TarefaFuncionarios (
    funcionario_codigo  INT NOT NULL,
    projeto_codigo      INT NOT NULL,
    tarefa_codigo       INT NOT NULL,
    responsavel         BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_TarefaFuncionarios PRIMARY KEY (funcionario_codigo, projeto_codigo, tarefa_codigo),
    CONSTRAINT FK_TarefaFuncionarios_Funcionario FOREIGN KEY (funcionario_codigo) REFERENCES Funcionarios(pessoa_codigo),
    CONSTRAINT FK_TarefaFuncionarios_Projeto     FOREIGN KEY (projeto_codigo)     REFERENCES Projetos(codigo),
    CONSTRAINT FK_TarefaFuncionarios_Tarefa      FOREIGN KEY (tarefa_codigo)      REFERENCES Tarefas(codigo)
);

-- ---------------------------------------------------------------------
-- DOCUMENTOS (depende de Tarefas — cada documento pertence a uma tarefa
-- específica, não ao projeto como um todo)
-- ---------------------------------------------------------------------

CREATE TABLE Documentos (
    codigo          INT IDENTITY(1,1) PRIMARY KEY,
    tarefa_codigo   INT NOT NULL,
    descricao       VARCHAR(MAX) NULL,
    extensao        VARCHAR(10) NULL, -- ex.: 'pdf', 'docx', 'png' (antes era "tipo")
    diretorio       VARCHAR(255) NULL,
    CONSTRAINT FK_Documentos_Tarefa FOREIGN KEY (tarefa_codigo) REFERENCES Tarefas(codigo)
);

-- ---------------------------------------------------------------------
-- COMENTARIOS (depende de Tarefas e Funcionarios)
-- ATENÇÃO (2026-09-30): o autor do comentário é Funcionario (não mais
-- Pessoa/Cliente) — decisão confirmada com o usuário, mesmo o DER não
-- mostrando explicitamente essa relação. Se um dia o cliente também
-- precisar comentar, isso volta a apontar pra Pessoas.
-- ---------------------------------------------------------------------

CREATE TABLE Comentarios (
    codigo              INT IDENTITY(1,1) PRIMARY KEY,
    tarefa_codigo        INT NOT NULL,
    funcionario_codigo   INT NOT NULL,
    titulo               VARCHAR(50) NULL,
    descricao            VARCHAR(MAX) NULL,
    data_hora            DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Comentarios_Tarefa      FOREIGN KEY (tarefa_codigo)      REFERENCES Tarefas(codigo),
    CONSTRAINT FK_Comentarios_Funcionario FOREIGN KEY (funcionario_codigo) REFERENCES Funcionarios(pessoa_codigo)
);
