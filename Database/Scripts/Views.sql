create or alter view V_Tarefas
as
select A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
       A.datahora_fim, A.visibilidade, A.status,
       A.projeto_codigo, A.categoria_codigo, 
       B.descricao categoria_descricao, B.cor categoria_cor
from tarefas A
inner join Categorias B on B.codigo = A.categoria_codigo
where A.status <> 'Cancelada'
GO

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
CREATE OR ALTER VIEW V_Comentarios
AS 
SELECT A.*,
	   B.nome funcionario_nome, B.Telefone funcionario_telefone,
	   B.email funcionario_email,
	   B.cargo_codigo, B.cargo_descricao
FROM Comentarios A 
-- [autor-comentario] o join era "B.codigo = A.codigo" (código do comentário), o que trazia o funcionário errado
INNER JOIN V_Funcionarios B ON B.codigo = A.funcionario_codigo
GO
