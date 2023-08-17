using DougaAPI.Exceptions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace DougaAPI.Download;

[UsedImplicitly]
public sealed class DownloadHandler : IRequestHandler<QueryBase, ResultBase>
{
    private readonly YoutubeDL _ytDlp;
    private readonly FileExtensionContentTypeProvider _provider;

    private const string FormatSort = "vcodec:h264,ext:mp4:m4a,res:720";
    private readonly TimeSpan _maxDuration;

    public DownloadHandler(FileExtensionContentTypeProvider provider, IOptions<AppSettings> options)
    {
        var ffmpeg = options.Value.FfmpegPath ?? throw new ArgumentException("FFmpeg path is not set.");
        var ytdlp = options.Value.YtdlpPath ?? throw new ArgumentException("YoutubeDL path is not set.");

        _maxDuration = options.Value.MaxDuration;

        _ytDlp = new YoutubeDL
        {
            YoutubeDLPath = ytdlp,
            FFmpegPath = ffmpeg,
            OutputFileTemplate = "%(id)s.%(ext)s"
        };

        _provider = provider;
    }

    public async Task<ResultBase> Handle(QueryBase request, CancellationToken token)
    {
        var uri = request.Uri.ToString();

        var dataFetch = await _ytDlp.RunVideoDataFetch(uri, ct: token);
        CheckDataFetch(dataFetch);

        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()[..6]);
        Directory.CreateDirectory(temp);

        OptionSet optionSet = new()
        {
            NoPlaylist = true,
            FormatSort = FormatSort,
            AudioQuality = 0, // 0 - best quality available, 5 - default, 10 - worst
            Output = Path.Combine(temp, "%(id)s.%(ext)s")
        };

        var download =
            await _ytDlp.RunVideoDownload(uri, overrideOptions: optionSet, ct: token);
        if (download is { Success: false })
            throw new FailedDownload("Failed to download video.");

        var filePath = Directory.GetFiles(temp).FirstOrDefault();
        if (filePath is null)
            throw new MissingFile("Could not find downloaded file.");

        _provider.TryGetContentType(filePath, out var contentType);
        contentType ??= "application/octet-stream";

        FileStream fileStream = new(filePath, FileMode.Open);
        FileStreamResult fileStreamResult = new(fileStream, contentType) { FileDownloadName = Path.GetFileName(filePath) };
        return new ResultBase(fileStreamResult, filePath);
    }

    private void CheckDataFetch(RunResult<VideoData> fetch)
    {
        if (fetch is { Success: false })
            throw new FailedFetch("Failed to fetch video data.");
        if (fetch.Data.IsLive is true)
            throw new FailedFetch("Live videos are not supported.");
        if (float.Parse(_maxDuration.Seconds.ToString()) > _maxDuration.TotalSeconds)
            throw new VideoTooLong($"Video is too long for Download! The maximum allowed time is {_maxDuration}");
    }
}
