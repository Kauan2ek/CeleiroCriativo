use CeleiroCriativo
go

-- Views para cada tabela

-- 1. Visualiza��o da tabela cargos

create or alter view v_cargos
as
	select codigo, descricao
	from Cargos

-- Teste de execu��o

select * from v_cargos

-- 2. Visualiza��o da tabela status

create or alter view v_status
as
	select codigo, descricao
	from status

-- Teste de execu��o

select * from v_status

-- 3. Visualiza��o da tabela Categorias

create or alter view v_categorias
as
	select codigo, descricao, cor
	from Categorias

-- Teste de execu��o

select * from v_categorias

-- 4. Visualiza��o da tabela projetos

create or alter view v_projetos
as
	select codigo, cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo
	from Projetos

-- Teste de execu��o

select * from v_projetos

-- 5. Visualiza��o da tabela

create or alter view v_tarefas
as
	select codigo, projeto_codigo, categoria_codigo, titulo, descricao, status, datahora_inicio, datahora_fim, datahora_prevista, visibilidade
	from Tarefas

-- Teste de execu��o

select * from v_tarefas

-- 6. Visualiza��o da tabela TarefaFuncionarios
-- Obs: n�o sei se tabela N:N conta

create or alter view v_tarefafuncionarios
as
	select funcionario_codigo, tarefa_codigo
	from TarefaFuncionarios

-- Teste de execu��o

select * from v_tarefafuncionarios

-- 7. Visualiza��o da tabela Documentos

create or alter view v_documentos
as
	select codigo, tarefa_codigo, descricao, extensao, diretorio
	from Documentos

-- Teste de execu��o

select * from v_documentos

-- 8. Visualiza��o da tabela Comentarios

create or alter view v_comentarios
as
	select codigo, tarefa_codigo, funcionario_codigo, descricao, data_hora
	from Comentarios

-- Teste de execu��o

select * from v_comentarios

-- 9. Visualiza��o da tabela Funcion�rios

create or alter view v_funcionarios
as
	select pessoa_codigo, cargo_codigo, ativo, email, senha
	from Funcionarios

-- Teste de execu��o

select * from v_funcionarios

-- 10. Visualiza��o da tabela Clientes

create or alter view v_clientes
as
	select pessoa_codigo, codigo_verificacao, email, senha
	from Clientes

-- Teste de execu��o

select * from v_clientes

-- Views usando join

-- 1. Visualiza��o da tabela Funcion�rios com Pessoas

create or alter view v_funcionarios2
as
	select f.pessoa_codigo, p.nome, p.telefone, p.documento, p.tipo_doc, f.cargo_codigo, c.descricao as cargo, f.ativo, f.email
    from Funcionarios as f
    inner join Pessoas as p on p.codigo = f.pessoa_codigo
    inner join Cargos as c on c.codigo = f.cargo_codigo
go
-- Teste de execu��o

select * from v_funcionarios2

-- 2. Visualiza��o da tabela Clientes com Pessoas

create or alter view v_clientes2
as
	select c.pessoa_codigo, p.nome, p.telefone, p.documento, p.tipo_doc, c.codigo_verificacao, c.email
    from Clientes as c
    inner join Pessoas as p on p.codigo = c.pessoa_codigo
go

-- Teste de execu��o

select * from v_clientes2

-- 3. Visualiza��o da tabela Tarefas com Categorias

create or alter view V_Tarefas2
as
select A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
       A.datahora_fim, A.visibilidade, A.status,
       A.projeto_codigo, A.categoria_codigo, 
       B.descricao categoria_descricao, B.cor categoria_cor
from tarefas A
inner join Categorias B on B.codigo = A.categoria_codigo
where A.status <> 'Cancelada'
GO

-- Teste de execu��o

select * from V_Tarefas2

-- 4. Visualiza��o da tabela Funcion�rios com Pessoas e Cargos

CREATE OR ALTER VIEW V_FuncionariosCargos
AS 
SELECT B.codigo, B.nome, b.telefone, b.documento, B.tipo_doc,
	   A.email, A.senha,
	   C.codigo cargo_codigo, C.descricao cargo_descricao
FROM Funcionarios A 
INNER JOIN Pessoas B ON A.pessoa_codigo = B.codigo
INNER JOIN Cargos  C ON C.codigo        = A.cargo_codigo
GO

-- Teste de execu��o

select * from V_FuncionariosCargos

-- 5. Visualiza��o da tabela Coment�rios com Funcion�rios

CREATE OR ALTER VIEW V_Comentarios2
AS 
SELECT A.*,
	   B.nome funcionario_nome, B.Telefone funcionario_telefone,
	   B.email funcionario_email,
	   B.cargo_codigo, B.cargo_descricao
FROM Comentarios A 
-- [autor-comentario] o join era "B.codigo = A.codigo" (c�digo do coment�rio), o que trazia o funcion�rio errado
INNER JOIN V_FuncionariosCargos B ON B.codigo = A.funcionario_codigo
GO

-- Teste de execu��o

select * from V_Comentarios2

-- 6. Visualizacao da tabela Projetos com Clientes, Pessoas e Status

create or alter view V_Projetos_Clientes
as
    select pr.codigo as projeto_codigo, pr.titulo as projeto_titulo, pr.descricao as projeto_descricao,
		   pr.data_inicio, pr.data_fim, pr.data_prevista, c.pessoa_codigo as cliente_codigo, p.nome as cliente_nome,
		   p.documento as cliente_documento, pr.status_codigo, s.descricao as status_descricao
    from Projetos as pr
    inner join Clientes as c on c.pessoa_codigo = pr.cliente_codigo
    inner join Pessoas as p on p.codigo = c.pessoa_codigo
    left join Status as s on s.codigo = pr.status_codigo
go

-- Teste de execucao

select * from V_Projetos_Clientes
go


-- 7. Visualiza��o da tabela Tarefas com Funcionarios, Pessoas, Tarefas e Projetos

create or alter view V_Tarefas_Funcionarios
as
    select tf.funcionario_codigo, p.nome as funcionario_nome, f.email as funcionario_email, tf.tarefa_codigo,
           t.titulo as tarefa_titulo, t.status as tarefa_status, t.datahora_inicio, t.datahora_prevista,
           t.datahora_fim, pr.codigo as projeto_codigo, pr.titulo as projeto_titulo
    from TarefaFuncionarios as tf
    inner join Funcionarios as f on f.pessoa_codigo = tf.funcionario_codigo
    inner join Pessoas as p on p.codigo = f.pessoa_codigo
    inner join Tarefas as t on t.codigo = tf.tarefa_codigo
    inner join Projetos as pr on pr.codigo = t.projeto_codigo
go

-- Teste de execucao

select * from V_Tarefas_Funcionarios
go

-- Anota��es

select * from v_cargos
select * from v_status
select * from v_categorias
select * from v_projetos
select * from v_tarefas
select * from v_tarefafuncionarios
select * from v_documentos
select * from v_funcionarios
select * from v_clientes

select * from v_funcionarios2
select * from v_clientes2
select * from V_Tarefas2
select * from V_Comentarios2
select * from V_Projetos_Clientes
select * from V_Tarefas_Funcionarios