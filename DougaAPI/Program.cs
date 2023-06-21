using System.Text.Json.Serialization;
using DougaAPI;
using DougaAPI.Clients;
using DougaAPI.Exceptions;
using DougaAPI.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using YoutubeDLSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddHttpClient<ServerClient>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = long.MaxValue);

builder.Services.AddSingleton<Global>();
builder.Services.AddSingleton<YoutubeDL>();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<CompressService>();
builder.Services.AddScoped<SpeedService>();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("MySettings"));

var app = builder.Build();
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();