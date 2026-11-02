# Pendências — Tarefa / Comentário

Anotado em 19/10/2026. Pontos encontrados na revisão da tela de tarefa que ficaram para depois.

## 1. Remover alguém da equipe apaga os comentários dessa pessoa na tarefa

- **Onde:** `Repositories/FuncionarioSqlRepository.cs`, método `AtualizarFuncionarios`, no laço `remover`.
- **Por quê:** a FK `FK_Comentarios_TarefaFuncionario` (DDL v4) liga cada comentário ao par (funcionário, tarefa) da `TarefaFuncionarios`. O banco não deixa apagar a linha da equipe enquanto existir comentário dela, então o código apaga os comentários antes.
- **Decidir:** se o histórico do chat precisa ficar, a FK de `Comentarios` tem que voltar a apontar só para `Funcionarios(pessoa_codigo)`, como era no DDL v3. Isso também deixaria o gestor comentar sem estar na equipe.

## 2. O autor provisório do comentário é o primeiro da equipe em ordem alfabética

- **Onde:** `Views/Tarefa/DetalhesGestor.cshtml`, hidden `Funcionario.Codigo` do chat (`Model.Funcionarios[0].Codigo`).
- **Por quê:** o `FuncionariosTarefa` usa `ORDER BY P.nome`, então o comentário fica no nome de quem vem primeiro no alfabeto, não de quem escreveu.
- **Resolver quando:** existir login. Aí o valor passa a ser o funcionário logado.

## 3. `ReadAll` em JSON devolve `sucesso = false` mesmo quando encontra tarefas

- **Onde:** `Controllers/TarefaController.cs`, action `ReadAll`, último `return Json(new {sucesso = false, tarefas})`.
- **Correção:** trocar para `sucesso = true`. Hoje nenhuma tela usa essa action (o kanban usa a `IndexGestor`), por isso não aparece.

## 4. O `Create` do repositório não fecha a conexão do `FuncionarioSqlRepository`

- **Onde:** `Repositories/TarefaSqlRepository.cs`, método `Create`: `new FuncionarioSqlRepository().AtualizarFuncionarios(...)`.
- **Por quê:** o repositório não fica numa variável, então ninguém chama o `Dispose()` dele. O `Update` já foi corrigido com o mesmo padrão (variável + `Dispose()`); o `Create` ainda não.

## 5. Cache do navegador com JS/CSS antigo

- **Onde:** `<script src="/js/exibicao_tarefa.js">` e o `<link>` do CSS no `DetalhesGestor.cshtml`.
- **Por quê:** foi a causa do comentário que "não gravava" (o navegador rodava o JS antigo, com `preventDefault()`).
- **Correção:** acrescentar `?v=2` no endereço e trocar o número a cada mudança no arquivo.

## 6. Perfil: falta regra de senha (CT-10 / DEF-08 do documento)

Anotado em 02/11/2026.

- **Onde:** `Controllers/PerfilController.cs`, actions `SalvarFuncionario` e `SalvarCliente`, logo depois do `if` que compara `Senha` com `ConfirmarSenha`.
- **Por quê:** o CT-10 ("Editar perfil com senha inválida") foi reprovado porque o sistema aceitou a senha `123`. Hoje o perfil só confere se a senha e a confirmação são iguais.
- **Decidir:** qual é o padrão (ex.: mínimo de 6 caracteres, ou mínimo de 8 com letra e número). Lembrar que os usuários de teste usam `123456`.
- **Aplicar também:** no cadastro do cliente (`ClienteController.Create`) e do funcionário (`FuncionarioController.Salvar`), pra regra ser a mesma em todo lugar.

## 7. Perfil: gestor pode desativar a si mesmo

- **Onde:** `Controllers/PerfilController.cs`, action `Desativar`.
- **Por quê:** se o único gestor se desativar, ninguém mais consegue entrar como gestor (o login bloqueia `ativo = 0`). Só dá pra reativar direto no banco (`UPDATE Funcionarios SET ativo = 1 WHERE pessoa_codigo = X`).
- **Decidir:** se o botão Desativar deve sumir do perfil do gestor ou se basta o aviso do `confirm`.
