# APICeleiroCriativo

Backend/API do sistema de gestão de projetos da Celeiro Criativo. ASP.NET Core Web API (.NET 10) + Entity Framework Core + SQL Server.

## Como rodar

Pré-requisitos: .NET SDK 10, e um SQL Server acessível (por padrão usa **LocalDB**, que já vem com as ferramentas do SQL Server no Windows — nada extra a instalar).

```bash
dotnet run
```

Por padrão sobe em `http://localhost:5028` (perfil "http" em `Properties/launchSettings.json`). Na primeira execução, a API cria o banco `CeleiroCriativo` automaticamente (via `Database.EnsureCreated()`) e semeia dados iniciais — ver `Data/DbInitializer.cs`.

**Credenciais de teste** (criadas automaticamente):

| Perfil | E-mail | Senha |
|---|---|---|
| Cliente | `cliente@teste.com` | `123456` |
| Gestor (Funcionário, Cargo = Gestor) | `gestor@teste.com` | `123456` |
| Funcionário (Cargo = Designer) | `funcionario@teste.com` | `123456` |

A connection string está em `appsettings.json` (`ConnectionStrings:DefaultConnection`), apontando para `(localdb)\MSSQLLocalDB`. Ajuste para outro SQL Server se preferir.

## Estrutura

```
Controllers/   Um controller por recurso REST, sob /api/<recurso> (+ /api/auth/login)
Models/        POCOs do domínio (entidades EF Core)
Data/          DbContext (mapeamento, incl. herança Pessoa/Cliente/Funcionario) + seed
Dtos/          Formas de request/response — nunca expõem o hash de senha
Security/      Hash de senha (PBKDF2, sem dependência externa)
wwwroot/uploads/  Arquivos enviados via POST /api/documentos (não versionado)
```

## Front-end

O front-end React (`System/CeleiroCriativo`) espera a API em `http://localhost:5028/api` por padrão (ver `.env.example` de lá) e já está configurado com CORS liberado para `http://localhost:5173` (servidor de dev do Vite).

## Simplificações assumidas (escopo acadêmico)

- O token de login não é validado nas requisições seguintes (sem `[Authorize]`) — não há controle de sessão real.
- `Database.EnsureCreated()` em vez de Migrations: bom para desenvolvimento, mas não gera histórico de alterações de schema. Para produção, trocar por EF Core Migrations.
- Sem HTTPS configurado para o front-end consumir (usa HTTP simples em dev, evitando a necessidade de confiar em certificado de desenvolvimento).
