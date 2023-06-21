using DougaAPI.Exceptions;
using DougaAPI.Models;
using Microsoft.AspNetCore.StaticFiles;
using YoutubeDLSharp.Options;

namespace DougaAPI.Services;

public class MediaService
{
    private readonly Global _global;
    private readonly FileExtensionContentTypeProvider _provider;

    public MediaService(Global global, FileExtensionContentTypeProvider provider)
    {
        _global = global;
        _provider = provider;
    }

    public async Task<(string path, string contentType)> DownloadMedia(ModelBase model,
        Action<OptionSet> configureOptions, CancellationToken token)
    {
        var fetch = await _global.YoutubeDl.RunVideoDataFetch(model.Url.ToString(), token);
        if (fetch.Data is null || fetch.Data.Duration is null or 0 || fetch.Data.IsLive is true)
            throw new CustomInvalidOperationException("Invalid URL");

        OptionSet optionSet = new()
        {
            FormatSort = _global.FormatSort,
            NoPlaylist = true
        };

        configureOptions(optionSet);
        var outPath = Path.GetDirectoryName(optionSet.Output)!;

        var download = await _global.YoutubeDl.RunVideoDownload(model.Url.ToString(),
            overrideOptions: optionSet,
            ct: token);
        if (!download.Success)
            throw new CustomInvalidOperationException("Video download failed");

        var id = fetch.Data.ID;
        var paths = Directory.GetFiles(outPath, $"{id}.*");
        var path = paths.FirstOrDefault(f => _provider.TryGetContentType(f,
                                                 out var s) &&
                                             s.StartsWith("video/") ||
                                             s!.StartsWith("audio/"));
        if (path is null)
            throw new CustomInvalidOperationException("Video file not found");

        var contentType = _provider.TryGetContentType(path, out var type) ? type : "application/octet-stream";

        return (path, contentType);
    }
}