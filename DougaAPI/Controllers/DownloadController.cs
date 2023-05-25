using DougaAPI.Clients;
using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;
using YoutubeDLSharp.Options;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DownloadController : ControllerBase
{
    private readonly ServerClient _serverClient;
    private readonly MediaService _mediaService;

    public DownloadController(ServerClient serverClient, MediaService mediaService)
    {
        _serverClient = serverClient;
        _mediaService = mediaService;
    }

    [HttpPost("video")]
    [HttpPost("audio")]
    public async Task<IActionResult> DownloadVideo([FromBody] ModelBase model)
    {
        var route = HttpContext.Request.Path.Value;
        var isAudio = route!.Contains("audio");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);

        var (path, contentType) = await _mediaService.DownloadMedia(model, set =>
        {
            if (isAudio)
            {
                set.AudioFormat = AudioConversionFormat.M4a;
                set.ExtractAudio = true;
            }
            set.Output = Path.Combine(Path.GetTempPath(), "%(id)s.%(ext)s");
        }, cts.Token);

        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size <= model.MaxFileSize)
            return PhysicalFile(path, contentType, Path.GetFileName(path));

        var uri = await _serverClient.UploadToServer(path, cts.Token);
        return Ok(uri);
    }
}