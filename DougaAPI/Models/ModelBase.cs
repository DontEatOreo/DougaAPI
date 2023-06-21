using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public record ModelBase(
    [property: JsonPropertyName("url")] [property: Required] Uri Uri,
    [property: JsonPropertyName("max_file_size")] [property: Required] [property: Range(0, 100)] int MaxFileSize
);