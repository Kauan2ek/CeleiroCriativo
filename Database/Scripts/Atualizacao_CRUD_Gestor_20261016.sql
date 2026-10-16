-- =====================================================================
-- CELEIRO CRIATIVO - AJUSTE PARA OS CRUDs DO GESTOR
-- 16/10/2026
--
-- Use este script somente se o banco v4 JÁ EXISTIR e você não quiser
-- recriá-lo do zero com DDL_v4 + Inserts_v4.
-- =====================================================================

USE CeleiroCriativo;
GO

-- A exclusão de Projeto é lógica e depende deste status.
-- O IF evita duplicar o registro caso ele já tenha sido inserido.
IF NOT EXISTS (SELECT 1 FROM Status WHERE descricao = 'Excluído')
BEGIN
    INSERT INTO Status (descricao) VALUES ('Excluído');
END;
GO

-- Mantém a View das tarefas coerente com a exclusão lógica de Tarefa.
CREATE OR ALTER VIEW V_Tarefas
AS
SELECT A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
       A.datahora_fim, A.visibilidade, A.status,
       A.projeto_codigo, A.categoria_codigo,
       B.descricao AS categoria_descricao, B.cor AS categoria_cor
FROM Tarefas A
INNER JOIN Categorias B ON B.codigo = A.categoria_codigo
WHERE A.status <> 'Cancelada';
GO
