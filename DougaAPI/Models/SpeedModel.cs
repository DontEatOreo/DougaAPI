using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public sealed record SpeedModel(
    [property: JsonPropertyName("speed")] [property: Required] double Speed,
    Uri Uri,
    int MaxFileSize
) : ModelBase(Uri, MaxFileSize);