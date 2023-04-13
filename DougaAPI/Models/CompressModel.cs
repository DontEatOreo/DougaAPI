using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public class CompressModel : ModelBase
{
    [JsonPropertyName("ios_compatible")]
    [Required]
    public bool IosCompatible { get; init; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("crf")]
    [Required]
    public int Crf { get; init; }

    [JsonPropertyName("bitrate")]
    public int? Bitrate { get; init; }
}