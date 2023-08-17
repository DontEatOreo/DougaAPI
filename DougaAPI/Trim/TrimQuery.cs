using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DougaAPI.Download;
using JetBrains.Annotations;

namespace DougaAPI.Trim;

public sealed class TrimQuery : QueryBase
{
    [UsedImplicitly]
    [Required]
    [JsonPropertyName("start")] 
    public required float Start { get; init; }

    [UsedImplicitly]
    [Required]
    [JsonPropertyName("end")] 
    public required float End { get; init; }
}
