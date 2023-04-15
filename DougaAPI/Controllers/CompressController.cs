using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CompressController : ControllerBase
{
    private readonly Global _global;
    private readonly CompressService _service;

    public CompressController(Global global, CompressService service)
    {
        _global = global;
        _service = service;
    }

    [HttpPost("video")]
    [HttpPost("audio")]
    public async Task<IActionResult> Video([FromBody] CompressModel model)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);

        var route = HttpContext.Request.Path.Value;
        var isAudio = route!.Contains("audio");
        var (path, contentType) = isAudio
            ? await _service.CompressAudio(model, cts.Token).ConfigureAwait(false)
            : await _service.CompressVideo(model, cts.Token).ConfigureAwait(false);

        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size <= model.MaxFileSize)
            return PhysicalFile(path, contentType, Path.GetFileName(path));

        var (filepath, _) = await _global.UploadToServer(path, cts.Token).ConfigureAwait(false);
        return Ok(filepath);
    }
}