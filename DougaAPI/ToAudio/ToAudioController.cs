using DougaAPI.Download;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.ToAudio;

[ApiController]
[Route("[controller]")]
public sealed class ToAudioController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ToAudioController> _logger;

    public ToAudioController(IMediator mediator, ILogger<ToAudioController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromBody] ToAudioQuery query, CancellationToken token)
    {
        _logger.LogInformation("Starting downloading video from {Uri}", query.Uri);
        QueryBase queryBase = new()
        {
            Uri = query.Uri
        };
        var (downloadFile, downloadPaths) = await _mediator.Send(queryBase, token);
        _logger.LogInformation("Finished downloading video from {Uri}", query.Uri);

        _logger.LogInformation("Starting converting video from {Uri} to audio", query.Uri);
        query.DownloadedVideo = downloadFile;
        var (audioFile, toAudioPaths) = await _mediator.Send(query, token);
        _logger.LogInformation("Finished converting video from {Uri} to audio", query.Uri);

        HttpContext.Response.OnCompleted(async () =>
        {
            foreach (var path in downloadPaths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            foreach (var path in toAudioPaths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

            await downloadFile.FileStream.DisposeAsync();
            await audioFile.FileStream.DisposeAsync();
        });
        
        var stream = audioFile.FileStream;
        var contentType = audioFile.ContentType;
        var fileName = audioFile.FileDownloadName;

        return File(stream, contentType, fileName);
    }
}