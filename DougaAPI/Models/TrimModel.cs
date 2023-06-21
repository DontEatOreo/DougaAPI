using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public sealed record TrimModel(
    [property: JsonPropertyName("start")] [property: Required] [property: Range(0, float.MaxValue)] float Start,
    [property: JsonPropertyName("end")] [property: Required] [property: Range(0, float.MaxValue)] float End,
    Uri Uri,
    int MaxFileSize
) : ModelBase(Uri, MaxFileSize);