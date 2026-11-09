use CeleiroCriativo
go

-- Views do documento interdisciplinar (seção 4.7 Views)

-- 1. Visualização da tabela Cargos

create or alter view v_cargos
as
	select codigo, descricao
	from Cargos
go

-- Teste de execução

select * from v_cargos

-- 2. Visualização da tabela Status

create or alter view v_status
as
	select codigo, descricao
	from status
go

-- Teste de execução

select * from v_status

-- 3. Visualização da tabela Categorias

create or alter view v_categorias
as
	select codigo, descricao, cor
	from Categorias
go

-- Teste de execução

select * from v_categorias

-- 4. Visualização da tabela Projetos

create or alter view v_projetos
as
	select codigo, cliente_codigo, titulo, descricao, data_inicio, data_fim, data_prevista, status_codigo
	from Projetos
go

-- Teste de execução

select * from v_projetos

-- 5. Visualização da tabela Tarefas

create or alter view v_tarefas
as
	select codigo, projeto_codigo, categoria_codigo, titulo, descricao, status, datahora_inicio, datahora_fim, datahora_prevista, visibilidade
	from Tarefas
go

-- Teste de execução

select * from v_tarefas

-- 6. Visualização da tabela TarefaFuncionarios

create or alter view v_tarefafuncionarios
as
	select funcionario_codigo, tarefa_codigo
	from TarefaFuncionarios
go

-- Teste de execução

select * from v_tarefafuncionarios

-- 7. Visualização da tabela Documentos

create or alter view v_documentos
as
	select codigo, tarefa_codigo, descricao, extensao, diretorio
	from Documentos
go

-- Teste de execução

select * from v_documentos

-- 8. Visualização da tabela Comentarios

create or alter view v_comentarios
as
	select codigo, tarefa_codigo, funcionario_codigo, descricao, data_hora
	from Comentarios
go

-- Teste de execução

select * from v_comentarios

-- 9. Visualização da tabela Funcionários

create or alter view v_funcionarios
as
	select pessoa_codigo, cargo_codigo, ativo, email, senha
	from Funcionarios
go

-- Teste de execução

select * from v_funcionarios

-- 10. Visualização da tabela Clientes

create or alter view v_clientes
as
	select pessoa_codigo, codigo_verificacao, email, senha
	from Clientes
go

-- Teste de execução

select * from v_clientes

-- 11. Visualização da tabela Funcionários com Pessoas

create or alter view v_funcionariosPessoas
as
	select f.pessoa_codigo, p.nome, p.telefone, p.documento, p.tipo_doc, f.cargo_codigo, c.descricao as cargo, f.ativo, f.email
	from Funcionarios as f
	inner join Pessoas as p on p.codigo = f.pessoa_codigo
	inner join Cargos as c on c.codigo = f.cargo_codigo
go

-- Teste de execução

select * from v_funcionariosPessoas

-- 12. Visualização da tabela Clientes com Pessoas

create or alter view v_clientesPessoas
as
	select c.pessoa_codigo, p.nome, p.telefone, p.documento, p.tipo_doc, c.codigo_verificacao, c.email
	from Clientes as c
	inner join Pessoas as p on p.codigo = c.pessoa_codigo
go

-- Teste de execução

select * from v_clientesPessoas

-- 13. Visualização da tabela Tarefas com Categorias

create or alter view V_TarefasJoin
as
select A.codigo, A.titulo, A.descricao, A.datahora_inicio, A.datahora_prevista,
       A.datahora_fim, A.visibilidade, A.status,
       A.projeto_codigo, A.categoria_codigo,
       B.descricao categoria_descricao, B.cor categoria_cor
from tarefas A
inner join Categorias B on B.codigo = A.categoria_codigo
where A.status <> 'Cancelada'
go

-- Teste de execução

select * from V_TarefasJoin

-- 14. Visualização da tabela Funcionários com Pessoas e Cargos

CREATE OR ALTER VIEW V_FuncionariosCargos
AS
SELECT B.codigo, B.nome, b.telefone, b.documento, B.tipo_doc,
	   A.email, A.senha,
	   C.codigo cargo_codigo, C.descricao cargo_descricao
FROM Funcionarios A
INNER JOIN Pessoas B ON A.pessoa_codigo = B.codigo
INNER JOIN Cargos  C ON C.codigo        = A.cargo_codigo
GO

-- Teste de execução

select * from V_FuncionariosCargos

-- 15. Visualização da tabela Comentários com Funcionários

CREATE OR ALTER VIEW V_ComentariosCompleto
AS
SELECT A.*,
	   B.nome funcionario_nome, B.Telefone funcionario_telefone,
	   B.email funcionario_email,
	   B.cargo_codigo, B.cargo_descricao
FROM Comentarios A
INNER JOIN V_FuncionariosCargos B ON B.codigo = A.funcionario_codigo
GO

-- Teste de execução

select * from V_ComentariosCompleto

-- 16. Visualizacao da tabela Projetos com Clientes, Pessoas e Status

create or alter view V_ProjetosClientes
as
	select pr.codigo as projeto_codigo, pr.titulo as projeto_titulo, pr.descricao as projeto_descricao,
		   pr.data_inicio, pr.data_fim, pr.data_prevista, c.pessoa_codigo as cliente_codigo, p.nome as cliente_nome,
		   p.documento as cliente_documento, pr.status_codigo, s.descricao as status_descricao
	from Projetos as pr
	inner join Clientes as c on c.pessoa_codigo = pr.cliente_codigo
	inner join Pessoas as p on p.codigo = c.pessoa_codigo
	left join Status as s on s.codigo = pr.status_codigo
go

-- Teste de execução

select * from V_ProjetosClientes
go

-- 17. Visualização da tabela Tarefas com Funcionarios, Pessoas, Tarefas e Projetos

create or alter view V_TarefasFuncionarios
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

-- Teste de execução

select * from V_TarefasFuncionarios
go

-- Anotações

select * from v_cargos
select * from v_status
select * from v_categorias
select * from v_projetos
select * from v_tarefas
select * from v_tarefafuncionarios
select * from v_documentos
select * from v_comentarios
select * from v_funcionarios
select * from v_clientes

select * from v_funcionariosPessoas
select * from v_clientesPessoas
select * from V_TarefasJoin
select * from V_FuncionariosCargos
select * from V_ComentariosCompleto
select * from V_ProjetosClientes
select * from V_TarefasFuncionarios
