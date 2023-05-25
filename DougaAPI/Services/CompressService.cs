using DougaAPI.Clients;
using DougaAPI.Exceptions;
using DougaAPI.Models;
using Microsoft.AspNetCore.StaticFiles;
using Xabe.FFmpeg;
using YoutubeDLSharp.Options;

namespace DougaAPI.Services;

public class CompressService
{
    private readonly Global _global;
    private readonly ServerClient _serverClient;
    private readonly FileExtensionContentTypeProvider _provider;
    private readonly MediaService _mediaService;

    public CompressService(Global global, ServerClient serverClient, FileExtensionContentTypeProvider provider, MediaService mediaService)
    {
        _global = global;
        _serverClient = serverClient;
        _provider = provider;
        _mediaService = mediaService;
    }

    private readonly string[] _vp9Args = {
        "-row-mt 1",
        "-lag-in-frames 25",
        $"-cpu-used {Environment.ProcessorCount}",
        "-auto-alt-ref 1",
        "-arnr-maxframes 7",
        "-arnr-strength 4",
        "-aq-mode 0",
        "-enable-tpl 1",
        "-row-mt 1"
    };

    public async Task<(string path, string contentType)> CompressVideo(CompressModel model, CancellationToken token)
    {
        var (path, _) = await _mediaService.DownloadMedia(model, VideoDownloadOptions, token);
        var mediaInfo = await FFmpeg.GetMediaInfo(path, token);

        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

        if (videoStream is null)
            throw new InvalidOperationException("Invalid video data.");

        if (audioStream != null)
            ConfigureAudioStream(audioStream, model.Bitrate);
        await ConfigureVideoStream(videoStream, model);

        var extension = Path.GetExtension(path);
        var outputExtension = model.IosCompatible ? ".mp4" : extension;

        var compressedFilePath = await CompressMedia(videoStream, audioStream, path, model, outputExtension, token);

        var contentType = _provider.TryGetContentType(compressedFilePath, out var type)
            ? type
            : "application/octet-stream";

        return (compressedFilePath, contentType);
    }

    public async Task<(string path, string contentType)> CompressAudio(CompressModel model, CancellationToken token)
    {
        var (path, _) = await _mediaService.DownloadMedia(model, AudioDownloadOptions, token);
        var mediaInfo = await FFmpeg.GetMediaInfo(path, token);

        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
        if (audioStream is null)
            throw new CustomInvalidOperationException("Invalid audio data");

        ConfigureAudioStream(audioStream, model.Bitrate);

        var compressedFilePath = await CompressMedia(default, audioStream, path, model, ".m4a", token);
        var contentType = _provider.TryGetContentType(compressedFilePath, out var type)
            ? type
            : "application/octet-stream";
        var size = new FileInfo(compressedFilePath).Length / 1024 / 1024;

        if (size <= model.MaxFileSize)
            return (compressedFilePath, contentType);

        var uri = await _serverClient.UploadToServer(compressedFilePath, token);
        return (uri.ToString(), contentType);
    }

    private void VideoDownloadOptions(OptionSet optionSet)
    {
        optionSet.FormatSort = _global.FormatSort;
        optionSet.NoPlaylist = true;
        optionSet.Output = Path.Combine(Path.GetTempPath(), "%(id)s.%(ext)s");
    }

    private void AudioDownloadOptions(OptionSet optionSet)
    {
        optionSet.FormatSort = _global.FormatSort;
        optionSet.NoPlaylist = true;
        optionSet.AudioFormat = AudioConversionFormat.M4a;
        optionSet.ExtractAudio = true;
        optionSet.Output = Path.Combine(Path.GetTempPath(), "%(id)s.%(ext)s");
    }

    private static void ConfigureAudioStream(IAudioStream audioStream, int? bitrate)
    {
        audioStream.SetBitrate(bitrate ?? 128);
        audioStream.SetCodec(AudioCodec.aac);
    }

    private static async Task ConfigureVideoStream(IVideoStream videoStream, CompressModel model)
    {
        videoStream.SetCodec(model.IosCompatible ? VideoCodec.libx264 : VideoCodec.vp9);
        if (!string.IsNullOrEmpty(model.Resolution) && model.Resolution != "None")
            await SetRes(videoStream, model.Resolution);
    }

    private async Task<string> CompressMedia(IStream? videoStream,
        IStream? audioStream,
        string inputPath,
        CompressModel model,
        string outputExtension,
        CancellationToken token)
    {
        var id = Path.GetFileNameWithoutExtension(inputPath);

        var folderUuid = Guid.NewGuid().ToString()[..4];
        var compressedFilePath = Path.Combine(Path.GetTempPath(), folderUuid, $"{id}{outputExtension}");
        Directory.CreateDirectory(Path.GetDirectoryName(compressedFilePath)!);

        var conversion = FFmpeg.Conversions.New()
            .AddStream(audioStream)
            .SetOutput(compressedFilePath);

        if (videoStream != null)
        {
            conversion.AddStream(videoStream)
                .SetPixelFormat(PixelFormat.yuv420p)
                .AddParameter($"-crf {model.Crf}");
            if (model.IosCompatible)
                conversion.AddParameter(string.Join(" ", _vp9Args));
        }

        await conversion.Start(token);

        if (compressedFilePath is null)
            throw new FileNotFoundException("File not found");

        return compressedFilePath;
    }

    private static ValueTask SetRes(IVideoStream stream, string res)
    {
        double originalWidth = stream.Width;
        double originalHeight = stream.Height;

        // Parse the resolution input string (remove the "p" suffix)
        // P144 -> 144
        var resolutionInt = int.Parse(res[1..]);
        var aspectRatio = originalWidth / originalHeight;

        var outputWidth = (int)Math.Round(resolutionInt * aspectRatio);
        var outputHeight = resolutionInt;

        outputWidth -= outputWidth % 2;
        outputHeight -= outputHeight % 2;
        stream.SetSize(outputWidth, outputHeight);
        return ValueTask.CompletedTask;
    }
}