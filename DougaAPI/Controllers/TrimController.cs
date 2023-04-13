using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TrimController : ControllerBase
{
    private readonly Global _global;
    private readonly MediaService _mediaService;

    public TrimController(Global global, MediaService mediaService)
    {
        _global = global;
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
            set.Output = Path.Combine(_global.DownloadPath, folderUuid, "%(id)s.%(ext)s");
        }, cts.Token);

        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size <= model.MaxFileSize)
            return PhysicalFile(path, contentType, Path.GetFileName(path));

        var (filepath, _) = await _global.UploadToServer(path, cts.Token).ConfigureAwait(false);
        return Ok(filepath);
    }
}