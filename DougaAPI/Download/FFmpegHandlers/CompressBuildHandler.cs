using System.Collections.Immutable;
using DougaAPI.Compress;
using DougaAPI.Exceptions;
using DougaAPI.Interfaces;
using Xabe.FFmpeg;

namespace DougaAPI.Download.FFmpegHandlers;

public sealed class CompressBuildHandler : IConverter<CompressQuery>
{
    public IConversion BuildConversion(IEnumerable<IStream> streams, CompressQuery query, string output)
    {
        var list = streams.ToImmutableList();
        var videoStream = list.OfType<IVideoStream>().FirstOrDefault();
        var audioStream = list.OfType<IAudioStream>().FirstOrDefault();

        if (videoStream is null)
            throw new NoStreams("No video streams found in the media.");

        var conversion = FFmpeg.Conversions.New()
            .AddStream(videoStream)
            .SetPixelFormat(PixelFormat.yuv420p)
            .SetPreset(ConversionPreset.VerySlow)
            .AddParameter($"-crf {query.Crf}")
            .SetOutput(output);

        if (audioStream is not null)
            conversion.AddStream(audioStream);

        videoStream.SetCodec(VideoCodec.libx264);
        audioStream?.SetCodec(AudioCodec.aac);
        audioStream?.SetBitrate(128);

        if (query.Resolution is null)
            return conversion;

        var valid = ValidateResolution(query.Resolution);
        if (valid is false)
            SetResolution(videoStream, query.Resolution);

        return conversion;
    }

    private static bool ValidateResolution(string resolution)
    {
        if (string.IsNullOrEmpty(resolution) || resolution.Length < 3)
            throw new ArgumentException("Resolution cannot be null or less than 3 characters", nameof(resolution));

        // Validation check for input string format (which should be "Pxxx" where xxx is a resolution integer)
        var noP = resolution[0] is not 'P';
        var noInt = !int.TryParse(resolution[1..], out _);
        if (noP || noInt)
            throw new FormatException(
                "Invalid resolution format. Expected format is Pxxx where xxx is an integer.",
                new ArgumentException(
                    null, nameof(resolution)));
        return true;
    }

    /// <summary>
    /// Sets the resolution of a video stream based on a given resolution string.
    /// </summary>
    /// <param name="stream">The video stream to set the resolution for.</param>
    /// <param name="resolution">The resolution string to use. Expected format is Pxxx where xxx is an integer.</param>
    private static void SetResolution(IVideoStream stream, string resolution)
    {
        var resolutionInt = int.Parse(resolution[1..]);
        double originalWidth = stream.Width;
        double originalHeight = stream.Height;

        // Aspect ratio formula: aspectRatio = originalWidth / originalHeight
        var aspectRatio = originalWidth / originalHeight;

        // Output width formula: 
        // Multiply the aspect ratio by the resolution integer and round to the nearest integer
        // Dividing by 2 and then multiplying by 2 ensures the value is even (since modern video resolutions are often even)
        var outputWidth = (int)(Math.Round(resolutionInt * aspectRatio) / 2) * 2;

        // Output height formula:
        // Dividing the resolution integer by 2 and then multiplying by 2 to make the number even
        var outputHeight = resolutionInt / 2 * 2;

        // Set the new size for the video stream
        stream.SetSize(outputWidth, outputHeight);
    }
}
