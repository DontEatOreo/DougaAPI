using DougaAPI.Clients;
using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TrimController : ControllerBase
{
    private readonly ServerClient _serverClient;
    private readonly MediaService _mediaService;

    public TrimController(ServerClient serverClient, MediaService mediaService)
    {
        _serverClient = serverClient;
        _mediaService = mediaService;
    }

    [HttpPost]
    public async Task<IActionResult> Trim([FromBody] TrimModel model)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        var (path, contentType) = await _mediaService.DownloadMedia(model, set =>
        {
            set.DownloadSections = $"*{model.Start}-{model.End}";
            set.ForceKeyframesAtCuts = true;
            var folderUuid = Guid.NewGuid().ToString()[..4];
            set.Output = Path.Combine(Path.GetTempPath(), folderUuid, "%(id)s.%(ext)s");
        }, cts.Token);

        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size <= model.MaxFileSize)
            return PhysicalFile(path, contentType, Path.GetFileName(path));

        var uri = await _serverClient.UploadToServer(path, cts.Token);
        return Ok(uri);
    }
}