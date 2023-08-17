using System.Collections.Immutable;
using DougaAPI.Download;
using DougaAPI.Exceptions;
using DougaAPI.Interfaces;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Exceptions;

namespace DougaAPI.Speed;

[UsedImplicitly]
public sealed class SpeedHandler : IRequestHandler<SpeedQuery, ResultBase>
{
    private readonly FileExtensionContentTypeProvider _provider;
    private readonly IConverter<SpeedQuery> _buildHandler;

    public SpeedHandler(FileExtensionContentTypeProvider provider, IConverter<SpeedQuery> buildHandler)
    {
        _provider = provider;
        _buildHandler = buildHandler;
    }

    public async Task<ResultBase> Handle(SpeedQuery request, CancellationToken token)
    {
        var downloadedVideo = request.DownloadedVideo;
        if (downloadedVideo is null)
            throw new MissingFile($"{nameof(request.DownloadedVideo)} is null.");

        // Create path for the downloaded file
        var downloadPath = CreateDirectoryInTempPath();
        var downloadFile = Path.Combine(downloadPath, downloadedVideo.FileDownloadName);

        // Create the downloaded file from the filestream
        await using FileStream fs = new(downloadFile, FileMode.Create);
        await downloadedVideo.FileStream.CopyToAsync(fs, token);
        await fs.FlushAsync(token);

        // Create path for the compressed file
        var speedPath = CreateDirectoryInTempPath();
        var speedFile = Path.Combine(speedPath, downloadedVideo.FileDownloadName);

        var mediaInfo = await FFmpeg.GetMediaInfo(downloadFile, token);
        var streams = mediaInfo.Streams;
        var mediaStreams = streams.ToImmutableArray();

        if (mediaStreams.Any() is false)
            throw new NoStreams("No video stream found in the media.");
        
        var conversion = _buildHandler.BuildConversion(mediaStreams, request, speedFile);
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

        if (string.IsNullOrEmpty(speedFile))
            throw new FailedConversion("Video compression failed.");

        _provider.TryGetContentType(speedFile, out var contentType);
        if (string.IsNullOrEmpty(contentType))
            contentType = "application/octet-stream";

        FileStream stream = new(speedFile, FileMode.Open);
        FileStreamResult fileStreamResult = new(stream, contentType)  { FileDownloadName = Path.GetFileName(speedFile) };
        return new ResultBase(fileStreamResult, downloadPath, speedPath);
    }

    private static string Uuid() => Guid.NewGuid().ToString()[..4];

    private static string CreateDirectoryInTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Uuid());
        Directory.CreateDirectory(path);
        return path;
    }
}
