CREATE OR ALTER FUNCTION dbo.F_FuncionariosDaTarefa (@tarefa INT)
RETURNS TABLE
AS
RETURN
(
	SELECT A.funcionario_codigo codigo, C.nome, B.email, B.senha, 
		   C.telefone, C.tipo_doc tipo_documento, C.documento, B.ativo,
		   B.cargo_codigo, D.descricao cargo_descricao
	FROM TarefaFuncionarios A
	INNER JOIN Funcionarios B ON A.funcionario_codigo = B.pessoa_codigo
	INNER JOIN Pessoas      C ON C.codigo             = B.pessoa_codigo
	INNER JOIN Cargos       D ON D.codigo             = B.cargo_codigo
	WHERE A.tarefa_codigo = @tarefa
);
GO 