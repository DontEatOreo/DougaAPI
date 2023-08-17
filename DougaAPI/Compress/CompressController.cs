using DougaAPI.Download;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Compress;

[ApiController]
[Route("[controller]")]
public sealed class CompressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CompressController> _logger;

    public CompressController(IMediator mediator, ILogger<CompressController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromBody] CompressQuery query, CancellationToken token)
    {
        _logger.LogInformation("Starting downloading video from {Uri}", query.Uri);
        QueryBase download = new() { Uri = query.Uri };
        var (downloadFile, downloadPaths) = await _mediator.Send(download, token);
        _logger.LogInformation("Finished downloading video from {Uri}", query.Uri);

        _logger.LogInformation("Starting compressing video from {Uri}", query.Uri);
        query.DownloadedVideo = downloadFile;
        var (compressFile, compressPaths) = await _mediator.Send(query, token);
        _logger.LogInformation("Finished compressing video from {Uri}", query.Uri);

        HttpContext.Response.OnCompleted(async () =>
        {
            foreach (var path in downloadPaths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            foreach (var path in compressPaths)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

            await downloadFile.FileStream.DisposeAsync();
            await compressFile.FileStream.DisposeAsync();
        });
        
        var stream = compressFile.FileStream;
        var contentType = compressFile.ContentType;
        var fileName = compressFile.FileDownloadName;
        
        return File(stream, contentType, fileName);
    }
}
