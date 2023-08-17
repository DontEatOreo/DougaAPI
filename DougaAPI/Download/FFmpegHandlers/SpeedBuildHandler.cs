using DougaAPI.Exceptions;
using DougaAPI.Interfaces;
using DougaAPI.Speed;
using Xabe.FFmpeg;

namespace DougaAPI.Download.FFmpegHandlers;

public sealed class SpeedBuildHandler : IConverter<SpeedQuery>
{
    public IConversion BuildConversion(IEnumerable<IStream> streams, SpeedQuery query, string output)
    {
        var list = streams.ToList();
        var audioStream = list.OfType<IAudioStream>().FirstOrDefault();
        var videoStream = list.OfType<IVideoStream>().FirstOrDefault();

        var conversion = FFmpeg.Conversions.New()
            .SetPreset(ConversionPreset.Fast)
            .SetOutput(output);
        
        if (audioStream is not null)
            conversion.AddStream(audioStream);
        if (videoStream is not null)
            conversion.AddStream(videoStream);
        
        videoStream?.ChangeSpeed(query.Speed);
        audioStream?.ChangeSpeed(query.Speed);

        return conversion;
    }
}
