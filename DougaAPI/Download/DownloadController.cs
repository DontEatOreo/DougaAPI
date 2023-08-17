using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Download;

[ApiController]
[Route("[controller]")]
public sealed class DownloadController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DownloadController> _logger;

    public DownloadController(IMediator mediator, ILogger<DownloadController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromBody] QueryBase query, CancellationToken token)
    {
        _logger.LogInformation("Starting downloading video from {Uri}", query.Uri);
        var (file, paths) = await _mediator.Send(query, token);
        _logger.LogInformation("Finished downloading video from {Uri}", query.Uri);

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
