using System.ComponentModel.DataAnnotations;

namespace BrewCoffee.Api.Infrastructure.Settings;

internal sealed class AgentSettings
{
    [Required] public required string Name { get; set; }
    [Required] public required string ApiKey { get; set; }
    [Required] public required string Model { get; set; }
    [Required] public required string Instructions { get; set; }
}