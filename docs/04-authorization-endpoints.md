# 04 — Authorization Server: Endpoints

> **Referências:**
> - OpenIddict — Authorization endpoint passthrough: https://documentation.openiddict.com/configuration/authorization-code-flow#using-the-passthrough-mode
> - OIDC UserInfo endpoint (spec): https://openid.net/specs/openid-connect-core-1_0.html#UserInfo
> - OpenIddict — SetDestinations (claim destinations): https://documentation.openiddict.com/configuration/claim-destinations
> - ASP.NET Core Razor Pages: https://learn.microsoft.com/en-us/aspnet/core/razor-pages/

Todos os endpoints OAuth2/OIDC ficam sob o prefixo `/connect`. As operações de usuário (login, registro, troca de senha) são **Razor Pages** — páginas com UI real que o browser acessa diretamente.

---

## GET /connect/authorize

**Arquivo:** `Features/Connect/Authorize/AuthorizeEndpoint.cs`

Este é o endpoint central do Authorization Code Flow. O OpenIddict o intercepta quando o BFF redireciona o browser para o AS, e com o **Passthrough** habilitado, passa o controle para o nosso handler.

### Lógica em três caminhos

```
1. Tem cookie de sessão local (ApplicationScheme)?  → emite o code direto
2. Tem cookie de login externo (ExternalScheme)?    → cria/localiza usuário → emite o code
3. Nenhum dos dois?                                 → redireciona para login
```

### Código completo

```csharp
private static async Task<IResult> HandleAsync(
    HttpContext context,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    CancellationToken cancellationToken)
{
    var request = context.GetOpenIddictServerRequest();
    if (request is null) return Results.BadRequest("OpenIddict request not found.");
    var provider = request.GetParameter("provider")?.ToString();

    // Caminho 1: usuário já autenticado localmente (Razor Page de login)
    var localResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
    if (localResult.Succeeded)
    {
        var localUser = await userManager.FindByEmailAsync(
            localResult.Principal!.FindFirstValue(ClaimTypes.Email)!);

        if (localUser is null) return Results.Forbid();

        var localPrincipal = await signInManager.CreateUserPrincipalAsync(localUser);
        localPrincipal.ConfigurePrincipal();

        return Results.SignIn(
            principal: localPrincipal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }

    // Caminho 2: usuário autenticou com Google/Microsoft (cookie temporário ExternalScheme)
    var externalResult = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
    if (externalResult.Succeeded)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) return Results.Forbid();

        var user = await FindOrCreateUserAsync(info, userManager);
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        principal.ConfigurePrincipal(request.GetScopes());

        return Results.SignIn(
            principal: principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }

    // Caminho 3: ninguém autenticado → redireciona para login
    return RedirectToLogin(provider, signInManager, context);
}
```

### Caminho 3: RedirectToLogin

```csharp
private static IResult RedirectToLogin(
    string? provider,
    SignInManager<ApplicationUser> signInManager,
    HttpContext context)
{
    // Captura a URL atual para voltar aqui depois do login
    var returnUrl = Uri.EscapeDataString(context.Request.GetEncodedPathAndQuery());

    // Login local → manda para a Razor Page /login
    if (string.IsNullOrWhiteSpace(provider) || provider == "local")
        return Results.Redirect($"/login?returnUrl={returnUrl}");

    // Login social → manda para o provedor externo (Google/Microsoft)
    var callbackUrl = $"/connect/external-callback?returnUrl={returnUrl}";
    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
    return Results.Challenge(properties, [provider]);
}
```

**O parâmetro `provider` no request OAuth2**

O BFF pode passar `provider=Google` como parâmetro extra no request de autorização. O AS usa isso para saber para qual provedor redirecionar. Se não tiver `provider` (ou for `local`), manda para a Razor Page de login com email/senha.

### FindOrCreateUser: estratégia de upsert

```csharp
private static async Task<ApplicationUser> FindOrCreateUserAsync(
    ExternalLoginInfo info,
    UserManager<ApplicationUser> userManager)
{
    // 1. Busca pelo par (provedor, chave do provedor) — ex: ("Google", "1234567890")
    var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
    if (user is not null) return user;

    // 2. Busca pelo email (pode ter criado conta com senha antes)
    var email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
    user = await userManager.FindByEmailAsync(email);

    if (user is not null)
    {
        // Vincula o login externo à conta existente
        await userManager.AddLoginAsync(user, info);
        return user;
    }

    // 3. Cria novo usuário (username = parte antes do @ do email, já confirmado)
    user = new ApplicationUser
    {
        UserName = email.Split('@')[0],
        Email = email,
        EmailConfirmed = true
    };
    await userManager.CreateAsync(user);
    await userManager.AddLoginAsync(user, info);
    return user;
}
```

---

## GET /connect/external-login

**Arquivo:** `Features/Connect/ExternalLogin/ExternalLoginEndpoint.cs`

Endpoint chamado pelas **Razor Pages** quando o usuário clica em "Entrar com Google" na página de login do AS.

```csharp
private static IResult Handle(
    string provider,
    string returnUrl,
    SignInManager<ApplicationUser> signInManager)
{
    var callbackUrl = $"/connect/external-callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
    return Results.Challenge(properties, [provider]);
}
```

**Por que esse endpoint existe separado do `/connect/authorize`?**

A Razor Page de login precisa de um link direto para iniciar o login social. Ela monta a URL: `/connect/external-login?provider=Google&returnUrl=/connect/authorize?...`. Isso mantém a separação clara: a Razor Page não precisa conhecer os detalhes do `/connect/authorize`.

---

## GET /connect/external-callback

**Arquivo:** `Features/Connect/ExternalCallback/ExternalCallbackEndpoint.cs`

Endpoint intermediário simples. O Google/Microsoft redireciona para ele após a autenticação, e ele redireciona de volta para onde o fluxo deve continuar (normalmente `/connect/authorize`).

```csharp
private static IResult HandleAsync(HttpContext context)
    => Results.Redirect(context.Request.Query["returnUrl"].FirstOrDefault() ?? "/connect/authorize");
```

**Por que esse endpoint existe?**

O provedor externo precisa de uma URI de callback registrada e fixa. Usar `/connect/authorize` diretamente como callback causaria problemas com o state e a query string do OAuth2. Ter um endpoint dedicado separa as responsabilidades:

```
Google → /connect/external-callback?returnUrl=/connect/authorize?client_id=...&state=...
       → /connect/authorize?client_id=...&state=...  (agora com cookie ExternalScheme setado)
```

---

## POST /connect/token

**Arquivo:** `Features/Connect/Token/TokenEndpoint.cs`

Chamado pelo BFF para trocar o `code` (Authorization Code Flow) por tokens reais, ou para renovar um `access_token` usando um `refresh_token`.

```csharp
private static async Task<IResult> HandleAsync(...)
{
    var request = context.GetOpenIddictServerRequest();

    // Só aceita Authorization Code e Refresh Token
    return !request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType()
        ? Results.BadRequest("Invalid grant type.")
        : await IssueTokenAsync(context, request, signInManager, userManager);
}

private static async Task<IResult> IssueTokenAsync(...)
{
    // Autentica pelo scheme do OpenIddict — ele valida o code ou refresh_token
    var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    if (!result.Succeeded) return Results.Forbid();

    var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);
    var user = await userManager.FindByIdAsync(userId);

    if (user is null || !await signInManager.CanSignInAsync(user))
        return Results.Forbid();

    // Reconstrói o principal com claims atualizadas do banco
    var principal = await signInManager.CreateUserPrincipalAsync(user);
    principal.ConfigurePrincipal(request.GetScopes());  // ← passa escopos do request

    return Results.SignIn(
        principal: principal,
        authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
    );
}
```

**Por que `CreateUserPrincipalAsync` de novo?**

O `code` contém o subject (user ID), mas não as claims completas. Ao chamar `CreateUserPrincipalAsync`, buscamos os dados atualizados do usuário no banco — garantindo que o token sempre reflita o estado atual (ex: novo email, nova role).

**Por que `request.GetScopes()` é passado?**

O BFF solicita escopos específicos no request de autorização (`scope=openid email profile api offline_access`). Ao passar esses escopos para `ConfigurePrincipal`, o token emitido contém apenas o que foi pedido — não todos os escopos do servidor.

---

## GET /connect/userinfo

**Arquivo:** `Features/Connect/UserInfo/UserInfoEndpoint.cs`

Endpoint OIDC padrão. Recebe um `access_token` válido e retorna as claims do usuário.

```csharp
private static async Task<IResult> HandleAsync(
    HttpContext context,
    UserManager<ApplicationUser> userManager,
    CancellationToken cancellationToken)
{
    var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    if (!result.Succeeded) return Results.Forbid();

    var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);
    var user = await userManager.FindByIdAsync(userId);
    if (user is null) return Results.Forbid();

    var claims = new Dictionary<string, object>
    {
        [OpenIddictConstants.Claims.Subject]       = user.Id.ToString(),
        [OpenIddictConstants.Claims.Email]         = user.Email!,
        [OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed,
        [OpenIddictConstants.Claims.Name]          = user.UserName!
    };

    return Results.Ok(claims);
}
```

**Quem chama esse endpoint?**

O OpenIdConnect middleware do BFF chama automaticamente durante o login (quando `GetClaimsFromUserInfoEndpoint = true`) para popular as claims no cookie de sessão.

---

## GET /connect/end-session

**Arquivo:** `Features/Connect/EndSession/EndSessionEndpoint.cs`

Encerra a sessão do usuário no servidor de identidade. Chamado pelo BFF durante o logout.

```csharp
private static async Task<IResult> HandleAsync(
    HttpContext context,
    SignInManager<ApplicationUser> signInManager,
    CancellationToken cancellationToken)
{
    await signInManager.SignOutAsync();

    return Results.SignOut(authenticationSchemes:
        [
            IdentityConstants.ApplicationScheme,         // limpa o cookie local do AS
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme  // invalida o token
        ]
    );
}
```

O `signInManager.SignOutAsync()` limpa o cookie de sessão do Identity no AS. O `Results.SignOut` com o scheme do OpenIddict invalida os tokens ativos no banco.

---

## Razor Pages: Login, Registro e Troca de Senha

**Localização:** `Pages/Login/`, `Pages/Register/`, `Pages/ChangePassword/`

Estas são páginas com UI real renderizadas pelo AS. O browser as acessa diretamente durante o fluxo OAuth2.

**Login (`/login`)**

Recebe `?returnUrl=/connect/authorize?...`. Após autenticar o usuário com `SignInAsync(ApplicationScheme)`, redireciona para o `returnUrl` — que leva de volta ao `/connect/authorize` onde o cookie `ApplicationScheme` será detectado e o code será emitido.

Também exibe botões "Entrar com Google" e "Entrar com Microsoft" que apontam para `/connect/external-login?provider=Google&returnUrl=...`.

**Registro (`/register`)**

Cria um novo usuário com email e senha. Após o registro, redireciona para `/login`.

**Troca de senha (`/change-password`)**

Requer que o usuário esteja autenticado localmente (cookie `ApplicationScheme`). Valida a senha atual e define a nova.
