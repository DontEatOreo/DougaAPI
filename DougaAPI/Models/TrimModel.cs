using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public class TrimModel : ModelBase
{
    [JsonPropertyName("start")]
    [Required]
    [Range(0, float.MaxValue)]
    public float Start { get; init; }

    [JsonPropertyName("end")]
    [Required]
    [Range(0, float.MaxValue)]
    public float End { get; init; }
}