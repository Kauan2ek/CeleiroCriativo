-- !!! AO ALTERAR ESTE ARQUIVO, ATUALIZE TAMBÉM O Celeiro_Criativo_Completo.sql !!!
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
--   4) Status possui os 3 estados de projeto e o status 'Excluído' usado na exclusão lógica.
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
    ('Finalizado'),         -- 3
    ('Excluído');           -- 4 - usado para exclusão lógica de Projeto

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

-- Mesmo projetos em análise podem ter senha: o cadastro da landing page já salva a senha.
INSERT INTO Clientes (pessoa_codigo, codigo_verificacao, email, senha) VALUES
    (6,  'A1B2C3', 'contato@cafeaurora.exemplo.com',    '123456'),
    (7,  'D4E5F6', 'contato@ecoverde.exemplo.com',      '123456'),
    (8,  'G7H8I9', 'contato@tecnova.exemplo.com',       '123456'),
    (9,  'J1K2L3', 'contato@floresdocampo.exemplo.com', '123456'),
    (10, 'M4N5O6', 'marina@exemplo.com',                '123456');

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
