// using Anthropic;
// using BrewCoffee.Authorization.Infrastructure.Options;
// using BrewCoffee.Authorization.Infrastructure.Persistence;
// using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
// using BrewCoffee.Shared.Behaviors;
// using BrewCoffee.Shared.Exceptions;
// using FluentValidation;
// using Microsoft.Agents.AI.Hosting;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.AI;
// using Serilog;
// using Serilog.Events;
//
// namespace BrewCoffee.Authorization.Infrastructure.Configuration;
//
// internal static class BuilderConfiguration
// {
//     extension(WebApplicationBuilder builder)
//     {
//         /// <summary>
//         /// Configura os componentes principais do aplicativo, incluindo documentação da API, opções, agentes, banco de dados,
//         /// medição com validação, logger, gerenciamento de exceções, serviços auxiliares e repositórios.
//         /// </summary>
//         /// <remarks>
//         /// Este método realiza o registro de várias dependências e serviços essenciais utilizados pela aplicação.
//         /// Ele organiza de maneira modular diferentes configurações para facilitar a manutenção e a escalabilidade do sistema.
//         /// </remarks>
//         public void Configure()
//         {
//             builder.ConfigureApiDocumentation();
//             builder.ConfigureOptions();
//             builder.ConfigureAgents();
//             builder.ConfigureAuthentication();
//             builder.ConfigureDbContext();
//             builder.ConfigureMediatorWithValidation();
//             builder.ConfigureLogger();
//             builder.ConfigureExceptionHandling();
//             // builder.ConfigureRepositories();
//         }
//
//         private void ConfigureApiDocumentation()
//         {
//             builder.Services.AddOpenApi();
//         }
//
//         private void ConfigureOptions()
//         {
//             builder.Services
//                 .AddOptions<AgentsOptions>()
//                 .BindConfiguration("Agents")
//                 .ValidateDataAnnotations()
//                 .ValidateOnStart();
//         }
//
//         private void ConfigureAgents()
//         {
//             var agentsOptions = builder.Configuration
//                 .GetRequiredSection("Agents")
//                 .Get<AgentsOptions>()!;
//
//             builder.AddAIAgent(
//                 name: agentsOptions.Name,
//                 instructions: agentsOptions.Instructions,
//                 chatClient: new AnthropicClient { APIKey = agentsOptions.ApiKey }
//                     .AsIChatClient(agentsOptions.Model)
//             );
//         }
//
//         private void ConfigureDbContext()
//         {
//             var isDevelopment = builder.Environment
//                 .IsDevelopment();
//
//             builder.Services.AddDbContext<CoffeeAuthDbContext>(options => options
//                 .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
//                 .UseOpenIddict()
//                 .EnableServiceProviderCaching()
//                 .EnableSensitiveDataLogging(isDevelopment)
//                 .EnableDetailedErrors(isDevelopment));
//         }
//
//         private void ConfigureMediatorWithValidation()
//         {
//             builder.Services.AddMediator(options =>
//             {
//                 options.ServiceLifetime = ServiceLifetime.Scoped;
//                 options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
//             });
//
//             builder.Services.AddValidatorsFromAssembly(
//                 typeof(Program).Assembly,
//                 includeInternalTypes: true
//             );
//         }
//
//         private void ConfigureLogger()
//         {
//             const string logStructure = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
//             Log.Logger = new LoggerConfiguration()
//                 .MinimumLevel.Information()
//                 .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
//                 .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
//                 .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
//                 .WriteTo.Console(outputTemplate: logStructure)
//                 .CreateLogger();
//
//             builder.Logging.ClearProviders();
//             builder.Logging.AddSerilog();
//         }
//
//         private void ConfigureExceptionHandling()
//         {
//             builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//             builder.Services.AddProblemDetails();
//         }
//
//         private void ConfigureAuthentication()
//         {
//             builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
//                 .AddEntityFrameworkStores<CoffeeAuthDbContext>()
//                 .AddDefaultTokenProviders();
//
//             builder.Services.AddOpenIddict()
//                 .AddCore(options => options
//                     .UseEntityFrameworkCore()
//                     .UseDbContext<CoffeeAuthDbContext>()
//                     .ReplaceDefaultEntities<Guid>()
//                 )
//                 .AddServer(options =>
//                 {
//                     options.SetTokenEndpointUris("/connect/token")
//                         .AllowClientCredentialsFlow();
//
//                     options.AddDevelopmentEncryptionCertificate()
//                         .AddDevelopmentSigningCertificate();
//
//                     options.UseAspNetCore()
//                         .EnableTokenEndpointPassthrough();
//                 })
//                 .AddValidation(options =>
//                 {
//                     options.UseLocalServer();
//                     options.UseAspNetCore();
//                 });
//         }
//     }
// }