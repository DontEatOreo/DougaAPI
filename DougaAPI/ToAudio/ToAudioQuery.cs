using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DougaAPI.Download;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.ToAudio;

public sealed class ToAudioQuery : QueryBase
{
    [Required][JsonPropertyName("format")] public required string Format { get; init; }

    [JsonIgnore] public FileStreamResult? DownloadedVideo { get; set; }
}
