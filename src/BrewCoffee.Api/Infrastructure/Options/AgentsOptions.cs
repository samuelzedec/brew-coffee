using System.ComponentModel.DataAnnotations;

namespace CoffeeAgent.Authorization.Infrastructure.Options;

internal sealed class AgentsOptions
{
    [Required] public required string Name { get; set; }
    [Required] public required string ApiKey { get; set; }
    [Required] public required string Model { get; set; }
    [Required] public required string Instructions { get; set; }
}