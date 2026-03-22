# 06 — Fluxos OAuth2 Completos

> **Referências:**
> - OAuth2 Authorization Code Flow (RFC 6749 §4.1): https://www.rfc-editor.org/rfc/rfc6749#section-4.1
> - OAuth2 Client Credentials (RFC 6749 §4.4): https://www.rfc-editor.org/rfc/rfc6749#section-4.4
> - PKCE (RFC 7636): https://www.rfc-editor.org/rfc/rfc7636
> - OIDC Core spec: https://openid.net/specs/openid-connect-core-1_0.html
> - OAuth 2.1 (draft): https://oauth.net/2.1/

## Fluxo 1: Login com Email/Senha (Authorization Code Flow + PKCE + Razor Pages)

Este é o fluxo usado quando o usuário digita email e senha. A grande diferença para o padrão antigo (Password Grant) é que **o usuário autentica diretamente no AS**, não no Angular. O Angular nunca vê as credenciais.

```
Angular          BFF                  Auth Server (AS)         Cookie AS
  │               │                         │                      │
  │─GET /auth/─── ▶│                         │                      │
  │  login         │                         │                      │
  │                │ Challenge(OpenIdConnect) │                      │
  │◀─ redirect ────│                         │                      │
  │   para AS      │                         │                      │
  │                │                         │                      │
  │─────────────────────────────────────────▶│                      │
  │         GET /connect/authorize           │                      │
  │         ?client_id=bff                   │                      │
  │         &code_challenge=SHA256(verifier) │                      │
  │         &state=...                       │                      │
  │                │                  Sem cookie local              │
  │                │                  Sem cookie externo            │
  │◀─────────────────────────────────────── redirect ──────────────│
  │         para /login?returnUrl=/connect/authorize?...            │
  │                │                         │                      │
  │─────────────────────────────────────────▶│                      │
  │         GET /login (Razor Page)          │                      │
  │         [Usuário vê o form de login]     │                      │
  │                │                         │                      │
  │  POST /login   │                         │                      │
  │  {email, senha}│                         │                      │
  │─────────────────────────────────────────▶│                      │
  │                │                  SignInAsync(ApplicationScheme)─▶
  │◀─────────────────────────────────────── redirect ──────────────│
  │         para /connect/authorize?...      │                      │
  │                │                         │                      │
  │─────────────────────────────────────────▶│                      │
  │         GET /connect/authorize           │                      │
  │                │                  Cookie ApplicationScheme ──────▶
  │                │                  encontra usuário no banco      │
  │                │                  ConfigurePrincipal()           │
  │                │                  SignIn(OpenIddict)             │
  │◀─────────────────────────────────────── redirect ──────────────│
  │         para /auth/callback?code=XYZ     │                      │
  │                │                         │                      │
  │─GET /auth/─────▶│                         │                      │
  │  callback?code  │                         │                      │
  │                 │─POST /connect/token────▶│                      │
  │                 │  code=XYZ               │                      │
  │                 │  code_verifier=...      │                      │
  │                 │  client_id + secret     │                      │
  │                 │                  ValidateCode + VerifyPKCE     │
  │                 │                  IssueTokens                   │
  │                 │◀── access_token ────────│                      │
  │                 │    refresh_token        │                      │
  │                 │    id_token             │                      │
  │                 │                         │                      │
  │                 │─GET /connect/userinfo──▶│                      │
  │                 │  Bearer: access_token   │                      │
  │                 │◀── {sub, email, name} ──│                      │
  │                 │                         │                      │
  │                 │  SetCookie(bff.session)  │                      │
  │◀─ 302 + Cookie ─│                         │                      │
  │   bff.session   │                         │                      │
```

### O que é o PKCE nesse fluxo?

Antes do redirect para o AS, o OpenIdConnect middleware do BFF gera automaticamente:

- `code_verifier`: string aleatória de 128 chars (guardada na sessão temporária)
- `code_challenge`: `Base64Url(SHA-256(code_verifier))` (enviada para o AS)

Quando troca o `code` por tokens, envia o `code_verifier` original. O AS valida que `SHA-256(code_verifier) == code_challenge`. Isso garante que **só o BFF que iniciou o fluxo** consegue trocar o código — mesmo que um atacante intercepte o `code` no redirect.

---

## Fluxo 2: Login Social (Authorization Code Flow + PKCE + Google/Microsoft)

Fluxo usado quando o usuário clica "Entrar com Google" na Razor Page de login do AS.

```
Angular     BFF              Auth Server (AS)          Google
  │          │                      │                     │
  │─GET ─────▶│                      │                     │
  │ /auth/login│                      │                     │
  │            │ Challenge             │                     │
  │◀─redirect ─│                      │                     │
  │            │                      │                     │
  │────────────────────────────────── ▶│                     │
  │    GET /connect/authorize          │                     │
  │    ?client_id=bff&...              │                     │
  │            │               Sem cookies → redirect para   │
  │◀───────────────────────────────── ─│                     │
  │    GET /login?returnUrl=...        │                     │
  │            │                      │                     │
  │────────────────────────────────── ▶│                     │
  │    GET /login (Razor Page)         │                     │
  │    [usuário clica "Login com Google"]                    │
  │            │                      │                     │
  │────────────────────────────────── ▶│                     │
  │    GET /connect/external-login     │                     │
  │    ?provider=Google                │                     │
  │    &returnUrl=/connect/authorize?..│                     │
  │            │               Challenge(Google)             │
  │◀───────────────────────────────── ─│                     │
  │    redirect para Google            │                     │
  │            │                      │                     │
  │─────────────────────────────────────────────────────── ▶│
  │                                   │   Usuário autentica  │
  │◀──────────────────────────────────────────────────────── │
  │    redirect para /connect/external-callback?returnUrl=.. │
  │            │                      │                     │
  │────────────────────────────────── ▶│                     │
  │    GET /connect/external-callback  │                     │
  │            │               redirect para /connect/authorize?...
  │            │                      │                     │
  │────────────────────────────────── ▶│                     │
  │    GET /connect/authorize          │                     │
  │            │               Cookie ExternalScheme presente │
  │            │               FindOrCreateUser               │
  │            │               ConfigurePrincipal()           │
  │            │               SignIn(OpenIddict) → emite code│
  │◀───────────────────────────────── ─│                     │
  │    redirect para /auth/callback?code=XYZ                 │
  │            │                      │                     │
  │─GET ───────▶│                      │                     │
  │ /auth/callback?code                │                     │
  │             │─POST /connect/token─▶│                     │
  │             │  code + code_verifier│                     │
  │             │◀─ tokens ────────────│                     │
  │             │─GET /connect/userinfo▶│                     │
  │             │◀─ {sub, email, name}─│                     │
  │             │  SetCookie(bff.session)                     │
  │◀─ Cookie ───│                      │                     │
```

**Diferença do fluxo de senha:** a autenticação acontece no Google, não na Razor Page do AS. O AS só processa o resultado no `/connect/authorize` após o Google confirmar a identidade.

---

## Fluxo 3: Request à API (após login)

Após o login, o Angular faz requests normais. O BFF intercepta e injeta o token.

```
Angular              BFF                        API (BrewCoffee.Api)
  │                   │                               │
  │──GET /api/xyz─────▶│                               │
  │   Cookie: bff.session                             │
  │                   │                               │
  │           TokenTransformer                        │
  │           GetTokenAsync("access_token")           │
  │           Header: Bearer eyJ...                   │
  │                   │                               │
  │                   │──GET /api/xyz─────────────────▶│
  │                   │   Authorization: Bearer eyJ... │
  │                   │                        ValidateToken (OpenIddict)
  │                   │                        Handle request
  │                   │◀── 200 JSON ───────────────────│
  │◀── 200 JSON ───────│                               │
```

O Angular não sabe que existe um Bearer token — ele só envia o cookie. O Bearer token é injetado pelo BFF de forma transparente.

---

## Fluxo 4: Refresh Token (renovação automática)

O escopo `offline_access` habilita o refresh token. Quando o `access_token` expira, o middleware do BFF pode renovar automaticamente:

```
BFF                         Auth Server
  │                               │
  │──POST /connect/token──────────▶│
  │   grant_type=refresh_token     │
  │   refresh_token=eyJ...         │
  │   client_id + secret           │
  │                          ValidateRefreshToken
  │                          IssueNewTokens
  │◀── new access_token ──────────│
  │    new refresh_token          │
  │    expires_in                 │
  │                               │
  │  Atualiza tokens na sessão    │
  │  (transparente para Angular)  │
```

O Angular não percebe — o BFF atualiza os tokens na sessão e continua proxiando requests com o novo access_token.

---

## Fluxo 5: Logout

```
Angular              BFF                    Auth Server
  │                   │                          │
  │─POST /auth/logout─▶│                          │
  │                   │ SignOut(Cookie)            │
  │                   │ → invalida bff.session     │
  │                   │                          │
  │                   │ SignOut(OpenIdConnect)     │
  │◀─ redirect ───────│ → redireciona para AS      │
  │                   │                          │
  │────────────────────────────────────────────▶│
  │                   GET /connect/end-session   │
  │                   id_token_hint              │
  │                                       SignOutAsync (Identity)
  │                                       SignOut (OpenIddict)
  │◀────────────────────────────────────────────│
  │   redirect para /auth/logout-callback (BFF)  │
  │                   │                          │
  │◀── redirect ──────│                          │
  │   para returnUrl  │                          │
```

Dois sign-outs separados garantem que o usuário fica deslogado em toda a cadeia: BFF (cookie destruído) e AS (sessão e tokens invalidados).

---

## Fluxo 6: M2M (Client Credentials)

Usado pelo Worker Service para chamar a API sem usuário humano. Não envolve browser, cookies nem Razor Pages.

```
Worker Service              Auth Server              API
  │                               │                   │
  │──POST /connect/token──────────▶│                   │
  │   grant_type=client_credentials│                   │
  │   client_id=worker-service     │                   │
  │   client_secret=...            │                   │
  │   scope=api                    │                   │
  │                          ValidateClient            │
  │                          IssueAccessToken          │
  │                          (sem subject — não é user)│
  │◀── access_token ──────────────│                   │
  │    (sem refresh_token)        │                   │
  │                               │                   │
  │──GET /api/xyz──────────────────────────────────── ▶│
  │   Authorization: Bearer eyJ...                    │
  │                                            ValidateToken
  │                                            (sub ausente → é serviço)
  │◀── 200 JSON ───────────────────────────────────── │
```

**Diferenças em relação ao login de usuário:**

| | Login de Usuário | M2M |
|---|---|---|
| Quem autentica | Usuário humano | Serviço/Worker |
| Fluxo | Authorization Code + PKCE | Client Credentials |
| Tem `sub` no token | Sim (ID do usuário) | Não |
| Tem refresh_token | Sim (offline_access) | Não |
| Escopos | openid, email, profile, api | api |

---

## Resumo dos tokens emitidos

| Token | Tamanho | Validade | Onde fica | Para que serve |
|---|---|---|---|---|
| `access_token` | ~500 chars (JWT) | Curta (minutos) | Sessão do BFF | Chamar a API |
| `refresh_token` | opaco | Longa (dias) | Sessão do BFF | Renovar o access_token |
| `id_token` | ~400 chars (JWT) | Curta | Sessão do BFF | Identidade do usuário (OIDC) |
| `bff.session` | cookie | 8h (sliding) | Browser | Autenticar requests ao BFF |
