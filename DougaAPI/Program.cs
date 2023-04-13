using System.Text.Json.Serialization;
using DougaAPI;
using DougaAPI.Exceptions;
using DougaAPI.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using YoutubeDLSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = long.MaxValue);

builder.Services.AddSingleton<Global>();
builder.Services.AddSingleton<YoutubeDL>();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<CompressService>();
builder.Services.AddScoped<SpeedService>();
builder.Services.AddScoped<TrimService>();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public class AppSettings
{
    public AppSettings(string fFmpegPath, string ytdlPath, string formatSort, string uploadApiLink)
    {
        FFmpegPath = fFmpegPath;
        YtdlPath = ytdlPath;
        FormatSort = formatSort;
        UploadApiLink = uploadApiLink;
    }

    public string FFmpegPath { get; set; }
    public string YtdlPath { get; set; }
    public string FormatSort { get; set; }
    public string UploadApiLink { get; set; }
}