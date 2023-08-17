using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DougaAPI.Download;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Compress;

public sealed class CompressQuery : QueryBase
{
    [UsedImplicitly]
    [Required]
    [JsonPropertyName("crf")]
    public required int Crf { get; init; }

    [UsedImplicitly]
    [JsonPropertyName("resolution")]
    public string? Resolution { get; init; }

    [JsonIgnore] public FileStreamResult? DownloadedVideo { get; set; }
}
