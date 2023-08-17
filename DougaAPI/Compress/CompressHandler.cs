using System.Collections.Immutable;
using DougaAPI.Download;
using DougaAPI.Download.FFmpegHandlers;
using DougaAPI.Exceptions;
using DougaAPI.Interfaces;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Exceptions;

namespace DougaAPI.Compress;

[UsedImplicitly]
public sealed class CompressHandler : IRequestHandler<CompressQuery, ResultBase>
{
    private readonly FileExtensionContentTypeProvider _provider;
    private readonly IConverter<CompressQuery> _buildHandler;

    private readonly TimeSpan _maxCompressTime;

    public CompressHandler(FileExtensionContentTypeProvider provider,
        CompressBuildHandler buildHandler,
        IOptions<AppSettings> options)
    {
        _provider = provider;
        _buildHandler = buildHandler;
        _maxCompressTime = options.Value.MaxCompressTime;
    }

    public async Task<ResultBase> Handle(CompressQuery request, CancellationToken token)
    {
        var downloadedVideo = request.DownloadedVideo;
        if (downloadedVideo is null)
            throw new MissingFile($"{nameof(request.DownloadedVideo)} is null.");
        var downloadFileName = Path.GetFileNameWithoutExtension(downloadedVideo.FileDownloadName);

        var extension = GetExtension(downloadedVideo.ContentType);

        // Create path for the downloaded file
        var downloadPath = CreateDirectoryInTempPath();
        var downloadFile = Path.Combine(downloadPath, $"{downloadFileName}{extension}");

        // Create file from the stream and flushing it from memory
        await using var file = File.Create(downloadFile);
        await downloadedVideo.FileStream.CopyToAsync(file, token);
        await file.FlushAsync(token);

        if (File.Exists(downloadFile) is false)
            throw new MissingFile("Downloaded file is missing.");

        var downloadMediaInfo = await FFmpeg.GetMediaInfo(downloadFile, token);
        var belowMaxTime = downloadMediaInfo.Duration < _maxCompressTime;
        if (belowMaxTime is false)
            throw new VideoTooLong($"Video is too long for compression! The maximum allowed time is {_maxCompressTime}");

        // Create path for the compressed file
        var compressPath = CreateDirectoryInTempPath();
        var compressFile = Path.Combine(compressPath, $"{downloadFileName}.mp4");

        var mediaInfo = await FFmpeg.GetMediaInfo(downloadFile, token);
        var mediaStreams = mediaInfo.Streams.ToImmutableArray();
        
        if (mediaStreams.OfType<IVideoStream>().Any() is false)
            throw new NoStreams("No video stream found in the media.");

        var conversion = _buildHandler.BuildConversion(mediaStreams, request, compressFile);
        try
        {
            await conversion.Start(token);
        }
        catch (ConversionException)
        {
            throw new FailedConversion("Video compression failed.");
        }
        catch (FFmpegNotFoundException)
        {
            throw new FailedConversion("Server is missing FFmpeg.");
        }

        if (string.IsNullOrEmpty(compressFile))
            throw new FailedConversion("Video compression failed.");

        _ = _provider.TryGetContentType(compressFile, out var contentType);
        if (string.IsNullOrEmpty(contentType))
            contentType = "application/octet-stream";

        FileStream fileStream = new(compressFile, FileMode.Open);
        FileStreamResult fileStreamResult = new(fileStream, contentType) { FileDownloadName = Path.GetFileName(compressFile) };

        return new ResultBase(fileStreamResult, downloadPath, compressPath);
    }

    private static string Uuid() => Guid.NewGuid().ToString()[..4];

    private static string CreateDirectoryInTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Uuid());
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetExtension(string s)
    {
        var extension = s[(s.IndexOf('/') + 1)..];
        extension = $".{extension}";
        return extension;
    }
}
