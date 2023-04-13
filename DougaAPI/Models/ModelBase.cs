using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

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