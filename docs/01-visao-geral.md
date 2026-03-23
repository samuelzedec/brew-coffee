# 01 — Visão Geral da Arquitetura

## Por que temos três serviços separados?

```
┌─────────────┐       ┌──────────────────┐       ┌───────────────────────┐
│   Angular   │──────▶│  BrewCoffee.BFF  │──────▶│ BrewCoffee.Api        │
│ (porta 4200)│◀──────│  (porta 7024)    │       │ (porta 7272)          │
└─────────────┘ cookie└──────────────────┘       └───────────────────────┘
                              │
                              │ OAuth2/OIDC (Authorization Code Flow + PKCE)
                              ▼
                   ┌──────────────────────────┐
                   │ BrewCoffee.Authorization │
                   │ (porta 7295)             │
                   │                          │
                   │  /login    (Razor Page)   │
                   │  /register (Razor Page)  │
                   │  /connect/authorize      │
                   │  /connect/token          │
                   │  /account/change-password│
                   │  /account/profile        │
                   │  /account/password/exists│
                   └──────────────────────────┘
```

---

### BrewCoffee.Authorization (AS — Authorization Server)

É o servidor de identidade da aplicação. Responsável por:

- **Emitir tokens** (access_token, refresh_token, id_token)
- **Gerenciar usuários** via ASP.NET Core Identity
- **Hospedar a UI de autenticação** com Razor Pages (Login, Registro)
- **Expor endpoints de conta** como API REST (troca de senha, atualização de perfil, verificação de senha)
- **Federar login externo** (Google, Microsoft) — o usuário autentica com o provedor e o AS cria ou encontra o usuário local
- **Expor o UserInfo endpoint** para leitura de claims após autenticação

Usa **OpenIddict** como engine de OAuth2/OIDC. Pense nele como um "mini Auth0" próprio — com a diferença de que toda a UI de login fica dentro do próprio AS (Razor Pages), não no Angular.

**Por que a UI fica no AS e não no Angular?**

No Authorization Code Flow, o browser é redirecionado para o AS para autenticar. O AS precisa ter uma página de login para receber esse usuário. Isso é o padrão correto do OAuth2 — o usuário entrega suas credenciais diretamente ao servidor de identidade, não ao cliente (Angular).

---

### BrewCoffee.BFF (Backend-For-Frontend)

O BFF existe para proteger o Angular. Navegadores têm restrições sérias com tokens:

- **Não armazene access_token no localStorage** — qualquer script XSS na página consegue lê-lo
- **Não exponha client_secret no frontend** — o Angular não pode fazer o code exchange com segurança

O BFF resolve isso sendo um **intermediário confiável**:

1. Armazena o access_token em **cookie HttpOnly** (JavaScript não consegue ler)
2. Recebe requests do Angular sem token visível
3. Injeta o Bearer token automaticamente antes de repassar para a API ou para o AS (via YARP + `TokenTransformer`)

```
Angular → BFF (cookie de sessão) → API     (Bearer token no header)
                                 → AS /account/* (Bearer token no header)
                ↕ OAuth2 Authorization Code Flow + PKCE
          Authorization Server
```

---

### BrewCoffee.Api

A API real de negócio. Recebe requests com `Authorization: Bearer {token}` e valida via OpenIddict. Nunca conversa diretamente com o Angular.

---

## Responsabilidades resumidas

| Camada | Quem autentica | O que armazena | Protege |
|---|---|---|---|
| **Authorization Server** | Usuários (Razor Pages: senha, Google, Microsoft) e clientes M2M | Usuários, tokens, aplicações OAuth2 | Base de identidade e operações de conta |
| **BFF** | Cookie de sessão HttpOnly do browser | access_token + refresh_token na sessão do servidor | Frontend Angular |
| **API** | Bearer token no header Authorization | Nada (stateless) | Recursos de negócio |

---

## O que é OAuth2 e por que usamos?

OAuth2 é um protocolo de autorização que permite que uma aplicação (o BFF) obtenha acesso a recursos em nome de um usuário, sem que esse usuário precise entregar suas credenciais à aplicação.

O fluxo central que usamos é o **Authorization Code Flow com PKCE**:

```
1. BFF diz ao browser: "vá se autenticar no AS"
2. Usuário autentica no AS (página de login hospedada no próprio AS)
3. AS emite um "code" de autorização temporário e redireciona de volta ao BFF
4. BFF troca o "code" por tokens (access_token, refresh_token, id_token)
5. BFF armazena os tokens de forma segura — Angular nunca os vê
```

O **PKCE** (Proof Key for Code Exchange) é uma extensão de segurança: o BFF gera um segredo aleatório antes de iniciar o fluxo e prova que é o mesmo cliente que iniciou quando vai trocar o code. Isso impede que um atacante que intercepte o code consiga trocar por tokens.
