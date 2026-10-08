# Estrutura do projeto antes da reorganização — 08/10/2026

Retrato do projeto no início da conversa de 08/10/2026 (ver `20261008.md`).

- Branch: `master`
- Commit: `24d137e` — "Atualização banco de dados (v4)"
- Árvore de trabalho: limpa (nenhuma alteração pendente)
- Para ver qualquer arquivo como estava: `git show 24d137e:<caminho>`
- Para listar tudo: `git ls-tree -r --name-only 24d137e`

## Árvore de arquivos (versionados no git)

```
CeleiroCriativo/
├── .gitignore
├── APICeleiroCriativo.csproj        (net10.0, sem pacotes NuGet)
├── CeleiroCriativo-master.sln
├── Program.cs
├── appsettings.json                 (sem connection string)
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json
│
├── Controllers/
│   ├── CadastroFuncionarioController.cs
│   ├── ExibicaoProjeto.cs           (classe ExibicaoProjetoController)
│   ├── HomeController.cs
│   ├── PerfilController.cs
│   ├── ProjetoController.cs
│   └── TarefaController.cs
│
├── Models/
│   ├── Cargo.cs
│   ├── Categoria.cs
│   ├── Cliente.cs
│   ├── Comentario.cs
│   ├── Documento.cs
│   ├── Funcionario.cs               (herda de Pessoa)
│   ├── Pessoa.cs                    (abstract)
│   ├── Projeto.cs
│   ├── Status.cs
│   └── Tarefa.cs
│
├── Views/                           (sem Shared/, _Layout, _ViewStart ou _ViewImports)
│   ├── CadastroFuncionario/
│   │   └── CadastroFuncionario.cshtml
│   ├── Categorias/                  (não existe CategoriasController)
│   │   ├── Create.cshtml
│   │   ├── Index.cshtml
│   │   └── Update.cshtml
│   ├── ExibicaoProjeto/
│   │   ├── ExProjetoCliente.cshtml
│   │   ├── ExProjetoFuncionario.cshtml
│   │   └── ExProjetoGestor.cshtml
│   ├── ExibicaoTarefa/
│   │   ├── ExTarefaCliente.cshtml
│   │   ├── ExTarefaFuncionario.cshtml
│   │   └── ExTarefaGestor.cshtml
│   ├── Home/
│   │   ├── Entrar.cshtml
│   │   └── Index.cshtml
│   ├── Perfil/
│   │   ├── ClientePerfil.cshtml
│   │   ├── FuncionarioPerfil.cshtml
│   │   └── GestorPerfil.cshtml
│   ├── Projeto/
│   │   ├── ClienteProjeto.cshtml
│   │   ├── FuncionarioProjeto.cshtml
│   │   └── GestorProjeto.cshtml
│   └── Tarefa/
│       ├── ClienteTarefa.cshtml
│       ├── FuncionarioTarefa.cshtml
│       └── GestorTarefa.cshtml
│
├── bd/
│   ├── Celeiro_Criativo_DDL_v3.sql
│   ├── Celeiro_Criativo_DDL_v4.sql  (SQL Server)
│   ├── DER - Celeiro Criativo (ES3) - 2026-10-06 (meu).drawio
│   └── DER - Celeiro Criativo (ES3) - 2026-10-06 (meu).drawio.png
│
└── wwwroot/
    ├── css/
    │   ├── cadastro_funcionario.css
    │   ├── exibicao_projeto.css
    │   ├── exibicao_tarefa.css
    │   ├── perfil.css
    │   ├── projeto.css
    │   ├── style.css
    │   └── tarefa.css
    ├── imagens/
    │   ├── Entrar.png, Entrar_icone.png, abrir.png, adicionar_tarefa.png
    │   ├── banner_cad.png, criar_funcionario.png, criar_mais.png
    │   ├── documento.png, editar.png, voltar.png, logo.png, fundo_celeiro.png
    │   ├── instagram.png, whats.png
    │   ├── empresa_fic1.jpg … empresa_fic4.jpg
    │   ├── evento_cafe.jpg, evento_eco.jpg, evento_flores.jpg, evento_tec.jpg
    │   └── membro1_design.jpg, membro2_video.jpg, membro3_avaliacao.jpg, membro4_design.jpg
    └── js/
        ├── exibicao_tarefa.js
        ├── projeto.js
        ├── script.js
        └── tarefa.js
```

## Program.cs

- `AddControllersWithViews()`, `UseStaticFiles()`, `UseRouting()`, `UseAuthorization()` (sem autenticação configurada)
- Rota única: `{controller=Home}/{action=Index}/{id?}`
- Nenhum serviço ou repositório registrado; nenhum acesso a banco

## Controllers → actions → views

Nenhum controller declara `namespace`. Todos herdam de `Controller` e só retornam views (sem lógica de negócio).

| Controller | Action | View retornada |
|---|---|---|
| `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| | `Entrar` | `Views/Home/Entrar.cshtml` |
| | `Cadastro` | `Views/Home/Cadastro.cshtml` (**não existe**) |
| `CadastroFuncionarioController` | `CadastroFuncionario` | `Views/CadastroFuncionario/CadastroFuncionario.cshtml` |
| | `Salvar` [POST] (`string TelaAnterior`) | redireciona p/ `TelaAnterior` ou `Projeto/GestorProjeto` (não salva nada ainda) |
| `ExibicaoProjetoController` | `ExProjetoGestor` / `ExProjetoFuncionario` / `ExProjetoCliente` | `Views/ExibicaoProjeto/ExProjeto{Perfil}.cshtml` |
| `PerfilController` | `GestorPerfil` / `FuncionarioPerfil` / `ClientePerfil` | `Views/Perfil/{Perfil}Perfil.cshtml` |
| `ProjetoController` | `GestorProjeto` | `Views/Projeto/GestorProjeto.cshtml` |
| | `ExProjetoGestor` | `~/Views/ExibicaoProjeto/ExProjetoGestor.cshtml` (caminho explícito) |
| | `FuncionarioProjeto` — `[HttpGet("/Projeto/FuncionarioProjeto")]` | `~/Views/Projeto/FuncionarioProjeto.cshtml` |
| | `ClienteProjeto` | `~/Views/Projeto/ClienteProjeto.cshtml` |
| `TarefaController` | `GestorTarefa` | `Views/Tarefa/GestorTarefa.cshtml` |
| | `FuncionarioTarefa` / `ClienteTarefa` | `~/Views/Tarefa/{Perfil}Tarefa.cshtml` |
| | `ExTarefaGestor` / `ExTarefaFuncionario` / `ExTarefaCliente` (`string status, string tarefa` → `ViewBag.Status`, `ViewBag.Tarefa`) | `~/Views/ExibicaoTarefa/ExTarefa{Perfil}.cshtml` |

## Observações do estado inicial

- Padrão "uma action + uma view por perfil" (Gestor, Funcionário, Cliente) em Projeto, Tarefa, Perfil e Exibição.
- Telas de exibição em controllers/pastas separadas (`ExibicaoProjeto`, `ExibicaoTarefa`), algumas acessadas por outro controller via caminho `~/Views/...`.
- Cada `.cshtml` é uma página HTML completa (`<html>`, `<head>`, logo, links de CSS), já que não há `_Layout`.
- Links entre telas feitos com `href` fixo (ex.: `/Projeto/GestorProjeto`, `/Tarefa/ExTarefaGestor`) e um `@Url.Action("GestorProjeto", "Projeto")` em `ExProjetoGestor.cshtml`.
- `Views/Categorias/` usa `@model List<Categoria>` e links `/categorias/update/{id}`, `/categorias/delete/{id}`, `/Categorias/create`, mas não há controller correspondente.
- Banco: SQL Server, tabelas Pessoas, Cargos, Status, Categorias, Funcionarios, Clientes, Projetos, Tarefas, TarefaFuncionarios, Documentos, Comentarios (DDL v4).
