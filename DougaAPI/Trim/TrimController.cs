using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Trim;

[ApiController]
[Route("[controller]")]
public sealed class TrimController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TrimController> _logger;

    public TrimController(IMediator mediator, ILogger<TrimController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromBody] TrimQuery trimQuery, CancellationToken token)
    {
        _logger.LogInformation("Starting trim of video {Uri}, from {Start} to {End}", trimQuery.Uri, trimQuery.Start, trimQuery.End);
        var (file, paths) = await _mediator.Send(trimQuery, token);
        _logger.LogInformation("Finished trim of video {Uri}, from {Start} to {End}", trimQuery.Uri, trimQuery.Start, trimQuery.End);

        HttpContext.Response.OnCompleted(async () =>
        {
            foreach (var path in paths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

            await file.FileStream.DisposeAsync();
        });
        
        var stream = file.FileStream;
        var contentType = file.ContentType;
        var fileName = file.FileDownloadName;

        return File(stream, contentType, fileName);
    }
}
