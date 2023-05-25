using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

#pragma warning disable CS8618
public class ModelBase
{
    [JsonPropertyName("url")]
    [Required]
    [Url]
    public string Url { get; init; }

    [JsonPropertyName("max_file_size")]
    [Required]
    [Range(0, 100)]
    public int MaxFileSize { get; init; }
}
#pragma warning restore CS8618