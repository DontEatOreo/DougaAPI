using DougaAPI;
using DougaAPI.Compress;
using DougaAPI.Download.FFmpegHandlers;
using DougaAPI.Interfaces;
using DougaAPI.Speed;
using DougaAPI.ToAudio;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging.Console;
using YoutubeDLSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(o => o.RegisterServicesFromAssemblyContaining<Program>());

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddTransient<FileExtensionContentTypeProvider>();
builder.Services.AddTransient<YoutubeDL>();

builder.Services.AddTransient<IConverter<CompressQuery>, CompressBuildHandler>();
builder.Services.AddTransient<IConverter<SpeedQuery>, SpeedBuildHandler>();
builder.Services.AddTransient<IConverter<ToAudioQuery>, ToAudioBuildHandler>();

builder.Services.AddTransient<CompressBuildHandler>();
builder.Services.AddTransient<SpeedBuildHandler>();
builder.Services.AddTransient<ToAudioBuildHandler>();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Settings"));

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.UseMiddleware<Middleware>();
app.Run();
