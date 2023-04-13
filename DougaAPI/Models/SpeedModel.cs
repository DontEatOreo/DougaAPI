using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public class SpeedModel : ModelBase
{
    [JsonPropertyName("speed")]
    [Required]
    public double Speed { get; init; }
}