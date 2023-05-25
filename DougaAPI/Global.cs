using Microsoft.Extensions.Options;
using YoutubeDLSharp;

namespace DougaAPI;

public class Global
{
    public readonly YoutubeDL YoutubeDl;

    public readonly string FormatSort;

    public Global(IOptions<AppSettings> appSettings)
    {
        var ffmpegPath = appSettings.Value.FFmpegPath ?? throw new Exception("Empty ffmpeg path");
        var ytdlpPath = appSettings.Value.YtdlPath ?? throw new Exception("Empty youtube-dl path");
        FormatSort = appSettings.Value.FormatSort ?? throw new Exception("Empty format sort\nExample: \"res:720,ext:mp4\"");

        YoutubeDl = new YoutubeDL
        {
            FFmpegPath = ffmpegPath,
            YoutubeDLPath = ytdlpPath,
            OverwriteFiles = false,
            OutputFileTemplate = "%(id)s.%(ext)s",
            OutputFolder = Path.GetTempPath()
        };
    }
}