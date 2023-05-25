using System.Text.Json.Serialization;

namespace DougaAPI;

public sealed class AppSettings
{
    [JsonPropertyName("ffmpeg_path")]
    public string? FFmpegPath { get; init; }

    [JsonPropertyName("yt_dl_path")]
    public string? YtdlPath { get; init; }

    [JsonPropertyName("format_sort")]
    public string? FormatSort { get; init; }
}