using DougaAPI.Models;
using DougaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CompressController : ControllerBase
{
    private readonly CompressService _service;

    public CompressController(CompressService service)
    {
        _service = service;
    }

    [HttpPost("video")]
    public async Task<IActionResult> Video([FromBody] CompressModel input)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        var (path, contentType) = await _service.CompressVideo(input, cts.Token).ConfigureAwait(false);
        if (path.StartsWith("http"))
            return Ok(path);
        return PhysicalFile(path, contentType, true);
    }

    [HttpPost("audio")]
    public async Task<IActionResult> Audio([FromBody] CompressModel input)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        var (path, contentType) = await _service.CompressAudio(input, cts.Token).ConfigureAwait(false);
        if (path.StartsWith("http"))
            return Ok(path);
        return PhysicalFile(path, contentType, true);
    }
}