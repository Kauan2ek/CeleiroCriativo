<p align="center">
  <img src="wwwroot/imagens/logo.png" alt="Logo Celeiro Criativo" width="160">
</p>

<h1 align="center">🌾 Celeiro Criativo</h1>

<p align="center">
  Sistema de gestão de projetos para uma agência de marketing digital 🎨📈<br>
  <sub>Projeto Interdisciplinar · FATEC Rio Preto · 3º ADS · 2026</sub>
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
💭 Cliente envia uma ideia
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

## 🛠️ Tecnologias

- ⚙️ **C# / ASP.NET Core MVC** (.NET 10)
- 🗄️ **SQL Server**, acessado com ADO.NET (`Microsoft.Data.SqlClient`)
- 🎨 **HTML + CSS** nas Views Razor (`.cshtml`)
- 📐 **draw.io** para a modelagem do banco (DER)

## 📁 Estrutura

```
📦 celeirocriativo
 ┣ 📂 Controllers     → recebem as requisições e chamam as views
 ┣ 📂 Models          → entidades do sistema (Projeto, Tarefa, Cliente...)
 ┣ 📂 Repositories    → acesso ao banco via SQL (um repositório por entidade)
 ┣ 📂 Views           → telas de cada perfil (Cliente, Funcionário, Gestor)
 ┣ 📂 wwwroot         → CSS, JS e imagens
 ┣ 📂 Database
 ┃ ┣ 📂 Modelagem     → DER do banco
 ┃ ┗ 📂 Scripts       → scripts SQL de criação do banco
 ┗ 📜 Program.cs      → ponto de entrada da aplicação
```

## 🚀 Como rodar

1. **Pré-requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download) e um **SQL Server** disponível.
2. **Crie o banco** executando o script mais recente em `Database/Scripts/` (ex.: `Celeiro_Criativo_DDL_v4.sql`).
3. **Ajuste a conexão** em `Repositories/DataConnection.cs` com o servidor, usuário e senha do seu banco.
4. **Rode o projeto:**

   ```bash
   dotnet run
   ```

5. Abra no navegador o endereço que aparecer no terminal e pronto! 🎉

## 🗺️ Modelo de dados

O DER completo está em [`Database/Modelagem`](Database/Modelagem). Em resumo:

- 👤 **Pessoa** é a base de **Cliente** e **Funcionário**
- 📁 Um **Cliente** tem vários **Projetos**
- ✅ Um **Projeto** tem várias **Tarefas**, distribuídas entre os **Funcionários**
- 🏷️ Cada **Tarefa** tem **Status**, **Categoria**, **Comentários** e **Documentos**

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
