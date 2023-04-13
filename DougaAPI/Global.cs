using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using YoutubeDLSharp;

namespace DougaAPI;

public class Global
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FileExtensionContentTypeProvider _provider;
    public readonly YoutubeDL YoutubeDl;

    private readonly string _uploadApiLink;

    public readonly string FormatSort;
    public readonly string DownloadPath = Path.GetTempPath();

    public Global(IHttpClientFactory httpClientFactory,
        FileExtensionContentTypeProvider provider, IOptions<AppSettings> appSettings)
    {
        _httpClientFactory = httpClientFactory;
        _provider = provider;

        var ffmpegPath = appSettings.Value.FFmpegPath;
        var ytdlpPath = appSettings.Value.YtdlPath;
        FormatSort = appSettings.Value.FormatSort;
        _uploadApiLink = appSettings.Value.UploadApiLink;

        YoutubeDl = new YoutubeDL
        {
            FFmpegPath = ffmpegPath,
            YoutubeDLPath = ytdlpPath,
            OverwriteFiles = false,
            OutputFileTemplate = "%(id)s.%(ext)s",
            OutputFolder = DownloadPath
        };
    }

    public async Task<(string filePath, string contentType)> UploadToServer(string path, CancellationToken token)
    {
        await using var fileStream = File.OpenRead(path);
        using MultipartFormDataContent uploadFileRequest = new()
        {
            { new StringContent("fileupload"), "reqtype" },
            { new StringContent("24h"), "time" },
            { new StreamContent(fileStream), "fileToUpload", path }
        };
        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(_uploadApiLink, uploadFileRequest, token).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

        return (responseString, _provider.TryGetContentType(path,
            out var fileContentType)
            ? fileContentType
            : string.Empty);
    }
}