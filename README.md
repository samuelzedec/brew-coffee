# BrewCoffee

Uma plataforma de vendas de café construída com ASP.NET Core 10, combinando autenticação OAuth2/OIDC com OpenIddict e um agente de IA especializado em recomendações de café utilizando o modelo Claude da Anthropic.

## O que é o BrewCoffee?

O BrewCoffee é uma aplicação backend que expõe endpoints REST para autenticação de usuários via provedores externos (Google e Microsoft) e integra um agente de IA (CoffeeAdvisor) capaz de fornecer recomendações personalizadas de café. A arquitetura segue princípios de Vertical Slice Architecture com boas práticas de desenvolvimento .NET moderno.

## Stack

- **Runtime**: .NET 10 / C# 14
- **Framework**: ASP.NET Core 10 (Minimal APIs via ZedEndpoints)
- **Autenticação**: OpenIddict (OAuth2/OIDC) + ASP.NET Core Identity
- **Provedores OAuth**: Google, Microsoft
- **Banco de dados**: PostgreSQL (via Npgsql + Entity Framework Core 10)
- **IA**: Anthropic Claude (`claude-sonnet-4-20250514`) via Microsoft.Agents.AI
- **Validação**: FluentValidation 12
- **Logs**: Serilog
- **Documentação**: Scalar (OpenAPI)
- **Testes**: xUnit v3, Reqnroll (BDD/Gherkin), Testcontainers, FluentAssertions, NSubstitute, Bogus

## Estrutura do projeto

```
brew-coffee/
├── src/
│   ├── BrewCoffee.Authorization/       # Servidor OAuth2/OIDC (OpenIddict)
│   │   ├── Features/
│   │   │   └── Connect/               # Endpoints OAuth2 (authorize, token, userinfo, end-session)
│   │   │       ├── Authorize/
│   │   │       ├── Token/
│   │   │       ├── UserInfo/
│   │   │       ├── EndSession/
│   │   │       └── ExternalCallback/
│   │   └── Infrastructure/
│   │       ├── Persistence/           # DbContext, Identity, migrations, mappings
│   │       ├── HostedServices/        # Seed de clientes e escopos OpenIddict
│   │       ├── Extensions/            # Métodos de extensão
│   │       └── Setups/               # Configuração da aplicação
│   ├── BrewCoffee.Api/                # API principal
│   │   ├── Features/                  # Features por domínio (Catalog, Orders, etc.)
│   │   └── Infrastructure/
│   │       ├── Persistence/
│   │       └── Services/
│   └── BrewCoffee.Shared/             # Código compartilhado entre projetos
│       ├── Abstractions/              # Interfaces base
│       ├── Behaviors/                 # Pipeline behaviors (validação)
│       ├── Common/                    # Result, Error, ErrorType
│       └── Exceptions/               # Tratamento global de erros
├── tests/
│   ├── BrewCoffee.AcceptanceTests/    # Testes BDD com Reqnroll + Testcontainers
│   └── BrewCoffee.UnitTests/          # Testes unitários com xUnit
├── Dockerfile
├── Directory.Build.props
├── Directory.Packages.props
└── BrewCoffee.slnx
```

## Fluxo de Autenticação

O BrewCoffee implementa OAuth2/OIDC com Authorization Code Flow + PKCE para o frontend Angular e Client Credentials para serviços M2M.

```
Angular → /connect/authorize?provider=Google
        → Google autentica o usuário
        → Volta com code para Angular (/callback?code=xxx)
        → Angular troca code por tokens (POST /connect/token)
        → Angular usa access_token nas chamadas à API
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- PostgreSQL acessível (ex: [Supabase](https://supabase.com/) ou instância local)
- Chave de API da [Anthropic](https://www.anthropic.com/)
- Credenciais OAuth no [Google Cloud Console](https://console.cloud.google.com/) e [Azure Portal](https://portal.azure.com/)

## Configuração

Configure os secrets via `dotnet user-secrets` em desenvolvimento:

```bash
# Google OAuth
dotnet user-secrets set "Auth:Google:ClientId" "seu-client-id"
dotnet user-secrets set "Auth:Google:ClientSecret" "seu-client-secret"

# Microsoft OAuth
dotnet user-secrets set "Auth:Microsoft:ClientId" "seu-client-id"
dotnet user-secrets set "Auth:Microsoft:ClientSecret" "seu-client-secret"

# M2M Worker Service
dotnet user-secrets set "M2M:ClientId" "worker-service"
dotnet user-secrets set "M2M:ClientSecret" "seu-secret"

# Connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=..."
```

Em produção, utilize **Azure Key Vault** ou variáveis de ambiente.

## Executando localmente

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
dotnet ef database update --project src/BrewCoffee.Authorization

# Rodar a aplicação
dotnet run --project src/BrewCoffee.Authorization
```

A API estará disponível em `https://localhost:7295`. A documentação interativa (Scalar) pode ser acessada em `/scalar`.

O discovery document OIDC estará disponível em:
```
https://localhost:7295/.well-known/openid-configuration
```

## Executando via Docker

```bash
docker build -t brew-coffee .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Auth__Google__ClientId="..." \
  -e Auth__Google__ClientSecret="..." \
  brew-coffee
```

## Testes

```bash
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test tests/BrewCoffee.UnitTests

# Apenas testes de aceitação (requer Docker para Testcontainers)
dotnet test tests/BrewCoffee.AcceptanceTests
```

## Endpoints OAuth2

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/connect/authorize` | Inicia o fluxo de autenticação |
| POST | `/connect/token` | Troca o code pelos tokens |
| GET | `/connect/userinfo` | Retorna claims do usuário autenticado |
| GET | `/connect/end-session` | Logout |
| GET | `/.well-known/openid-configuration` | Discovery document OIDC |

Respostas de erro seguem o padrão [Problem Details (RFC 9457)](https://www.rfc-editor.org/rfc/rfc9457).

## Arquitetura

- **Vertical Slice Architecture**: cada feature é autônoma e independente
- **OpenIddict**: servidor OAuth2/OIDC completo com suporte a Authorization Code + PKCE e Client Credentials
- **ASP.NET Core Identity**: gerenciamento de usuários com suporte a login social e local
- **Repository Pattern**: repositório genérico com especialização por entidade
- **Result Pattern**: sem exceções para fluxo de negócio — erros retornam `Result<T>` tipado
- **Minimal APIs**: rotas organizadas por feature com ZedEndpoints