create view V_Tarefas
as
select A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
       A.datahora_fim, A.visibilidade, A.status,
       A.projeto_codigo, A.categoria_codigo, 
       B.descricao categoria_descricao, B.cor categoria_cor
from tarefas A
inner join Categorias B on B.codigo = A.categoria_codigo

