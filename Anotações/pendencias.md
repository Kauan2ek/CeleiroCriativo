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

---

# Pendências — teste geral da aplicação

Anotado em 02/11/2026, depois de testar todos os perfis, cadastros, alterações, exclusões, comentários, upload e perfil.
Os números entre parênteses são os do checklist do teste.

Legenda da coluna **Quebra?**: 🔴 a aplicação cai / dá erro 500 / o recurso não funciona · 🟠 funciona, mas é falha de segurança ou deixa dado errado · 🟢 melhoria.

## 8. CPF de funcionário no cadastro de cliente transforma o funcionário em cliente (item 3) 🔴

- **Onde:** procedure `CadastrarCliente` (`Database/Scripts/Procedures.sql`, linha do `SET @codigo = (SELECT codigo FROM Pessoas WHERE documento = @documento)`).
- **Por quê:** se o CPF já existe em `Pessoas`, a procedure reaproveita a pessoa e cria o cliente em cima dela. A mesma pessoa vira cliente e funcionário, e o login (`IIF(B.pessoa_codigo IS NOT NULL, 'cliente', ...)`) passa a tratá-la como cliente: o funcionário perde o acesso. O contrário também acontece pelo `FuncionarioController.Salvar` (CPF de um cliente vira funcionário na mesma pessoa).
- **Decidir:** se uma pessoa pode ser cliente e funcionário ao mesmo tempo. Se não puder, a procedure deve recusar (THROW) quando o CPF já existir.

## 9. Login mistura e-mail e senha das duas tabelas (item 4) 🟠

- **Onde:** `Repositories/UsuarioSqlRepository.cs`, `WHERE (B.email = @email OR C.email = @email) AND (B.senha = @senha OR C.senha = @senha)`.
- **Por quê:** para uma pessoa que é cliente e funcionário, aceita o e-mail de um com a senha do outro. Com e-mail repetido, entra quem vier primeiro (o funcionário 1008 `kauan@gmail.com` nunca entrava, porque o e-mail também estava em 6 clientes).
- **Correção:** comparar e-mail e senha da mesma tabela: `(B.email = @email AND B.senha = @senha) OR (C.email = @email AND C.senha = @senha)`.

## 10. Cliente abrindo projeto de outro cliente dá erro 500 (item 6) 🔴

- **Onde:** `Controllers/ProjetoController.cs`, action `DetalhesCliente`, no `return Index(); // Se não estiver vinculado...`.
- **Por quê:** o `repository.Dispose()` já rodou, e o `Index()` chama a `ClienteProjeto`, que usa o mesmo repositório com a conexão fechada. Só acontece digitando o número na URL (a tela só lista os projetos do próprio cliente). Não mostra dados de outro cliente, só quebra a página.
- **Correção:** trocar por `return RedirectToAction("Index", "Projeto");`, igual à `IndexCliente` do `TarefaController`.

## 11. Funcionário cadastrado com e-mail ou CPF repetido (item 9) 🟠

- **Onde:** `Controllers/FuncionarioController.cs`, action `Salvar`.
- **Por quê:** não confere se o e-mail já existe (nem em `Clientes`). No teste foi criado um funcionário com o e-mail `cliente@gmail`, e o login com aquela senha caiu no funcionário. Na edição também aceita o CPF de outra pessoa.
- **Correção:** usar o `ClienteSqlRepository.EmailExiste` igual ao `PerfilController.SalvarFuncionario` (só quando o e-mail mudou, na edição). O `UNIQUE` no banco cobre o CPF.

## 12. Funcionário desativado continua logado (item 10) 🟠

- **Onde:** `FuncionarioController.Excluir` + sessão.
- **Por quê:** o gestor desativa o funcionário, mas a sessão dele continua valendo até ele clicar em Sair.
- **Decidir:** se precisa resolver (ex.: conferir `ativo` no banco a cada tela) ou se basta o bloqueio no próximo login.

## 13. Funcionário vê e cria em qualquer projeto (item 11) 🟠

- **Onde:** `ProjetoController.DetalhesFuncionario`, `TarefaController.IndexFuncionario`, `DetalhesFuncionario` e `Create`.
- **Por quê:** trocando o número na URL, o funcionário abre qualquer projeto/tarefa, inclusive solicitações "Em análise" de outros clientes. E o `Create` da tarefa grava a equipe que vier no POST, apesar do comentário dizer que a tarefa do funcionário "nasce sem equipe".
- **Decidir:** se o funcionário deve ver só os projetos em que está na equipe. No `Create`, se a equipe deve ser limpa quando o papel for funcionário (`tarefa.Funcionarios = new List<Funcionario>();`).

## Restante do checklist

| # | Problema | Onde | Quebra? |
|---|---|---|---|
| 12 | Mensagens cruas do SQL Server aparecem pro usuário (ex.: *"The parameterized query… expects the parameter '@titulo'"*, erros de FK e CHECK). | catch do Projeto, Tarefa, Comentário, Funcionário | 🟢 |
| 13 | Sem validação no servidor: aceitou CPF "123"/"abc", e-mail inválido, nome > 50 caracteres (a procedure corta sem avisar, `@nome VARCHAR(50)`), status de tarefa inventado (some do kanban), data prevista antes do início na tarefa (o projeto tem CHECK, a tarefa não), projeto sem status e funcionário inativo na equipe. | Cliente/Funcionário/Tarefa/Projeto `Create` | 🟠 |
| 14 | Senha sem regra (aceitou "1") e guardada em texto puro. Ver pendência 6. | Perfil, cadastros | 🟠 |
| 15 | Gestor consegue se desativar. Ver pendência 7. | `PerfilController.Desativar` | 🟠 |
| 16 | Falha sem mensagem: Negar projeto com tarefas ou já em andamento, Aprovar de novo, Excluir inexistente (o `if (resultado)` não tem `else`). | `ProjetoController` | 🟢 |
| 17 | Gestor fora da equipe não comenta (erro de FK cru). Ver pendência 1. | `ComentarioController.Create` | 🟢 |
| 18 | Tarefa cancelada "ressuscita" com um `Update` mandado direto (o `UPDATE` não filtra `status <> 'Cancelada'`). | `TarefaSqlRepository.Update` | 🟢 |
| 19 | Erro no cadastro da landing page manda pra tela Entrar e o usuário perde o que digitou. | `ClienteController.Create` | 🟢 |
| 20 | O erro vai pela URL (`?erro=`), então qualquer link pode mostrar um texto falso na tela (é escapado, não é XSS). | Comentário/Documento → `Tarefa/Detalhes` | 🟢 |
| 21 | Lixo de teste no banco: status "TESTE2", categoria "Desenvolvimento" duplicada, clientes com e-mail repetido, cliente sem nome/e-mail, cliente sem senha; CPF gravado ora com pontuação, ora sem. | banco | 🟢 |
| 22 | Documentos do seed apontam para arquivos que não existem (404): `/documentos/atualizado.pdf`, `/uploads/tarefa-2/manual-da-marca.pdf`… | inserts | 🟢 |
| 23 | Upload pra tarefa inexistente deixa a pasta `tarefa-X` vazia (o `Directory.CreateDirectory` vem antes do banco). Arquivos em `wwwroot/uploads` baixam sem login. | `DocumentoController.Create` | 🟢 |
| 24 | `/imagens/entrar.png` é referenciado em minúscula, mas o arquivo é `Entrar.png`: funciona no Windows, quebra num servidor Linux. | `Views/Tarefa/DetalhesGestor.cshtml` (modal de equipe) | 🟢 / 🔴 no Linux |
| 25 | Formulários sem antiforgery token (o cookie `SameSite=Lax` reduz o risco); logout por GET. | todos os forms | 🟢 |
| 26 | O build avisa vulnerabilidade alta em `System.Security.Cryptography.Pkcs`; o pacote `System.Data.SqlClient` está no `.csproj` mas não é usado no código. | `APICeleiroCriativo.csproj` | 🟢 |
| 27 | Código morto com bugs no login: o bloco que preenche `Cliente?`/`Funcionario?` (objetos nulos) tem `TipoDocumento = nome` e `Ativo = GetBoolean("codigo_verificacao")`. | `UsuarioSqlRepository.Entrar` | 🟢 |
| 28 | Projeto aprovado continua com o título "Aguardando aprovação…" e sem data de início. | `ProjetoSqlRepository.Aprovar` | 🟢 |
