# 05 — BFF (Backend-For-Frontend)

> **Referências:**
> - Padrão BFF (Duende): https://docs.duendesoftware.com/identityserver/v7/bff/overview/
> - YARP — Transforms: https://microsoft.github.io/reverse-proxy/articles/transforms.html
> - ASP.NET Core OpenIdConnect middleware: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/
> - Cookie security (SameSite, HttpOnly): https://learn.microsoft.com/en-us/aspnet/core/security/samesite

## Por que o BFF existe?

O Angular (SPA) tem um problema fundamental: ele roda no browser do usuário. Guardar `access_token` em `localStorage` ou `sessionStorage` é inseguro — qualquer script JavaScript injetado na página (XSS) consegue ler e exfiltrar esses tokens.

O BFF resolve isso:

1. **O Angular nunca vê o token** — só manda e recebe cookies
2. **O BFF armazena o token** em cookie `HttpOnly` (inacessível para JavaScript)
3. **O BFF injeta o Bearer token** automaticamente em cada request para a API

---

## Estrutura do projeto

```
BrewCoffee.BFF/
├── Features/
│   └── Auth/
│       ├── AuthGroupEndpoint.cs   ← agrupa em /auth
│       ├── LoginEndpoint.cs       → GET  /auth/login
│       ├── LogoutEndpoint.cs      → POST /auth/logout
│       └── MeEndpoint.cs          → GET  /auth/me
├── Infrastructure/
│   ├── Setups/
│   │   ├── BuildSetups.cs         ← registra todos os serviços
│   │   └── PipelineSetup.cs       ← monta o pipeline HTTP
│   └── Transformers/
│       └── TokenTransformer.cs    ← injeta Bearer token nos requests proxiados
└── Shared/
    └── Constants/
        └── HttpClientConstants.cs
```

---

## BuildSetups: configuração do BFF

### Autenticação: Cookie + OpenIdConnect

```csharp
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "bff.session";
        options.Cookie.HttpOnly = true;           // JavaScript não lê
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;   // só HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict;  // não vai em requests cross-site
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;          // renova o timer a cada request
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];   // https://localhost:7295
        options.ClientId = builder.Configuration["Auth:ClientId"];
        options.ClientSecret = builder.Configuration["Auth:ClientSecret"];

        options.ResponseType = OpenIdConnectResponseType.Code;  // Authorization Code Flow
        options.UsePkce = true;                    // PKCE obrigatório
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("api");
        options.Scope.Add("offline_access");

        options.SaveTokens = true;                 // ← salva access_token + refresh_token na sessão
        options.GetClaimsFromUserInfoEndpoint = true;  // chama /connect/userinfo automaticamente
        options.CallbackPath = "/auth/callback";
        options.SignedOutCallbackPath = "/auth/logout-callback";
    });
```

**`DefaultScheme = Cookie`** significa que por padrão todos os requests são autenticados pelo cookie de sessão.

**`DefaultChallengeScheme = OpenIdConnect`** significa que quando um endpoint requer autenticação e o usuário não está logado, o middleware automaticamente redireciona para o AS via OpenIdConnect.

**`SaveTokens = true`** é crítico: faz o middleware do OpenIdConnect guardar o `access_token` e `refresh_token` dentro dos dados do cookie de sessão. O `TokenTransformer` os recupera depois para injetar nos requests para a API.

**`GetClaimsFromUserInfoEndpoint = true`** faz o middleware chamar automaticamente `GET /connect/userinfo` após o login, populando as claims do usuário no cookie de sessão.

### YARP Reverse Proxy

```csharp
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<TokenTransformer>();
```

O YARP (Yet Another Reverse Proxy) lê as rotas do `appsettings.json` e age como proxy reverso. O `TokenTransformer` é registrado como transform — ele roda em cada request proxiado.

**Rotas configuradas no appsettings:**

```json
"ReverseProxy": {
  "Routes": {
    "brew-coffee-api": {
      "ClusterId": "brew-coffee-api",
      "Match": { "Path": "/api/{**catch-all}" }
    }
  },
  "Clusters": {
    "brew-coffee-api": {
      "Destinations": {
        "destination1": { "Address": "https://localhost:7272" }
      }
    }
  }
}
```

O BFF hoje proxia apenas requests para a `BrewCoffee.Api` — qualquer coisa que começa com `/api/` é encaminhada para `https://localhost:7272`.

### CORS para o Angular

```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()  // ← necessário para o browser enviar cookies
    )
);
```

`AllowCredentials()` é obrigatório para que o browser envie o cookie `bff.session` nos requests cross-origin (Angular em `:4200` → BFF em `:7024`). Sem isso o cookie não vai junto.

---

## TokenTransformer: injetando o Bearer token

```csharp
internal sealed class TokenTransformer : ITransformProvider
{
    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            // Recupera o access_token salvo na sessão pelo SaveTokens = true
            var accessToken = await transformContext.HttpContext
                .GetTokenAsync("access_token");

            if (accessToken is not null)
            {
                transformContext.ProxyRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        });
    }
}
```

O `TokenTransformer` injeta o Bearer token em **todos** os requests proxiados pelo YARP. Como a única rota proxiada é `/api/`, que requer autenticação, isso é correto — não há rotas públicas sendo proxiadas atualmente.

**Como funciona na prática:**

```
Angular GET /api/products (com cookie bff.session)
  → BFF recebe o request
  → TokenTransformer: recupera access_token da sessão do cookie
  → Adiciona "Authorization: Bearer eyJ..." no request proxiado
  → Encaminha para https://localhost:7272/api/products
  → API valida o Bearer token via OpenIddict
  → Retorna a resposta
```

**Configuração de desenvolvimento (certificados autoassinados)**

```csharp
// YARP — aceita certificados autoassinados do AS e da API em dev
.ConfigureHttpClient((_, handler) => handler
    .SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true);

// OpenIdConnect — aceita certificado autoassinado do AS em dev
options.BackchannelHttpHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
```

Os blocos `#if DEBUG` garantem que essas validações relaxadas só existam em desenvolvimento — em produção, certificados reais são validados normalmente.

---

## Endpoints do BFF

### GET /auth/login

Inicia o fluxo de login. O Angular chama esse endpoint e o BFF emite um Challenge para o OpenIdConnect middleware, que redireciona o browser para o AS.

```csharp
private static IResult Handle(string? returnUrl)
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "http://localhost:4200"
    };
    return Results.Challenge(properties);
}
```

`Results.Challenge` com o `DefaultChallengeScheme = OpenIdConnect` faz o middleware construir a URL de autorização completa (`https://localhost:7295/connect/authorize?client_id=...&code_challenge=...&state=...`) e redirecionar o browser para lá.

**Sem parâmetro `provider` aqui.** O BFF não sabe qual provedor o usuário vai escolher — isso é decidido na Razor Page de login do AS. Se o Angular quiser pré-selecionar Google, pode passar o provider como parâmetro customizado do OpenIdConnect (configuração adicional necessária).

---

### POST /auth/logout

```csharp
private static async Task HandleAsync(HttpContext context, string? returnUrl)
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = returnUrl ?? "http://localhost:4200" });
}
```

Faz dois sign-outs:

1. **Cookie local** — invalida o `bff.session` imediatamente
2. **OpenIdConnect** — redireciona para o AS (`/connect/end-session`), que limpa a sessão no servidor de identidade

O resultado é que o usuário fica deslogado tanto no BFF quanto no AS.

---

### GET /auth/me

Retorna os dados do usuário logado a partir do cookie de sessão. Não chama banco nem token.

```csharp
private static IResult Handle(HttpContext context)
{
    var user = context.User;

    var response = new UserResponse(
        Id:    user.FindFirstValue("sub")!,
        Email: user.FindFirstValue("email")!,
        Name:  user.FindFirstValue("name")!
    );

    return Results.Ok(response);
}
```

O Angular chama esse endpoint para saber se o usuário está logado e obter os dados básicos para exibir na UI. As claims `sub`, `email` e `name` foram populadas pelo `GetClaimsFromUserInfoEndpoint = true` durante o login.
