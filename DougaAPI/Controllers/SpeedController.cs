using DougaAPI.Clients;
using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeedController : ControllerBase
{
    private readonly ServerClient _serverClient;
    private readonly SpeedService _speedService;

    public SpeedController(ServerClient serverClient, SpeedService speedService)
    {
        _serverClient = serverClient;
        _speedService = speedService;
    }

    [HttpPost]
    public async Task<IActionResult> Speed([FromBody] SpeedModel model)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        var (path, contentType) = await _speedService.SpeedAsync(model, cts.Token);

        var size = new FileInfo(path).Length / 1024 / 1024;
        if (size <= model.MaxFileSize)
            return PhysicalFile(path, contentType, Path.GetFileName(path));

        var uri = await _serverClient.UploadToServer(path, cts.Token);
        return Ok(uri);
    }
}