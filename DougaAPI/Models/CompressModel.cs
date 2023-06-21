using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DougaAPI.Models;

public sealed record CompressModel(
    [property: JsonPropertyName("ios_compatible")] [property: Required] bool IosCompatible,
    [property: JsonPropertyName("resolution")] string? Resolution,
    [property: JsonPropertyName("crf")] [property: Required] int Crf,
    [property: JsonPropertyName("bitrate")] int? Bitrate,
    Uri Uri,
    int MaxFileSize
) : ModelBase(Uri, MaxFileSize);