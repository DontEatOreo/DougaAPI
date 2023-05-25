using DougaAPI.Clients;
using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CompressController : ControllerBase
{
    private readonly ServerClient _serverClient;
    private readonly CompressService _service;

    public CompressController(ServerClient serverClient, CompressService service)
    {
        _serverClient = serverClient;
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
            ? await _service.CompressAudio(model, cts.Token)
            : await _service.CompressVideo(model, cts.Token);

        // The reason we divide by 1024 twice is because we want to convert Bytes to MiB
        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size <= model.MaxFileSize)
            return PhysicalFile(path, contentType, Path.GetFileName(path));

        var uri = await _serverClient.UploadToServer(path, cts.Token);
        return Ok(uri);
    }
}