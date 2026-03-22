using BrewCoffee.BFF.Infrastructure.Transformers;
using BrewCoffee.BFF.Shared.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Serilog.Events;

namespace BrewCoffee.BFF.Infrastructure.Setups;

internal static class BuildSetups
{
    extension(WebApplicationBuilder builder)
    {
        public void Configure()
        {
            builder.ConfigureLogger();
            builder.ConfigureApiDocumentation();
            builder.ConfigureAuthentication();
            builder.ConfigureProxy();
            builder.ConfigureHttpClient();
            builder.ConfigureCors();
        }

        private void ConfigureApiDocumentation()
        {
            builder.Services.AddOpenApi();
        }

        private void ConfigureLogger()
        {
            const string logStructure = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.Console(outputTemplate: logStructure)
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
        }

        private void ConfigureAuthentication()
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = "bff.session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = builder.Configuration["Auth:Authority"];
                    options.ClientId = builder.Configuration["Auth:ClientId"];
                    options.ClientSecret = builder.Configuration["Auth:ClientSecret"];

                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.UsePkce = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("api");
                    options.Scope.Add("offline_access");

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.CallbackPath = "/auth/callback";
                    options.SignedOutCallbackPath = "/auth/logout-callback";
#if DEBUG
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
#endif
                });

            builder.Services.AddAuthorization();
        }

        private void ConfigureProxy()
        {
            builder.Services
                .AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                .AddTransforms<TokenTransformer>()
#if DEBUG
                .ConfigureHttpClient((_, handler) => handler
                    .SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true);
#endif
        }

        private void ConfigureHttpClient()
        {
            builder.Services
                .AddHttpClient(HttpClientConstants.Auth, client =>
                    client.BaseAddress = new Uri(builder.Configuration["Auth:Authority"]!))
#if DEBUG
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
#endif
        }

        private void ConfigureCors()
        {
            builder.Services.AddCors(options =>
                options.AddPolicy("Angular", policy => policy
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                )
            );
        }
    }
}