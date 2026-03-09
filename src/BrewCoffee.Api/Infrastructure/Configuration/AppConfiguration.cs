// using BrewCoffee.Authorization.Infrastructure.Persistence;
// using Microsoft.EntityFrameworkCore;
// using Scalar.AspNetCore;
// using ZedEndpoints.Extensions;
//
// namespace BrewCoffee.Authorization.Infrastructure.Configuration;
//
// internal static class AppConfiguration
// {
//     extension(WebApplication app)
//     {
//         /// <summary>
//         /// Configura os componentes essenciais da aplicação, incluindo redirecionamento HTTPS,
//         /// documentação da API, agrupamento de endpoints e o manipulador de exceções.
//         /// Este método é responsável por estabelecer os pipelines básicos de execução do aplicativo.
//         /// </summary>
//         public async Task Configure()
//         {
//             if (!app.Environment.IsDevelopment())
//                 app.UseHttpsRedirection();
//
//             app.ConfigureApiDocumentation();
//             app.MapEndpointGroups(globalPrefix: "api/v1");
//             app.UseExceptionHandler();
//         }
//
//         private void ConfigureApiDocumentation()
//         {
//             if (!app.Environment.IsDevelopment())
//                 return;
//
//             app.MapOpenApi();
//             app.MapScalarApiReference();
//         }
//     }
// }