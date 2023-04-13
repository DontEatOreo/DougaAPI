using System.Globalization;
using DougaAPI.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.StaticFiles;
using YoutubeDLSharp.Options;

namespace DougaAPI.Services;

[UsedImplicitly]
public class TrimService
{
    private readonly Global _global;
    private readonly FileExtensionContentTypeProvider _provider;

    public TrimService(Global global, FileExtensionContentTypeProvider provider)
    {
        _global = global;
        _provider = provider;
    }

    public async Task<(string FilePath, string ContentType)> Trim(TrimModel model, CancellationToken token)
    {
        var fetch = await _global.YoutubeDl.RunVideoDataFetch(model.Url, token).ConfigureAwait(false);
        if (fetch?.Data.Duration == null || fetch.Data.IsLive == true)
            throw new InvalidOperationException("Invalid video data.");

        var folderUuid = Guid.NewGuid().ToString()[..4];
        var outPath = Path.Combine(_global.DownloadPath, folderUuid);

        var start = model.Start.ToString(CultureInfo.InvariantCulture);
        var end = model.End.ToString(CultureInfo.InvariantCulture);
        OptionSet options = new()
        {
            FormatSort = _global.FormatSort,
            NoPlaylist = true,
            DownloadSections = $"*{start}-{end}",
            ForceKeyframesAtCuts = true,
            Output = Path.Combine(outPath, "%(id)s.%(ext)s")
        };

        var download = await _global.YoutubeDl.RunVideoDownload(model.Url,
                overrideOptions: options,
                ct: token)
            .ConfigureAwait(false);
        if (!download.Success)
            throw new InvalidOperationException("Video download failed.");

        var id = fetch.Data.ID;
        var path = Directory.GetFiles(outPath, $"{id}.*", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (path is null)
            throw new FileNotFoundException("File not found.");

        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size > model.MaxFileSize)
            return await _global.UploadToServer(path, token).ConfigureAwait(false);
        var contentType = _provider.TryGetContentType(path, out var fileContentType) ? fileContentType : string.Empty;
        return (path, contentType);
    }
}