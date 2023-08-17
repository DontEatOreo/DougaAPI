using DougaAPI.Download;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Speed;

[ApiController]
[Route("[controller]")]
public sealed class SpeedController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SpeedController> _logger;

    public SpeedController(IMediator mediator, ILogger<SpeedController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromBody] SpeedQuery query, CancellationToken token)
    {
        _logger.LogInformation("Starting downloading video from {Uri}", query.Uri);
        QueryBase download = new()
        {
            Uri = query.Uri
        };
        var (downloadResult, downloadPaths) = await _mediator.Send(download, token);
        _logger.LogInformation("Finished downloading video from {Uri}", query.Uri);

        query.DownloadedVideo = downloadResult;

        _logger.LogInformation("Starting changing speed of video from {Uri}, with speed {Speed}", query.Uri, query.Speed);
        var (speedFile, speedPaths) = await _mediator.Send(query, token);
        _logger.LogInformation("Finished changing speed of video from {Uri}, with speed {Speed}", query.Uri, query.Speed);

        HttpContext.Response.OnCompleted(async () =>
        {
            foreach (var path in downloadPaths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            foreach (var path in speedPaths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

            await downloadResult.FileStream.DisposeAsync();
            await speedFile.FileStream.DisposeAsync();
        });
        
        var stream = speedFile.FileStream;
        var contentType = speedFile.ContentType;
        var fileName = speedFile.FileDownloadName;

        return File(stream, contentType, fileName);
    }
}
