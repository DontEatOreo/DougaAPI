using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DougaAPI.Download;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Speed;

public sealed class SpeedQuery : QueryBase
{
    [UsedImplicitly]
    [Required]
    [JsonPropertyName("speed")]
    public required double Speed { get; init; }

    [JsonIgnore] public FileStreamResult? DownloadedVideo { get; set; }
}
