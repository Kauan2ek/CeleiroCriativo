<p align="center">
  <img src="wwwroot/imagens/logo.png" alt="Logo Celeiro Criativo" width="160">
</p>

<h1 align="center">🌾 Celeiro Criativo</h1>

<p align="center">
  Sistema de gestão de projetos para uma agência de marketing digital 🎨📈<br>
  <sub>Projeto Interdisciplinar · FATEC Rio Preto · 4º ADS · 2026</sub>
</p>

<p align="center">
  🚧 <b>Em desenvolvimento</b> — modelagem do banco e telas prontas, CRUD em andamento
</p>

---

## 💡 Sobre o projeto

A **Celeiro Criativo** é uma agência de marketing digital que gerenciava tudo por **e-mail** 📧: pedidos de clientes, distribuição de tarefas, entrega de arquivos... Com muitos projetos ao mesmo tempo, isso virava uma bagunça, e o cliente ficava sem saber em que pé estava o projeto dele. 😵‍💫

Pra resolver isso, criamos um **sistema web orientado a projetos** em que cada pessoa tem o seu espaço:

| Perfil | O que faz |
| --- | --- |
| 🙋 **Cliente** | Acompanha o andamento dos seus projetos, vê as tarefas e os arquivos entregues |
| 🧑‍💻 **Funcionário** | Vê os projetos e tarefas atribuídos a ele, envia arquivos e atualiza o progresso |
| 🧑‍💼 **Gestor** | Cadastra clientes e funcionários, cria projetos, distribui tarefas e aprova as entregas |

## 🔄 Como funciona

```
💭 Cliente envia uma ideia pelo site da agência
        ↓
✅ Gestor aprova e cria o projeto
        ↓
👷 Gestor divide em tarefas e atribui à equipe
        ↓
📤 Funcionário sobe os arquivos da tarefa
        ↓
🔍 Gestor revisa → aprova ✔️ ou devolve ↩️
        ↓
🎉 Projeto entregue ao cliente!
```

## 📌 Status do projeto

| | Etapa |
| --- | --- |
| ✅ | Levantamento de requisitos, escopo e modelagem do banco (DER + script SQL Server v4) |
| ✅ | Telas dos três perfis (home da agência, entrar, perfil, projetos, tarefas e cadastro de funcionário) |
| ✅ | Estrutura MVC: `Controllers`, `Models` e camada `Repositories` (ADO.NET) |
| 🔨 | CRUD: ligar os repositórios ao banco e fazer os controllers entregarem dados reais às telas |
| 📝 | Próximos passos: login por perfil, envio de arquivos nas tarefas e aprovação de entregas pelo gestor |

> Por enquanto as telas são navegáveis como protótipo: ainda **não há acesso ao banco** nem autenticação, e algumas rotas estão sendo religadas depois da reorganização das pastas.

## 🛠️ Tecnologias

- ⚙️ **C# / ASP.NET Core MVC** (.NET 10)
- 🗄️ **SQL Server**, acessado com ADO.NET (`Microsoft.Data.SqlClient`)
- 🎨 **HTML + CSS + JavaScript** nas Views Razor (`.cshtml`)
- 📐 **draw.io** para a modelagem do banco (DER)

## 📁 Estrutura

```
📦 celeirocriativo
 ┣ 📂 Controllers     → recebem as requisições e escolhem a view
 ┣ 📂 Models          → entidades do sistema (Pessoa, Projeto, Tarefa, Cliente...)
 ┣ 📂 Repositories    → acesso ao banco via SQL (um repositório por entidade + DataConnection)
 ┣ 📂 Views           → telas por área (Home, Perfil, Projeto, Tarefa, Funcionario),
 ┃                      cada uma com a versão de Cliente, Funcionário e Gestor
 ┣ 📂 wwwroot         → CSS, JS e imagens
 ┣ 📂 Database
 ┃ ┣ 📂 Modelagem     → DER do banco (draw.io + imagem)
 ┃ ┗ 📂 Scripts       → scripts SQL de criação do banco (versões v3 e v4)
 ┣ 📜 APICeleiroCriativo.csproj → projeto .NET
 ┗ 📜 Program.cs      → ponto de entrada da aplicação
```

## 🚀 Como rodar

1. **Pré-requisito:** [.NET 10 SDK](https://dotnet.microsoft.com/download).
2. **Clone o repositório** e entre na pasta do projeto.
3. **Rode o projeto:**

   ```bash
   dotnet run
   ```

4. Abra [http://localhost:5028](http://localhost:5028) no navegador. 🎉

### 🗄️ Banco de dados (necessário quando o CRUD estiver ligado)

Você vai precisar de um **SQL Server** disponível.

1. Execute o script mais recente em `Database/Scripts/` (`Celeiro_Criativo_DDL_v4.sql`).
2. Ajuste a conexão em `Repositories/DataConnection.cs` com o servidor, usuário e senha do seu SQL Server, usando `Database=CeleiroCriativo`.

## 🗺️ Modelo de dados

<p align="center">
  <img src="Database/Modelagem/DER%20-%20Celeiro%20Criativo%20(ES3)%20-%202026-10-06%20(meu).drawio.png" alt="DER do Celeiro Criativo" width="900">
</p>

O DER (versão de 06/10/2026) e os arquivos editáveis do draw.io estão em [`Database/Modelagem`](Database/Modelagem). Em resumo:

- 👤 **Pessoa** é a base de **Cliente** e **Funcionário**
- 🏷️ Cada **Funcionário** tem um **Cargo**
- 📁 Um **Cliente** abre vários **Projetos**, e cada **Projeto** tem um **Status**
- ✅ Um **Projeto** tem várias **Tarefas**
- 🤝 **Funcionários** são alocados às **Tarefas** (relação N:N, com o campo `responsável`)
- 🗂️ Cada **Tarefa** tem **Categoria**, **Documentos** e **Comentários** (o status da tarefa é um atributo dela)
- 💬 Só quem está alocado na tarefa pode **comentar** nela

## 👥 Equipe

Feito com 💚 por:

- Enzo Oliveira Batista
- Henrique de Aguiar Fernandes
- Kauan Talles Silva Dias
- Manuela Pereira Pinotti
- Thayna Isabella Magalhães Pereira

<p align="center">
  <sub>🎓 FATEC Rio Preto · Análise e Desenvolvimento de Sistemas · Engenharia de Software & IHC</sub>
</p>
