using System.Collections.Immutable;
using DougaAPI.Exceptions;
using DougaAPI.Interfaces;
using DougaAPI.ToAudio;
using Xabe.FFmpeg;

namespace DougaAPI.Download.FFmpegHandlers;

public sealed class ToAudioBuildHandler : IConverter<ToAudioQuery>
{
    public IConversion BuildConversion(IEnumerable<IStream> streams, ToAudioQuery query, string output)
    {
        var list = streams.ToImmutableList();
        var audioStream = list.OfType<IAudioStream>().FirstOrDefault();

        if (audioStream is null)
            throw new NoStreams("No audio streams found in the media.");

        audioStream.SetCodec(query.Format);
        var conversion = FFmpeg.Conversions.New()
            .AddStream(audioStream)
            .SetPreset(ConversionPreset.VerySlow)
            .SetOutput(output);

        return conversion;
    }
}
