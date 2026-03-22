# 03 — OpenIddict: Seeds e Configuração de Clientes

> **Referências:**
> - OpenIddict — registrando aplicações: https://documentation.openiddict.com/configuration/application-permissions
> - OpenIddict — escopos e recursos: https://documentation.openiddict.com/configuration/token-formats
> - OAuth2 Client Types (RFC 6749 §2.1): https://www.rfc-editor.org/rfc/rfc6749#section-2.1

## O que é o OpenIddictHostedService?

É um `IHostedService` que roda **uma vez na inicialização da aplicação**. Seu papel é fazer o seed (carga inicial) de dados no banco que o OpenIddict precisa para funcionar: escopos e clientes OAuth2.

```csharp
internal sealed class OpenIddictHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await SeedScopesAsync(scope.ServiceProvider, cancellationToken);
        await SeedApplicationsAsync(scope.ServiceProvider, cancellationToken);
    }
}
```

Ele cria um `AsyncScope` para resolver serviços com lifetime `Scoped` (como os managers do OpenIddict) a partir de um `IHostedService`, que é `Singleton`.

---

## Seed de Escopos

```csharp
private static async Task SeedScopesAsync(
    IServiceProvider services,
    CancellationToken cancellationToken)
{
    var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();

    if (await scopeManager.FindByNameAsync("api", cancellationToken) is null)
        await scopeManager.CreateAsync(GetApiScopeDescriptor(), cancellationToken);
}

private static OpenIddictScopeDescriptor GetApiScopeDescriptor()
    => new()
    {
        Name = "api",
        Description = "CoffeeAgent API Access",
        Resources = { "coffee_agent_api" }  // ← nome do resource server
    };
```

**O que é um escopo no OAuth2?**

Um escopo define uma permissão que um cliente pode solicitar. O escopo `api` aqui representa acesso à `BrewCoffee.Api`. O campo `Resources` (`coffee_agent_api`) é o identificador do resource server — quando a API valida um token, ela checa se `coffee_agent_api` está na audience do token.

Os escopos padrão do OIDC (`openid`, `email`, `profile`, `offline_access`, `roles`) são registrados no `BuilderSetup` via `options.RegisterScopes(...)` e não precisam de seed no banco — o OpenIddict os reconhece internamente.

---

## Seed de Aplicações (Clientes OAuth2)

O OpenIddict armazena no banco **qual cliente pode usar qual fluxo e quais escopos**. Se um cliente não estiver registrado, o servidor rejeita o request.

### Cliente BFF (`brewcoffee-bff`)

```csharp
private OpenIddictApplicationDescriptor GetBffDescriptor()
{
    (string clientId, string clientSecret) = configuration.GetProviderAuth("BFF");
    return new OpenIddictApplicationDescriptor
    {
        ClientId = clientId,
        ClientSecret = clientSecret,
        ClientType = OpenIddictConstants.ClientTypes.Confidential,  // ← tem secret
        DisplayName = "BrewCoffee BFF",
        RedirectUris = { new Uri(configuration.GetOpenIddictClientConfig("BFF", "CallbackUri")) },
        PostLogoutRedirectUris = { new Uri(configuration.GetOpenIddictClientConfig("BFF", "LogoutUri")) },
        Permissions =
        {
            // Endpoints que este cliente pode usar
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.Endpoints.EndSession,

            // Fluxos permitidos
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

            // Tipo de resposta (Authorization Code Flow retorna "code")
            OpenIddictConstants.Permissions.ResponseTypes.Code,

            // Escopos que pode solicitar
            OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
            OpenIddictConstants.Permissions.Prefixes.Scope + "email",
            OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
            OpenIddictConstants.Permissions.Prefixes.Scope + "api",
            OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
            OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access"
        }
    };
}
```

**`ClientType = Confidential`** significa que o cliente tem um `client_secret` e consegue guardar esse secret com segurança. O BFF roda no servidor, então é confidential — ao contrário de um SPA ou app mobile que rodaria no browser/dispositivo do usuário.

**`RedirectUri`** é o callback configurado no appsettings:

```json
"OpenIddict": {
  "Clients": {
    "BFF": {
      "CallbackUri": "https://localhost:7024/auth/callback",
      "LogoutUri":   "https://localhost:7024/auth/logout-callback"
    }
  }
}
```

O servidor **só aceita** redirect para URIs pré-cadastradas. Isso previne ataques de redirect aberto — um atacante não consegue trocar o `code` redirecionando para um domínio malicioso.

---

### Cliente Worker Service (`worker-service`) — M2M

```csharp
private OpenIddictApplicationDescriptor GetWorkerDescriptor()
{
    (string clientId, string clientSecret) = configuration.GetProviderAuth("M2M");
    return new OpenIddictApplicationDescriptor
    {
        ClientId = clientId,
        ClientSecret = clientSecret,
        ClientType = OpenIddictConstants.ClientTypes.Confidential,
        DisplayName = "CoffeeAgent Worker Service",
        Permissions =
        {
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,  // ← só M2M
            OpenIddictConstants.Permissions.Prefixes.Scope + "api",
        }
    };
}
```

O Worker Service usa **Client Credentials Flow** — não há usuário humano. O serviço se autentica com `client_id + client_secret` e recebe um `access_token` para chamar a API em nome dele mesmo. Por isso não tem `RedirectUris` nem permissão para `openid`/`email`/`profile` — esses escopos são para usuários humanos.

---

## Tabelas no banco de dados

O OpenIddict cria as seguintes tabelas no PostgreSQL (schema padrão):

| Tabela | Conteúdo |
|---|---|
| `openiddict_applications` | Clientes registrados (BFF, Worker) |
| `openiddict_scopes` | Escopos customizados (api) |
| `openiddict_authorizations` | Autorizações concedidas a clientes |
| `openiddict_tokens` | Tokens emitidos (access, refresh) |

As tabelas do Identity ficam no schema `identity` (users, roles, claims...).

---

## Idempotência do seed

O seed checa antes de criar:

```csharp
if (await manager.FindByClientIdAsync("brewcoffee-bff", cancellationToken) is null)
    await manager.CreateAsync(GetBffDescriptor(), cancellationToken);
```

Se a aplicação reiniciar, não duplica os registros. **Mas não atualiza registros existentes** — se precisar mudar as permissões de um cliente, precisa deletar manualmente do banco ou escrever lógica de upsert.
