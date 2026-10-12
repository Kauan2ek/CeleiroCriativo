-- =====================================================================
-- CELEIRO CRIATIVO - SCRIPT DE DADOS DE TESTE (SQL Server)
-- Inserts para o banco criado pelo Celeiro_Criativo_DDL_v4.sql
-- Ordem de inserção respeita as dependências de chave estrangeira.
--
-- ATENÇÃO:
--   1) Rodar em um banco RECÉM-CRIADO (tabelas vazias). Os códigos das
--      FKs abaixo assumem que o IDENTITY de cada tabela começa em 1.
--   2) Todos os dados são fictícios (nomes, documentos, e-mails,
--      telefones e diretórios de arquivos).
--   3) As senhas estão em texto puro ('123456'), apenas para teste.
--   4) Status tem só 3 registros: são os 3 status de projeto definidos.
--   5) Datas no formato ISO ('AAAAMMDD' e 'AAAA-MM-DDTHH:MM:SS'), que
--      não depende do idioma configurado no SQL Server.
--   6) Tudo roda em uma transação: se um insert falhar, nada é gravado.
-- =====================================================================

USE CeleiroCriativo;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ---------------------------------------------------------------------
-- TABELAS SEM DEPENDÊNCIA
-- ---------------------------------------------------------------------

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
    ('Finalizado');         -- 3

INSERT INTO Categorias (descricao, cor) VALUES
    ('Design',          '#4A90D9'),   -- 1
    ('Desenvolvimento', '#7ED321'),   -- 2
    ('Vídeo',           '#D0021B'),   -- 3
    ('Redação',         '#F5A623'),   -- 4
    ('Revisão',         '#9013FE');   -- 5

-- ---------------------------------------------------------------------
-- SUBTIPOS DE PESSOA
-- ---------------------------------------------------------------------

INSERT INTO Funcionarios (pessoa_codigo, cargo_codigo, ativo, email, senha) VALUES
    (1, 1, 1, 'helena@celeirocriativo.com',  '123456'),   -- Gestor
    (2, 2, 1, 'rafael@celeirocriativo.com',  '123456'),   -- Designer
    (3, 3, 1, 'bruno@celeirocriativo.com',   '123456'),   -- Programador
    (4, 4, 1, 'larissa@celeirocriativo.com', '123456'),   -- Editor de Vídeo
    (5, 5, 1, 'tiago@celeirocriativo.com',   '123456');   -- Redator

-- Cliente 10 ainda não tem senha: o projeto dele está "Em análise".
INSERT INTO Clientes (pessoa_codigo, codigo_verificacao, email, senha) VALUES
    (6,  'A1B2C3', 'contato@cafeaurora.exemplo.com',    '123456'),
    (7,  'D4E5F6', 'contato@ecoverde.exemplo.com',      '123456'),
    (8,  'G7H8I9', 'contato@tecnova.exemplo.com',       '123456'),
    (9,  'J1K2L3', 'contato@floresdocampo.exemplo.com', '123456'),
    (10, 'M4N5O6', 'marina@exemplo.com',                NULL);

-- ---------------------------------------------------------------------
-- PROJETOS (depende de Clientes e Status)
-- ---------------------------------------------------------------------

INSERT INTO Projetos (cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo) VALUES
    (6,  'Identidade visual Café Aurora',   'Criação do logotipo e do manual da marca da cafeteria.',       '20260803', '20260828', '20260831', 3),  -- 1
    (7,  'Site institucional EcoVerde',     'Site de apresentação da empresa, com página de contato.',      '20260901', NULL,       '20261030', 2),  -- 2
    (8,  'Campanha de lançamento TecNova',  'Vídeo promocional e artes para as redes sociais.',             '20260921', NULL,       '20261120', 2),  -- 3
    (9,  'Catálogo digital Flores do Campo','Catálogo em PDF com os arranjos e preços da floricultura.',    '20260706', '20260724', '20260731', 3),  -- 4
    (10, 'Loja virtual Ateliê Marina',      'Ideia enviada pelo site: loja virtual para peças artesanais.', NULL,       NULL,       NULL,       1);  -- 5

-- ---------------------------------------------------------------------
-- TAREFAS (depende de Projetos e Categorias)
-- status: 'A fazer', 'Em andamento' ou 'Finalizado'
-- O projeto 5 está "Em análise" e por isso ainda não tem tarefas.
-- ---------------------------------------------------------------------

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

-- ---------------------------------------------------------------------
-- TAREFAFUNCIONARIOS (entidade associativa: Funcionário x Tarefa)
-- ---------------------------------------------------------------------

INSERT INTO TarefaFuncionarios (funcionario_codigo, tarefa_codigo, responsavel) VALUES
    (2, 1,  1),
    (2, 2,  1),
    (5, 2,  0),
    (2, 3,  1),
    (3, 4,  1),
    (5, 5,  1),
    (5, 6,  1),
    (4, 7,  1),
    (2, 7,  0),
    (2, 8,  1),
    (2, 9,  1),
    (5, 10, 1),
    (1, 10, 0);

-- ---------------------------------------------------------------------
-- DOCUMENTOS (depende de Tarefas)
-- ---------------------------------------------------------------------

INSERT INTO Documentos (tarefa_codigo, descricao, extensao, diretorio) VALUES
    (1, 'Logotipo aprovado',             'png',  '/uploads/tarefa-1/logotipo-cafe-aurora.png'),
    (2, 'Manual da marca',               'pdf',  '/uploads/tarefa-2/manual-da-marca.pdf'),
    (3, 'Layout das páginas',            'pdf',  '/uploads/tarefa-3/layout-site-ecoverde.pdf'),
    (6, 'Roteiro do vídeo promocional',  'docx', '/uploads/tarefa-6/roteiro-video-tecnova.docx'),
    (7, 'Prévia do vídeo (primeiro corte)', 'mp4', '/uploads/tarefa-7/previa-video-tecnova.mp4'),
    (9, 'Catálogo diagramado',           'pdf',  '/uploads/tarefa-9/catalogo-flores-do-campo.pdf');

-- ---------------------------------------------------------------------
-- COMENTARIOS (depende de Tarefas e TarefaFuncionarios)
-- O par (funcionario_codigo, tarefa_codigo) precisa existir em
-- TarefaFuncionarios: só comenta quem está alocado na tarefa.
-- ---------------------------------------------------------------------

INSERT INTO Comentarios (tarefa_codigo, funcionario_codigo, descricao, data_hora) VALUES
    (1,  2, 'Subi as três propostas de logotipo para aprovação.',            '2026-08-10T14:20:00'),
    (2,  5, 'Revisei os textos do manual, sem pendências.',                  '2026-08-27T10:05:00'),
    (4,  3, 'Página inicial implementada, faltam sobre e contato.',          '2026-10-06T16:40:00'),
    (7,  4, 'Primeiro corte do vídeo disponível nos documentos.',            '2026-10-09T11:15:00'),
    (7,  2, 'Enviei a vinheta com o logotipo para usar na abertura.',        '2026-10-09T15:30:00'),
    (10, 1, 'Revisão conferida, catálogo aprovado para entrega ao cliente.', '2026-07-24T11:30:00');

COMMIT TRANSACTION;
