# 02 — Authorization Server: Setup e Configuração

> **Referências:**
> - OpenIddict — configuração do servidor: https://documentation.openiddict.com/configuration/
> - OpenIddict + EF Core: https://documentation.openiddict.com/integrations/entity-framework-core
> - ASP.NET Core Identity: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity
> - PKCE (RFC 7636): https://www.rfc-editor.org/rfc/rfc7636

## Estrutura do projeto

```
BrewCoffee.Authorization/
├── Features/
│   ├── Account/
│   │   ├── AccountGroupEndpoint.cs
│   │   ├── ChangePassword/    → PATCH /account/change-password
│   │   ├── HasPassword/       → GET   /account/password/exists
│   │   └── Profile/           → PATCH /account/profile
│   └── Connect/
│       ├── Authorize/         → GET  /connect/authorize
│       ├── EndSession/        → GET  /connect/end-session
│       ├── ExternalCallback/  → GET  /connect/external-callback
│       ├── ExternalLogin/     → GET  /connect/external-login
│       ├── Token/             → POST /connect/token
│       └── UserInfo/          → GET  /connect/userinfo
├── Pages/
│   ├── Login/                 → /login        (Razor Page)
│   └── Register/              → /register     (Razor Page)
└── Infrastructure/
    ├── Extensions/
    │   ├── ClaimsPrincipalExtensions.cs
    │   └── ConfigurationExtensions.cs
    ├── HostedServices/
    │   └── OpenIddictHostedService.cs   ← seeds do banco
    ├── Persistence/
    │   ├── BrewCoffeeAuthDbContext.cs
    │   ├── Identity/
    │   │   ├── ApplicationUser.cs
    │   │   └── ApplicationRole.cs
    │   └── Mappings/Openiddict/
    ├── Services/
    │   └── CurrentUserService.cs
    └── Setups/
        ├── BuilderSetup.cs    ← registra todos os serviços
        └── PipelineSetup.cs   ← monta o pipeline HTTP
```

---

## BuilderSetup: como os serviços são registrados

O `BuilderSetup.cs` usa a nova sintaxe `extension(WebApplicationBuilder builder)` do C# 14 para organizar a configuração em métodos privados. O método público `Configure()` chama tudo na ordem certa:

```csharp
public void Configure()
{
    builder.ConfigureDbContext();
    builder.ConfigureIdentity();
    builder.ConfigureExceptionHandling();
    builder.ConfigureOpenIddict();
    builder.ConfigureAuthentication();
    builder.ConfigureMediatorWithValidation();
    builder.ConfigureLogger();
    builder.ConfigureServices();
    builder.ConfigureRazorPages();   // ← UI de login/registro
}
```

### DbContext com OpenIddict

```csharp
builder.Services.AddDbContext<BrewCoffeeAuthDbContext>(options => options
    .UseNpgsql(connectionString, b => b
        .MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
    .UseOpenIddict<Guid>()  // ← integra as entidades do OpenIddict ao contexto
    .EnableServiceProviderCaching()
    .EnableSensitiveDataLogging(isDevelopment));
```

O `.UseOpenIddict<Guid>()` faz o EF Core mapear automaticamente as tabelas do OpenIddict (`openiddict_applications`, `openiddict_tokens`, etc.) usando `Guid` como tipo de chave primária.

### ASP.NET Core Identity

```csharp
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3)
    .AddEntityFrameworkStores<BrewCoffeeAuthDbContext>()
    .AddDefaultTokenProviders();
```

`IdentitySchemaVersions.Version3` usa a versão mais recente do schema do Identity, com melhorias de performance e segurança (incluindo suporte a Passkeys via `IdentityUserPasskey<Guid>`).

### OpenIddict Server

Esta é a parte central. O OpenIddict tem três partes: `Core`, `Server` e `Validation`.

```csharp
builder.Services.AddOpenIddict()
    // Core: integração com EF Core
    .AddCore(options => options
        .UseEntityFrameworkCore()
        .UseDbContext<BrewCoffeeAuthDbContext>()
        .ReplaceDefaultEntities<Guid>()
    )
    // Server: o servidor OAuth2/OIDC em si
    .AddServer(options =>
    {
        // Quais URIs o OpenIddict "intercepta"
        options
            .SetAuthorizationEndpointUris("/connect/authorize")
            .SetTokenEndpointUris("/connect/token")
            .SetEndSessionEndpointUris("/connect/end-session")
            .SetUserInfoEndpointUris("/connect/userinfo");

        // Fluxos habilitados
        options
            .AllowClientCredentialsFlow()  // M2M: cliente autentica com client_id + secret
            .AllowAuthorizationCodeFlow()  // Login humano via Authorization Code + PKCE
            .AllowRefreshTokenFlow();      // Renovar access_token sem logar de novo

        // Escopos que o servidor reconhece
        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Scopes.OfflineAccess
        );

        // PKCE obrigatório no Authorization Code Flow
        options.RequireProofKeyForCodeExchange();

        // Certificados para assinar e criptografar tokens (modo desenvolvimento)
        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // Passthrough: delega o handler para nossos endpoints customizados
        options
            .UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough();
    })
    // Validation: valida tokens neste mesmo servidor
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });
```

**O que é Passthrough?**

Sem passthrough, o OpenIddict processaria os endpoints automaticamente com lógica genérica. Com passthrough, ele sinaliza que o request é um request OAuth2 válido, mas **passa o controle para o nosso handler** (ex: `AuthorizeEndpoint.cs`). Isso nos permite customizar o que acontece após a validação — como buscar o usuário no banco, configurar claims específicos, etc.

**Por que não há Password Grant (`AllowPasswordFlow`)?**

O Password Grant está sendo depreciado no OAuth 2.1 porque exige que o cliente (BFF) receba as credenciais do usuário — quebrando o princípio de que só o AS deve ver senhas. Com Razor Pages no próprio AS, o usuário autentica diretamente no AS via Authorization Code Flow, sem precisar do Password Grant.

### Provedores externos (Google / Microsoft)

```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var client = builder.Configuration.GetProviderAuth("Google");
        options.ClientId = client.Id;
        options.ClientSecret = client.Secret;
    })
    .AddMicrosoftAccount(options =>
    {
        var client = builder.Configuration.GetProviderAuth("Microsoft");
        options.ClientId = client.Id;
        options.ClientSecret = client.Secret;
    });
```

`GetProviderAuth()` é um extension method em `ConfigurationExtensions.cs` que lê as credenciais do `appsettings.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "Microsoft": {
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

### Razor Pages

```csharp
builder.Services.AddRazorPages();
// ...
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/login"));
```

As Razor Pages cobrem as operações de autenticação que o browser acessa diretamente durante o fluxo OAuth2 — Login e Registro. A raiz `/` redireciona para `/login`.

A troca de senha e demais operações de conta foram movidas para endpoints de API sob `/account/` (ver `Features/Account/`), acessíveis via BFF com Bearer token.

---

## PipelineSetup: ordem dos middlewares

```csharp
app.MapEndpointGroups();   // endpoints mínimos (connect/*)
app.UseExceptionHandler();
app.UseAuthentication();   // ← antes de UseAuthorization
app.UseAuthorization();
app.MapRazorPages();       // páginas de login/registro
app.MapGet("/", () => Results.Redirect("/login"));
await app.ApplyMigrationsAsync(); // aplica migrations automaticamente
```

A ordem importa: `UseAuthentication` deve vir antes de `UseAuthorization`. O `ApplyMigrationsAsync` garante que o banco esteja atualizado a cada inicialização — útil em desenvolvimento e deploy.

---

## ClaimsPrincipalExtensions: ConfigurePrincipal()

Este método é chamado no `AuthorizeEndpoint` e no `TokenEndpoint`. Ele prepara o `ClaimsPrincipal` para o OpenIddict emitir os tokens corretamente.

```csharp
public void ConfigurePrincipal(IEnumerable<string>? scopes = null)
{
    // Garante que o claim "sub" existe (obrigatório no OIDC)
    var subject =
        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? principal.FindFirst("sub")?.Value;

    if (subject is not null)
        principal.SetClaim(OpenIddictConstants.Claims.Subject, subject);

    // Define quais escopos estão no token (usa os do request, ou nenhum se null)
    principal.SetScopes(scopes);

    // Define para onde cada claim vai:
    // Name e Email → tanto no access_token quanto no identity_token
    // Todo o resto → só no access_token
    principal.SetDestinations(claim => claim.Type switch
    {
        ClaimTypes.Name or ClaimTypes.Email =>
        [
            OpenIddictConstants.Destinations.AccessToken,
            OpenIddictConstants.Destinations.IdentityToken
        ],
        _ => [OpenIddictConstants.Destinations.AccessToken]
    });
}
```

**Por que `SetDestinations` é necessário?**

O OpenIddict rejeita qualquer claim que não tenha um destino explícito. Isso é uma proteção contra vazamento de dados sensíveis — você precisa declarar explicitamente "essa claim vai para o access_token" ou "essa vai para o identity_token".

**Por que `scopes` é passado como parâmetro?**

Em vez de hardcodar os escopos, usamos os escopos que o cliente solicitou no request (`request.GetScopes()`). Isso respeita o princípio do menor privilégio — o token contém apenas os escopos que foram pedidos e autorizados, não todos os escopos possíveis.
