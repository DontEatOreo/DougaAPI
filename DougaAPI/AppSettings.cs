using System.ComponentModel.DataAnnotations;

namespace DougaAPI;

public sealed class AppSettings
{
    [Required] public string YtdlpPath { get; init; } = null!;
    [Required] public string FfmpegPath { get; init; } = null!;
    [Required] public TimeSpan MaxDuration { get; init; }
    [Required] public TimeSpan MaxTrimTime { get; init; }
    [Required] public TimeSpan MaxCompressTime { get; init; }
}
