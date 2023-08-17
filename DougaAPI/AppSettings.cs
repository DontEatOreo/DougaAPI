using System.ComponentModel.DataAnnotations;

namespace DougaAPI;

public sealed class AppSettings
{
    [Required] public required string YtdlpPath { get; init; } = null!;
    [Required] public required string FfmpegPath { get; init; } = null!;
    [Required] public required TimeSpan MaxDuration { get; init; }
    [Required] public required TimeSpan MaxTrimTime { get; init; }
    [Required] public required TimeSpan MaxCompressTime { get; init; }
}
