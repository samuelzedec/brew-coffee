// using System.Net;
// using System.Net.Http.Json;
// using CoffeeAgent.AcceptanceTests.Infrastructure.Extensions;
// using BrewCoffee.Api.Features.User.CreateUser;
// using FluentAssertions;
// using Reqnroll;
//
// namespace CoffeeAgent.AcceptanceTests.Steps.User;
//
// [Binding]
// public sealed class CreateUserSteps(HttpClient client)
// {
//     private const string Route = "/api/v1/users";
//     private HttpResponseMessage _response = null!;
//
//     [Given("que não existe um usuário cadastrado")]
//     public void DadoQueNaoExisteUsuario()
//     {
//         // banco já está limpo pelo BeforeScenario
//     }
//
//     [Given("que existe um usuário cadastrado com username {string}")]
//     public async Task DadoQueExisteUsuarioComUsername(string username)
//     {
//         var request = new CreateUser(Username: username, Email: "existing@email.com", Password: "Senha@123");
//         await client.PostAsJsonAsync(Route, request);
//     }
//
//     [Given("que existe um usuário cadastrado com email {string}")]
//     public async Task DadoQueExisteUsuarioComEmail(string email)
//     {
//         var request = new CreateUser(Username: "existing_user", Email: email, Password: "Senha@123");
//         await client.PostAsJsonAsync(Route, request);
//     }
//
//     [When("eu enviar uma requisição para criar um usuário com os dados válidos")]
//     public async Task QuandoEnviarRequisicaoComDadosValidos()
//     {
//         var request = new CreateUser(Username: "john_doe", Email: "john@email.com", Password: "Senha@123");
//         _response = await client.PostAsJsonAsync(Route, request);
//     }
//
//     [When("eu enviar uma requisição para criar um usuário com username {string}")]
//     public async Task QuandoEnviarRequisicaoComUsername(string username)
//     {
//         var request = new CreateUser(Username: username, Email: "new@email.com", Password: "Senha@123");
//         _response = await client.PostAsJsonAsync(Route, request);
//     }
//
//     [When("eu enviar uma requisição para criar um usuário com email {string}")]
//     public async Task QuandoEnviarRequisicaoComEmail(string email)
//     {
//         var request = new CreateUser(Username: "new_user", Email: email, Password: "Senha@123");
//         _response = await client.PostAsJsonAsync(Route, request);
//     }
//
//     [Then("devo receber o status {int}")]
//     public void EntaoStatusCode(int statusCode)
//     {
//         _response.StatusCode.Should().Be((HttpStatusCode)statusCode);
//     }
//
//     [Then("a mensagem de erro deve ser {string}")]
//     public async Task EntaoMensagemDeErro(string mensagem)
//     {
//         var error = await _response.ReadErrorAsync();
//         error!.Message.Should().Be(mensagem);
//     }
// }