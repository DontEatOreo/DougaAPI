using DougaAPI.Exceptions;
using DougaAPI.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.StaticFiles;
using Xabe.FFmpeg;

namespace DougaAPI.Services;

[UsedImplicitly]
public class SpeedService
{
    private readonly MediaService _mediaService;
    private readonly FileExtensionContentTypeProvider _provider;

    public SpeedService(FileExtensionContentTypeProvider provider, MediaService mediaService)
    {
        _provider = provider;
        _mediaService = mediaService;
    }

    public async Task<(string path, string contentType)> SpeedAsync(SpeedModel model, CancellationToken token)
    {
        var (path, _) = await _mediaService.DownloadMedia(model, options =>
        {
            options.Output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()[..4], "%(id)s.%(ext)s");
        }, token);

        var mediaInfo = await FFmpeg.GetMediaInfo(path, token);
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

        if (videoStream is null && audioStream is null)
            throw new CustomFileNotFoundException("No streams found.");

        videoStream?.ChangeSpeed(model.Speed);
        videoStream?.SetCodec(VideoCodec.libx264);
        audioStream?.ChangeSpeed(model.Speed);

        var id = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        var folderUuid = Guid.NewGuid().ToString()[..4];
        var outPath = Path.Combine(Path.GetTempPath(), folderUuid, $"{id}{extension}");

        var conversion = FFmpeg.Conversions.New()
            .SetOutput(outPath);

        if (videoStream != null)
            conversion.AddStream(videoStream);

        if (audioStream != null)
            conversion.AddStream(audioStream);

        await conversion.Start(token);

        var contentType = _provider.TryGetContentType(outPath, out var type) ? type : "application/octet-stream";

        return (outPath, contentType);
    }
}