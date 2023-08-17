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

namespace DougaAPI.ToAudio;

[UsedImplicitly]
public sealed class ToAudioHandler : IRequestHandler<ToAudioQuery, ResultBase>
{
    private readonly FileExtensionContentTypeProvider _provider;
    private readonly IConverter<ToAudioQuery> _buildHandler;

    private readonly Dictionary<string, string> _audioExtensions = new()
    {
        { "libopus", ".opus" },
        { "aac", ".m4a" },
        { "mp3", ".mp3" }
    };

    public ToAudioHandler(FileExtensionContentTypeProvider provider, IConverter<ToAudioQuery> buildHandler)
    {
        _provider = provider;
        _buildHandler = buildHandler;
    }

    public async Task<ResultBase> Handle(ToAudioQuery request, CancellationToken token)
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

        if (File.Exists(downloadFile) is false)
            throw new MissingFile("Downloaded video file is missing.");

        var downloadMediaInfo = await FFmpeg.GetMediaInfo(downloadFile, token);

        var audioExtension = _audioExtensions[request.Format.ToLowerInvariant()];

        var audioPath = CreateDirectoryInTempPath();
        var audioFile = Path.Combine(audioPath,
            $"{Path.GetFileNameWithoutExtension(downloadedVideo.FileDownloadName)}{audioExtension}");

        var mediaStreams = downloadMediaInfo.Streams.ToImmutableArray();

        if (mediaStreams.OfType<IVideoStream>().Any() is false)
            throw new NoStreams("No Video streams found.");
        if (mediaStreams.OfType<IAudioStream>().Any() is false)
            throw new NoStreams("No Audio streams found.");
        
        var conversion = _buildHandler.BuildConversion(mediaStreams, request, audioFile);
        try
        {
            await conversion.Start(token);
        }
        catch (ConversionException)
        {
            throw new FailedConversion("Audio conversion failed.");
        }
        catch (FFmpegNotFoundException)
        {
            throw new FailedConversion("Server is missing FFmpeg.");
        }
        if (string.IsNullOrEmpty(audioFile))
            throw new FailedConversion("Audio conversion failed.");

        _ = _provider.TryGetContentType(audioFile, out var contentType);
        if (string.IsNullOrEmpty(contentType))
            contentType = "application/octet-stream";

        FileStream fileStream = new(audioFile, FileMode.Open);
        FileStreamResult fileStreamResult = new(fileStream, contentType) { FileDownloadName = Path.GetFileName(audioFile) };
        return new ResultBase(fileStreamResult, downloadFile, audioFile);
    }

    private static string Uuid() => Guid.NewGuid().ToString()[..4];
    
    private static string CreateDirectoryInTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Uuid());
        Directory.CreateDirectory(path);
        return path;
    }
}