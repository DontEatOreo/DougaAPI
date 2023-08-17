using System.Net;
using System.Text.Json;
using DougaAPI.Exceptions;

namespace DougaAPI;

public sealed class Middleware
{
    private readonly RequestDelegate _next;

    public Middleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception e)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            context.Response.StatusCode = e switch
            {
                FailedConversion => (int)HttpStatusCode.InternalServerError,
                FailedDownload => (int)HttpStatusCode.BadRequest,
                FailedFetch => (int)HttpStatusCode.BadRequest,
                MissingFile => (int)HttpStatusCode.NotFound,
                NoStreams => (int)HttpStatusCode.BadRequest,
                VideoTooLong => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = e.Message
            }));
        }
        finally
        {
            context.Response.Body.Close();
        }
    }
}
