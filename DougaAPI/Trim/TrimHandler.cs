using DougaAPI.Download;
using DougaAPI.Exceptions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace DougaAPI.Trim;

[UsedImplicitly]
public sealed class TrimHandler : IRequestHandler<TrimQuery, ResultBase>
{
    private readonly YoutubeDL _ytDlp;
    private readonly FileExtensionContentTypeProvider _provider;

    private const string FormatSort = "vcodec:h264,ext:mp4:m4a,res:720";
    private readonly TimeSpan _maxTrimDuration;

    public TrimHandler(FileExtensionContentTypeProvider provider, IOptions<AppSettings> options)
    {
        var ffmpeg = options.Value.FfmpegPath ?? throw new Exception();
        var ytdlp = options.Value.YtdlpPath ?? throw new Exception();

        _maxTrimDuration = options.Value.MaxTrimTime;

        _ytDlp = new YoutubeDL
        {
            YoutubeDLPath = ytdlp,
            FFmpegPath = ffmpeg,
            OutputFileTemplate = "%(id)s.%(ext)s"
        };

        _provider = provider;
    }

    public async Task<ResultBase> Handle(TrimQuery request, CancellationToken token)
    {
        var uri = request.Uri.ToString();
        var start = request.Start;
        var end = request.End;

        var totalTime = TimeSpan.FromSeconds(end - start);
        var belowTime = totalTime < _maxTrimDuration;
        if (belowTime is false)
            throw new VideoTooLong($"Video is too long for Trimming! The maximum allowed time is {_maxTrimDuration}");

        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()[..4]);
        Directory.CreateDirectory(temp);

        var fetch = await _ytDlp.RunVideoDataFetch(uri, ct: token);
        CheckDataFetch(fetch);

        OptionSet optionSet = new()
        {
            NoPlaylist = true,
            FormatSort = FormatSort,
            ForceKeyframesAtCuts = true,
            DownloadSections = $"*{start}-{end}",
            Output = Path.Combine(temp, "%(title)s.%(ext)s"),
        };

        var download = await _ytDlp.RunVideoDownload(uri, overrideOptions: optionSet, ct: token);

        if (download is { Success: false })
            throw new FailedDownload("Failed to download video.");
        var filePath = Directory.GetFiles(temp).FirstOrDefault();
        if (filePath is null)
            throw new MissingFile("Could not find downloaded file.");

        _provider.TryGetContentType(filePath, out var contentType);
        contentType ??= "application/octet-stream";

        await using FileStream fileStream = new(filePath, FileMode.Open);
        FileStreamResult fileStreamResult = new(fileStream, contentType) { FileDownloadName = Path.GetFileName(filePath) };
        return new ResultBase(fileStreamResult, temp);
    }

    private void CheckDataFetch(RunResult<VideoData> fetch)
    {
        if (fetch is { Success: false })
            throw new FailedFetch("Failed to fetch video data.");
        if (fetch.Data.IsLive is true)
            throw new FailedFetch("Live videos are not supported.");
        if (float.Parse(_maxTrimDuration.Seconds.ToString()) > _maxTrimDuration.TotalSeconds)
            throw new VideoTooLong($"Video is too long for Trimming! The maximum allowed time is {_maxTrimDuration}");
    }
}
